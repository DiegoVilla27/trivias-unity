using System;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using Trivias.Shared.Diagnostics;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
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
        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private MoneyRuntime _money;
        [SerializeField] private VoidEventChannel _matchStartRequested;
        [SerializeField] private PlayerControlGate _playerControl;

        private readonly NetworkVariable<MatchPhase> _phase = new(MatchPhase.Lobby);
        private readonly NetworkVariable<byte> _questionNumber = new();
        private readonly NetworkVariable<ushort> _questionIndex = new();
        private readonly NetworkVariable<byte> _optionOrder = new(OptionOrder.Identity.Packed);
        private readonly NetworkVariable<sbyte> _correctBooth = new(NO_ANSWER);
        private readonly NetworkVariable<double> _phaseEndTime = new();
        private readonly NetworkVariable<RoundSummaryNet> _lastRound = new();

        private int[] _plan; // Solo en el host.
        private bool _notifyPending;

        // ---- IMatchSession ----

        public MatchPhase Phase => _phase.Value;
        public int QuestionNumber => _questionNumber.Value;
        public int TotalQuestions => _config.QuestionsPerMatch;
        public QuestionData CurrentQuestion => _questionNumber.Value == 0 ? null : _bank[_questionIndex.Value];
        public OptionOrder Order => new(_optionOrder.Value);
        public int CorrectBooth => _correctBooth.Value;
        public RoundOutcome LastRound => _lastRound.Value.ToOutcome();
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

        public float PhaseSecondsLeft
        {
            get
            {
                if (_phase.Value != MatchPhase.Locked && _phase.Value != MatchPhase.Reveal)
                    return 0f;
                var left = _phaseEndTime.Value - NetworkManager.ServerTime.Time;
                return left > 0 ? (float)left : 0f;
            }
        }

        public void RequestVerify() => VerifyRpc();

        // ---- Ciclo de red ----

        public override void OnNetworkSpawn()
        {
            _phase.OnValueChanged += OnPhaseChanged;
            _questionNumber.OnValueChanged += OnByteChanged;
            _questionIndex.OnValueChanged += OnUShortChanged;
            _optionOrder.OnValueChanged += OnByteChanged;
            _correctBooth.OnValueChanged += OnSByteChanged;
            _lastRound.OnValueChanged += OnRoundChanged;
            _money.Changed += OnMoneyChanged;

            if (IsServer)
                _matchStartRequested.Register(StartMatch);

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
            _money.Changed -= OnMoneyChanged;

            if (IsServer)
                _matchStartRequested.Unregister(StartMatch);

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
            _playerControl.SetCanMove(_phase.Value != MatchPhase.Lobby);
            _runtime.NotifyChanged();
        }

        private void TickDeadlines()
        {
            var phase = _phase.Value;
            if ((phase != MatchPhase.Locked && phase != MatchPhase.Reveal) ||
                NetworkManager.ServerTime.Time < _phaseEndTime.Value)
                return;

            if (phase == MatchPhase.Locked)
                Reveal();
            else
                AdvanceAfterReveal();
        }

        private void StartMatch()
        {
            if (_phase.Value != MatchPhase.Lobby)
                return;

            var difficulties = new int[_bank.Count];
            for (var i = 0; i < _bank.Count; i++)
                difficulties[i] = _bank[i].Difficulty;

            var random = new System.Random(Environment.TickCount);
            _plan = QuestionPlanner.Plan(difficulties, _config.QuestionsPerMatch, random);
            GameLog.Info($"[Partida] Empieza con {_plan.Length} preguntas.", this);
            ShowQuestion(1, random);
        }

        [Rpc(SendTo.Server)]
        private void VerifyRpc(RpcParams rpcParams = default)
        {
            if (_phase.Value != MatchPhase.Betting)
                return;

            // Bloqueo inmediato: desde aquí no se acepta ningún cambio de dinero (GDD 4.3).
            _money.Authority?.SetLocked(true);
            _phaseEndTime.Value = NetworkManager.ServerTime.Time + _config.LockCountdownSeconds;
            _phase.Value = MatchPhase.Locked;
            GameLog.Info($"[Partida] Verificado por el jugador {rpcParams.Receive.SenderClientId}.", this);
        }

        private void Reveal()
        {
            var question = CurrentQuestion;
            var correctBooth = Order.BoothOf(question.CorrectOption);
            // Sin comodines todavía: multiplicador x1 (hito 3 añade los apostados).
            var outcome = _money.Authority.ResolveRound(correctBooth, multiplier: 1, _config.LoseUnplacedMoney);

            _lastRound.Value = RoundSummaryNet.From(outcome);
            _correctBooth.Value = (sbyte)correctBooth;
            _phaseEndTime.Value = NetworkManager.ServerTime.Time + _config.RevealSeconds;
            _phase.Value = MatchPhase.Reveal;
        }

        private void AdvanceAfterReveal()
        {
            var next = MatchRules.AfterReveal(_lastRound.Value.Total, _questionNumber.Value, _config.QuestionsPerMatch);
            if (next == MatchPhase.Betting)
            {
                ShowQuestion(_questionNumber.Value + 1, new System.Random(Environment.TickCount));
                return;
            }

            _phase.Value = next;
            GameLog.Info($"[Partida] Fin: {next}.", this);
        }

        private void ShowQuestion(int number, System.Random random)
        {
            _questionIndex.Value = (ushort)_plan[number - 1];
            _optionOrder.Value = OptionOrder.Shuffle(random).Packed;
            _correctBooth.Value = NO_ANSWER;
            _questionNumber.Value = (byte)number;
            _money.Authority?.SetLocked(false);
            _phase.Value = MatchPhase.Betting;
        }

        // ---- Avisos de cambio (toda la interfaz se actualiza por eventos) ----

        private void OnPhaseChanged(MatchPhase previous, MatchPhase current) => _notifyPending = true;
        private void OnByteChanged(byte previous, byte current) => _notifyPending = true;
        private void OnUShortChanged(ushort previous, ushort current) => _notifyPending = true;
        private void OnSByteChanged(sbyte previous, sbyte current) => _notifyPending = true;
        private void OnRoundChanged(RoundSummaryNet previous, RoundSummaryNet current) => _notifyPending = true;
        private void OnMoneyChanged(int _) => _notifyPending = true;
    }
}
