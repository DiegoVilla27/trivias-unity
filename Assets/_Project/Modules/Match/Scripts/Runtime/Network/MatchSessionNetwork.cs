using System;
using System.Collections.Generic;
using System.Text;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using Trivias.SelfBet.Data;
using Trivias.Shared.Diagnostics;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Players;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Match.Network
{
    /// <summary>
    /// Partida en red. El host elige las preguntas, avanza las fases y resuelve el dinero; los clientes solo reciben
    /// el estado (índice de pregunta en el banco y orden de opciones). La respuesta correcta no se envía hasta revelarla.
    /// </summary>
    public sealed class MatchSessionNetwork : NetworkBehaviour, IMatchSession
    {
        private const int NO_ANSWER = -1;

        [SerializeField] private MatchConfig _config;
        [SerializeField] private QuestionBank _bank;
        [Tooltip("Historial de preguntas vistas del grupo (mismo objeto de red).")]
        [SerializeField] private QuestionHistoryNetwork _questionHistory;
        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private MoneyRuntime _money;
        [SerializeField] private VoidEventChannel _matchStartRequested;
        [SerializeField] private PlayerControlGate _playerControl;
        [SerializeField] private SelfBetRuntime _selfBet;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [Tooltip("Petición local de verificar desde una zona de apuesta (todos los activos apostados).")]
        [SerializeField] private VoidEventChannel _verifyFromZoneRequested;
        [Tooltip("Se lanza en el host al volver a la sala de espera; Networking reabre la sala.")]
        [SerializeField] private VoidEventChannel _returnedToLobby;

        private readonly NetworkVariable<MatchPhase> _phase = new(MatchPhase.Lobby);
        private readonly NetworkVariable<byte> _questionNumber = new();
        private readonly NetworkVariable<ushort> _questionIndex = new();
        private readonly NetworkVariable<byte> _optionOrder = new(OptionOrder.Identity.Packed);
        private readonly NetworkVariable<sbyte> _correctBooth = new(NO_ANSWER);
        private readonly NetworkVariable<double> _phaseEndTime = new();
        private readonly NetworkVariable<RoundSummaryNet> _lastRound = new();
        private readonly NetworkVariable<MatchResultNet> _result = new();

        private int[] _plan; // Solo en el host.
        private double _startTime; // Solo en el host: inicio común del concurso (GDD 12.2).
        private uint _matchId;     // Solo en el host: identifica la partida para entregar las fichas una vez.
        private int _firstQuestion = 1; // Solo en el host: pregunta con la que empezó (récords solo si es la 1).
        private readonly List<ulong> _eliminatedBuffer = new();
        private readonly List<ulong> _participants = new();
        private bool _notifyPending;

        // ---- IMatchSession ----

        public MatchPhase Phase => _phase.Value;
        public int QuestionNumber => _questionNumber.Value;
        public int TotalQuestions => _config.QuestionsPerMatch;
        public QuestionData CurrentQuestion => _questionNumber.Value == 0 ? null : _bank[_questionIndex.Value];
        public OptionOrder Order => new(_optionOrder.Value);
        public int CorrectBooth => _correctBooth.Value;
        public RoundOutcome LastRound => _lastRound.Value.ToOutcome();
        public int LastMultiplier => _lastRound.Value.Multiplier;
        public string LastEliminated => _lastRound.Value.Eliminated.ToString();

        public MatchResult Result
        {
            get
            {
                var result = _result.Value;
                return new MatchResult(result.MatchId, result.Victory, result.QuestionsPassed, result.FinalMoney, result.DurationSeconds,
                    result.TokensEach, result.Survivors.ToString(), result.Eliminated.ToString(), result.StartingPlayers,
                    result.StartedAtFirstQuestion);
            }
        }
        public long TeamMoney => _money.Session?.Ledger.TotalInPlay() ?? 0;

        public long MoneyAtRisk
        {
            get
            {
                var ledger = _money.Session?.Ledger;
                if (ledger == null || !_config.LoseUnplacedMoney)
                    return 0;
                long total = ledger.TotalAt(MoneyPlace.Table, 0);
                for (var i = 0; i < ledger.Count; i++)
                {
                    if (ledger.Items[i].Place == MoneyPlace.Carried)
                        total += ledger.ValueOf(i);
                }
                return total;
            }
        }

        public bool IsBettingTimed => _config.HasBettingTimer;

        public float PhaseSecondsLeft
        {
            get
            {
                var phase = _phase.Value;
                var timed = phase == MatchPhase.Locked || phase == MatchPhase.Reveal ||
                            (phase == MatchPhase.Betting && IsBettingTimed);
                if (!timed)
                    return 0f;
                var left = _phaseEndTime.Value - NetworkManager.ServerTime.Time;
                return left > 0 ? (float)left : 0f;
            }
        }

        public void RequestVerify() => VerifyRpc();

        public void RequestReturnToLobby() => ReturnToLobbyRpc();

        // ---- Ciclo de red ----

        public override void OnNetworkSpawn()
        {
            _phase.OnValueChanged += OnPhaseChanged;
            _questionNumber.OnValueChanged += OnByteChanged;
            _questionIndex.OnValueChanged += OnUShortChanged;
            _optionOrder.OnValueChanged += OnByteChanged;
            _correctBooth.OnValueChanged += OnSByteChanged;
            _lastRound.OnValueChanged += OnRoundChanged;
            _result.OnValueChanged += OnResultChanged;
            _money.Changed += OnMoneyChanged;
            _selfBet.Changed += OnSelfBetChanged;
            _verifyFromZoneRequested.Register(RequestVerify);

            if (IsServer)
            {
                _matchStartRequested.Register(StartMatch);
                NetworkManager.OnClientDisconnectCallback += OnClientLeft;
            }

            _runtime.Attach(this);
            _notifyPending = true;
        }

        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseChanged;
            _questionNumber.OnValueChanged -= OnByteChanged;
            _questionIndex.OnValueChanged -= OnUShortChanged;
            _optionOrder.OnValueChanged -= OnByteChanged;
            _correctBooth.OnValueChanged -= OnSByteChanged;
            _lastRound.OnValueChanged -= OnRoundChanged;
            _result.OnValueChanged -= OnResultChanged;
            _money.Changed -= OnMoneyChanged;
            _selfBet.Changed -= OnSelfBetChanged;
            _verifyFromZoneRequested.Unregister(RequestVerify);

            if (IsServer)
            {
                _matchStartRequested.Unregister(StartMatch);
                NetworkManager.OnClientDisconnectCallback -= OnClientLeft;
            }

            _runtime.Detach(this);
            _playerControl.SetCanMove(false);
        }

        // ---- Host ----

        private void Update()
        {
            // Solo el host comprueba los plazos; los clientes calculan el tiempo restante al dibujar.
            if (IsServer)
                TickDeadlines();
        }

        /// <summary>
        /// Avisa a la interfaz una vez por frame, cuando ya han llegado todas las variables del paquete
        /// (fase, número de pregunta, orden...). Así nunca se dibuja un estado a medias y un error de interfaz
        /// no puede interrumpir la lectura de la red.
        /// </summary>
        private void LateUpdate()
        {
            if (!_notifyPending)
                return;

            _notifyPending = false;
            // Solo se juega durante las preguntas: en la sala de espera y en los resultados nadie se mueve.
            var phase = _phase.Value;
            _playerControl.SetCanMove(phase == MatchPhase.Betting || phase == MatchPhase.Locked || phase == MatchPhase.Reveal);
            _runtime.NotifyChanged();
        }

        private void TickDeadlines()
        {
            var phase = _phase.Value;
            var timed = phase == MatchPhase.Locked || phase == MatchPhase.Reveal ||
                        (phase == MatchPhase.Betting && _config.HasBettingTimer);
            if (!timed || NetworkManager.ServerTime.Time < _phaseEndTime.Value)
                return;

            switch (phase)
            {
                case MatchPhase.Betting:
                    // Se acabó el tiempo: se verifica sola con lo que haya en las bandejas.
                    LockBets("tiempo agotado");
                    break;
                case MatchPhase.Locked:
                    Reveal();
                    break;
                default:
                    AdvanceAfterReveal();
                    break;
            }
        }

        private void StartMatch()
        {
            if (_phase.Value != MatchPhase.Lobby)
                return;

            // El reparto de comodines se fija con el tamaño inicial del grupo (GDD 5.1).
            _participants.Clear();
            _participants.AddRange(NetworkManager.ConnectedClientsIds);

            var difficulties = new int[_bank.Count];
            for (var i = 0; i < _bank.Count; i++)
                difficulties[i] = _bank[i].Difficulty;

            // Primero las preguntas que nadie del grupo ha visto, sin dos seguidas de la misma categoría.
            var random = new System.Random(Environment.TickCount);
            _plan = QuestionPlanner.Plan(difficulties, _bank.CategoryNumbers(), _questionHistory.GroupPenalties(_participants),
                _config.QuestionsPerMatch, random);

            _selfBet.Authority.StartMatch(_participants);
            _startTime = NetworkManager.ServerTime.Time;
            _matchId = (uint)random.Next(1, int.MaxValue);
            var first = MatchRules.StartQuestion(_config.TestStartQuestion, _plan.Length);
            _firstQuestion = first;
            if (first > 1)
                GameLog.Warning($"[Partida] MODO PRUEBA: empieza en la pregunta {first} de {_plan.Length} " +
                                 "(MatchConfig → «Empezar en la pregunta»; vuelve a 1 para jugar normal).", this);
            GameLog.Info($"[Partida] Empieza con {_plan.Length} preguntas.", this);
            ShowQuestion(first, random);
        }

        [Rpc(SendTo.Server)]
        private void VerifyRpc(RpcParams rpcParams = default)
        {
            var sender = rpcParams.Receive.SenderClientId;
            if (_phase.Value != MatchPhase.Betting || _avatars.IsEliminated(sender))
                return;

            // Un apostado solo puede verificar (desde su zona) si no queda nadie libre para pulsar el botón.
            var bets = _selfBet.Authority;
            if (bets.HasBet(sender) && !bets.AllActiveHaveBet)
                return;

            LockBets($"jugador {sender}");
        }

        /// <summary>Bloqueo inmediato (GDD 4.3): desde aquí no se acepta ningún cambio de dinero ni de comodines.</summary>
        private void LockBets(string reason)
        {
            _money.Authority?.SetLocked(true);
            _selfBet.Authority.LockBets();
            _phaseEndTime.Value = NetworkManager.ServerTime.Time + _config.LockCountdownSeconds;
            _phase.Value = MatchPhase.Locked;
            GameLog.Info($"[Partida] Verificado ({reason}).", this);
        }

        private void Reveal()
        {
            var question = CurrentQuestion;
            var correctBooth = Order.BoothOf(question.CorrectOption);

            // Comodines: multiplicador = 1 + apostados en la correcta; los apostados en otras cabinas quedan expulsados.
            _eliminatedBuffer.Clear();
            var multiplier = _selfBet.Authority.Resolve(correctBooth, _eliminatedBuffer);
            var outcome = _money.Authority.ResolveRound(correctBooth, multiplier, _config.LoseUnplacedMoney);

            _lastRound.Value = RoundSummaryNet.From(outcome, multiplier, NamesOf(_eliminatedBuffer));
            _correctBooth.Value = (sbyte)correctBooth;
            _phaseEndTime.Value = NetworkManager.ServerTime.Time + _config.RevealSeconds;
            _phase.Value = MatchPhase.Reveal;
        }

        private void AdvanceAfterReveal()
        {
            var next = MatchRules.AfterReveal(_lastRound.Value.Total, _selfBet.Authority.ActivePlayers,
                _questionNumber.Value, _config.QuestionsPerMatch);
            if (next == MatchPhase.Betting)
            {
                ShowQuestion(_questionNumber.Value + 1, new System.Random(Environment.TickCount));
                return;
            }

            Finish(next);
        }

        private void Finish(MatchPhase outcome)
        {
            _money.Authority?.SetLocked(true);
            _selfBet.Authority.SetOpen(false);
            _result.Value = BuildResult(outcome == MatchPhase.Victory);
            _phase.Value = outcome;
            GameLog.Info($"[Partida] Fin: {outcome} · {MatchResult.FormatDuration(_result.Value.DurationSeconds)}.", this);
        }

        /// <summary>
        /// Un jugador se desconecta a mitad de partida (GDD 10): su comodín se anula y deja de contar como activo
        /// (el dinero que llevaba ya vuelve a la mesa en el módulo Money). Si no queda nadie activo, es derrota.
        /// </summary>
        private void OnClientLeft(ulong client)
        {
            var phase = _phase.Value;
            if (client == NetworkManager.ServerClientId || phase == MatchPhase.Lobby || MatchRules.IsFinished(phase))
                return;

            _selfBet.Authority.RemovePlayer(client);
            GameLog.Info($"[Partida] El jugador {client} se ha ido.", this);
            if (_selfBet.Authority.ActivePlayers == 0)
                Finish(MatchPhase.Defeat);
        }

        /// <summary>Tiempo total = fin − inicio, con todo lo que pasó entre medias (GDD 12.2).</summary>
        private MatchResultNet BuildResult(bool victory)
        {
            var phase = victory ? MatchPhase.Victory : MatchPhase.Defeat;
            var passed = MatchRules.QuestionsPassed(phase, _questionNumber.Value);
            var tokens = MatchRules.TokensFor(passed, victory, _config.TokenTierMinQuestions, _config.TokenTierRewards,
                _config.QuestionsPerMatch);

            var survivors = new StringBuilder();
            var eliminated = new StringBuilder();
            foreach (var bettor in _selfBet.Session.Book.Bettors)
            {
                if (bettor.HasLeft)
                    continue; // Quien se fue no es superviviente ni expulsado.
                var names = bettor.IsEliminated ? eliminated : survivors;
                names.Append(names.Length > 0 ? ", " : string.Empty).Append(_avatars.NameOf(bettor.ClientId));
            }

            var result = new MatchResultNet
            {
                MatchId = _matchId,
                Victory = victory,
                QuestionsPassed = (byte)passed,
                FinalMoney = _money.Authority?.TotalInPlay ?? _lastRound.Value.Total,
                DurationSeconds = (float)(NetworkManager.ServerTime.Time - _startTime),
                TokensEach = (short)tokens,
                StartingPlayers = (byte)_participants.Count,
                StartedAtFirstQuestion = _firstQuestion == 1,
            };
            result.Survivors.CopyFromTruncated(survivors.ToString());
            result.Eliminated.CopyFromTruncated(eliminated.ToString());
            return result;
        }

        /// <summary>Volver a jugar: todo a la sala de espera (dinero, comodines, concursantes y sala abierta).</summary>
        [Rpc(SendTo.Server)]
        private void ReturnToLobbyRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || !MatchRules.IsFinished(_phase.Value))
                return;

            _money.Authority.ResetForNewMatch();
            _selfBet.Authority.ResetForNewMatch();
            foreach (var avatar in _avatars.All)
                avatar.Respawn();

            _questionNumber.Value = 0;
            _correctBooth.Value = NO_ANSWER;
            _lastRound.Value = default;
            _result.Value = default;
            _phase.Value = MatchPhase.Lobby;
            _returnedToLobby.Raise();
            GameLog.Info("[Partida] De vuelta en la sala de espera.", this);
        }

        private void ShowQuestion(int number, System.Random random)
        {
            _questionIndex.Value = (ushort)_plan[number - 1];
            _optionOrder.Value = OptionOrder.Shuffle(random).Packed;
            _correctBooth.Value = NO_ANSWER;
            _questionNumber.Value = (byte)number;
            _money.Authority?.SetLocked(false);
            _selfBet.Authority.SetOpen(true);
            _phaseEndTime.Value = _config.HasBettingTimer ? NetworkManager.ServerTime.Time + _config.BettingSeconds : 0;
            _phase.Value = MatchPhase.Betting;
        }

        private string NamesOf(List<ulong> clients)
        {
            if (clients.Count == 0)
                return string.Empty;

            var names = new StringBuilder();
            foreach (var client in clients)
                names.Append(names.Length > 0 ? ", " : string.Empty).Append(_avatars.NameOf(client));
            return names.ToString();
        }

        // ---- Avisos de cambio (toda la interfaz se actualiza por eventos) ----

        private void OnPhaseChanged(MatchPhase previous, MatchPhase current) => _notifyPending = true;
        private void OnByteChanged(byte previous, byte current) => _notifyPending = true;
        private void OnUShortChanged(ushort previous, ushort current) => _notifyPending = true;
        private void OnSByteChanged(sbyte previous, sbyte current) => _notifyPending = true;
        private void OnRoundChanged(RoundSummaryNet previous, RoundSummaryNet current) => _notifyPending = true;
        private void OnResultChanged(MatchResultNet previous, MatchResultNet current) => _notifyPending = true;
        private void OnMoneyChanged(int _) => _notifyPending = true;
        private void OnSelfBetChanged() => _notifyPending = true;
    }
}
