using System;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.SelfBet.Data;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Presenter.Network
{
    /// <summary>
    /// El presentador en red (GDD 9.1). El host observa la partida y los comodines, decide cuándo hablar y qué frase
    /// decir (por evento, sin repetir las últimas); todos reciben el mismo índice de frase y sus variables y la formatean
    /// localmente. Habla solo en momentos relevantes, nunca por cada interacción menor.
    /// </summary>
    public sealed class PresenterSessionNetwork : NetworkBehaviour
    {
        [SerializeField] private PresenterConfig _config;
        [SerializeField] private PresenterCatalog _catalog;
        [SerializeField] private PresenterRuntime _runtime;
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private SelfBetRuntime _selfBet;

        private readonly NetworkVariable<PresenterLineNet> _line = new(new PresenterLineNet { Phrase = PresenterLineNet.SILENT });

        // ---- Solo en el host ----
        private PhraseSelector _selector;
        private MatchPhase _lastPhase = MatchPhase.Lobby;
        private int _lastQuestion;
        private int _lastBetCount;
        private float _bettingSince;
        private float _lastLineTime = float.NegativeInfinity;
        private float _lastSelfBetLine = float.NegativeInfinity;
        private bool _timeWarningGiven;
        private int _lastLeftCount;

        // ---- En todos ----
        private float _silenceAt;
        private bool _showPending;

        public override void OnNetworkSpawn()
        {
            _line.OnValueChanged += OnLineChanged;
            if (IsServer)
            {
                _selector = new PhraseSelector(_catalog.BuildIndexByEvent(), new System.Random(Environment.TickCount));
                _match.Changed += OnMatchChanged;
                _selfBet.Changed += OnSelfBetChanged;
            }
            Show(_line.Value);
        }

        public override void OnNetworkDespawn()
        {
            _line.OnValueChanged -= OnLineChanged;
            if (IsServer)
            {
                _match.Changed -= OnMatchChanged;
                _selfBet.Changed -= OnSelfBetChanged;
            }
            _runtime.Say(string.Empty);
        }

        private void Update()
        {
            // La frase recibida por red se muestra aquí, fuera de la lectura del paquete.
            if (_showPending)
            {
                _showPending = false;
                Show(_line.Value);
            }

            // Todos: callar cuando pasa el tiempo de la frase.
            if (_silenceAt > 0f && Time.time >= _silenceAt)
            {
                _silenceAt = 0f;
                _runtime.Say(string.Empty);
            }

            if (!IsServer || _lastPhase != MatchPhase.Betting || _match.Session == null)
                return;

            var session = _match.Session;
            if (session.IsBettingTimed)
            {
                // Con tiempo: un aviso cuando quedan pocos segundos.
                if (!_timeWarningGiven && session.PhaseSecondsLeft <= _config.TimeWarningSeconds)
                {
                    _timeWarningGiven = true;
                    Speak(PresenterEvent.TimeRunningOut, CurrentVariables());
                }
                return;
            }

            // Sin tiempo: comentario si la discusión se alarga (no fuerza el cierre).
            var now = Time.time;
            if (now - _bettingSince >= _config.LongDiscussionAfter && now - _lastLineTime >= _config.LongDiscussionRepeat)
                Speak(PresenterEvent.LongDiscussion, CurrentVariables());
        }

        // ---- Host: cuándo hablar ----

        private void OnMatchChanged()
        {
            var session = _match.Session;
            if (session == null || (session.Phase == _lastPhase && session.QuestionNumber == _lastQuestion))
                return;

            var previous = _lastPhase;
            _lastPhase = session.Phase;
            _lastQuestion = session.QuestionNumber;

            switch (session.Phase)
            {
                case MatchPhase.Betting:
                    _bettingSince = Time.time;
                    _lastBetCount = 0;
                    _timeWarningGiven = false;
                    if (previous == MatchPhase.Lobby)
                        Speak(PresenterEvent.Welcome, CurrentVariables());
                    else if (PresenterMoments.TryQuestionStart(session.QuestionNumber, session.TotalQuestions,
                                 session.CurrentQuestion != null && session.CurrentQuestion.IsMath, out var questionEvent))
                        Speak(questionEvent, CurrentVariables());
                    break;
                case MatchPhase.Locked:
                    Speak(PresenterEvent.Countdown, CurrentVariables());
                    break;
                case MatchPhase.Reveal:
                    var round = session.LastRound;
                    Speak(PresenterMoments.Reveal(!string.IsNullOrEmpty(session.LastEliminated), round.Lost, round.Kept, round.Bonus),
                        CurrentVariables());
                    break;
                case MatchPhase.Victory:
                    Speak(PresenterEvent.Victory, CurrentVariables());
                    break;
                case MatchPhase.Defeat:
                    Speak(PresenterEvent.Defeat, CurrentVariables());
                    break;
                case MatchPhase.Lobby:
                    _lastLeftCount = 0;
                    Silence();
                    break;
            }
        }

        private void OnSelfBetChanged()
        {
            var bets = _selfBet.Session;
            if (bets == null)
                return;

            // Alguien se ha ido a mitad de partida.
            var left = 0;
            foreach (var bettor in bets.Book.Bettors)
            {
                if (bettor.HasLeft)
                    left++;
            }
            var someoneLeft = left > _lastLeftCount;
            _lastLeftCount = left;
            if (someoneLeft && !MatchRules.IsFinished(_lastPhase) && _lastPhase != MatchPhase.Lobby)
            {
                Speak(PresenterEvent.PlayerLeft, CurrentVariables());
                return;
            }

            if (_lastPhase != MatchPhase.Betting)
                return;

            var count = 0;
            foreach (var bettor in bets.Book.Bettors)
            {
                if (bettor.HasBet)
                    count++;
            }

            var someoneNewBet = count > _lastBetCount;
            _lastBetCount = count;
            if (someoneNewBet && Time.time - _lastSelfBetLine >= _config.SelfBetCooldown)
            {
                _lastSelfBetLine = Time.time;
                Speak(PresenterEvent.SelfBet, CurrentVariables());
            }
        }

        private PhraseVariables CurrentVariables()
        {
            var session = _match.Session;
            var bets = _selfBet.Session;
            var survivors = 0;
            if (bets != null)
            {
                foreach (var bettor in bets.Book.Bettors)
                {
                    if (bettor.IsActive)
                        survivors++;
                }
            }

            return session == null
                ? default
                : new PhraseVariables(session.LastRound.Lost, session.TeamMoney, survivors, session.QuestionNumber,
                    Math.Max(1, session.LastMultiplier));
        }

        private void Speak(PresenterEvent presenterEvent, in PhraseVariables variables)
        {
            var phrase = _selector.Next(presenterEvent);
            if (phrase < 0)
                return;

            _lastLineTime = Time.time;
            _line.Value = new PresenterLineNet
            {
                Phrase = (ushort)phrase,
                Sequence = (ushort)(_line.Value.Sequence + 1),
                Amount = variables.Amount,
                Prize = variables.Prize,
                Survivors = (byte)variables.Survivors,
                Round = (byte)variables.Round,
                Multiplier = (byte)variables.Multiplier,
            };
        }

        private void Silence() =>
            _line.Value = new PresenterLineNet { Phrase = PresenterLineNet.SILENT, Sequence = (ushort)(_line.Value.Sequence + 1) };

        // ---- Todos: mostrar ----

        private void OnLineChanged(PresenterLineNet previous, PresenterLineNet current) => _showPending = true;

        private void Show(in PresenterLineNet line)
        {
            if (line.Phrase == PresenterLineNet.SILENT || line.Phrase >= _catalog.Count)
            {
                _silenceAt = 0f;
                _runtime.Say(string.Empty);
                return;
            }

            var variables = new PhraseVariables(line.Amount, line.Prize, line.Survivors, line.Round, line.Multiplier);
            _runtime.Say(variables.Format(_catalog[line.Phrase].Text));
            _silenceAt = Time.time + _config.LineSeconds;
        }
    }
}
