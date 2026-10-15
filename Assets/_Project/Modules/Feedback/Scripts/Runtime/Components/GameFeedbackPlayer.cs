using System;
using System.Collections.Generic;
using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Data;
using Trivias.SelfBet.Data;
using Trivias.Shared.Settings;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Pone sonido y música al juego observando la partida, el dinero y los comodines (sin tocar su lógica).
    /// Los avisos de red solo marcan qué cambió; el sonido se decide en <see cref="LateUpdate"/>, con el paquete ya
    /// aplicado. Lo único que se mira cada frame es el reloj, y solo en las fases con tiempo.
    /// </summary>
    public sealed class GameFeedbackPlayer : MonoBehaviour
    {
        [SerializeField] private FeedbackLibrary _library;
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private MoneyRuntime _money;
        [SerializeField] private SelfBetRuntime _selfBet;
        [Tooltip("Volúmenes del jugador (general, música, efectos).")]
        [SerializeField] private GameSettings _settings;
        [SerializeField] private AudioSource _sfx;
        [Tooltip("Dos fuentes para el fundido entre pistas.")]
        [SerializeField] private AudioSource[] _musicSources = new AudioSource[2];

        private readonly List<FeedbackCue> _cues = new(8);
        private MatchSnapshot _lastMatch = MatchSnapshot.Lobby;
        private int _lastCarried;
        private int _lastBets;
        private bool _matchDirty = true;
        private bool _moneyDirty = true;
        private bool _betsDirty = true;
        private int _activeMusic;

        /// <summary>Se lanza cada vez que suena algo (lo usan las pruebas).</summary>
        public event Action<FeedbackCue> CuePlayed;

        /// <summary>Bloque de música que toca ahora (0 fácil, 1 media, 2 difícil) o <see cref="FeedbackRules.NO_MUSIC"/>.</summary>
        public int CurrentMusicBlock { get; private set; } = FeedbackRules.NO_MUSIC;

        private void OnEnable()
        {
            _match.Changed += OnMatchChanged;
            _money.Changed += OnMoneyChanged;
            _selfBet.Changed += OnBetsChanged;
            _settings.Changed += ApplyMasterVolume;
            ApplyMasterVolume();
        }

        private void OnDisable()
        {
            _match.Changed -= OnMatchChanged;
            _money.Changed -= OnMoneyChanged;
            _selfBet.Changed -= OnBetsChanged;
            _settings.Changed -= ApplyMasterVolume;
        }

        /// <summary>El volumen general afecta a todo lo que suena en el juego.</summary>
        private void ApplyMasterVolume() => AudioListener.volume = _settings.MasterVolume;

        private void OnMatchChanged() => _matchDirty = true;
        private void OnMoneyChanged(int index) => _moneyDirty = true;
        private void OnBetsChanged() => _betsDirty = true;

        private void LateUpdate()
        {
            var session = _match.Session;
            var phase = session?.Phase ?? MatchPhase.Lobby;
            var timed = phase == MatchPhase.Locked || (phase == MatchPhase.Betting && session.IsBettingTimed);

            if (_matchDirty || timed)
                UpdateMatch(session);
            if (_moneyDirty)
                UpdateMoney();
            if (_betsDirty)
                UpdateBets(phase);

            FadeMusic(Time.unscaledDeltaTime);
        }

        private void UpdateMatch(IMatchSession session)
        {
            _matchDirty = false;
            var current = MatchSnapshots.From(session);

            _cues.Clear();
            FeedbackRules.MatchCues(_lastMatch, current, _cues);
            _lastMatch = current;
            for (var i = 0; i < _cues.Count; i++)
                Play(_cues[i]);

            var block = session == null
                ? FeedbackRules.NO_MUSIC
                : FeedbackRules.MusicBlock(current.Phase, current.Question, session.TotalQuestions);
            if (block != CurrentMusicBlock)
                SwitchMusic(block);
        }

        private void UpdateMoney()
        {
            _moneyDirty = false;
            var session = _money.Session;
            var carried = session == null ? 0 : session.Ledger.CountCarriedBy(session.LocalClientId);
            if (FeedbackRules.TryMoneyCue(_lastCarried, carried, session == null || session.IsLocked, out var cue))
                Play(cue);
            _lastCarried = carried;
        }

        private void UpdateBets(MatchPhase phase)
        {
            _betsDirty = false;
            var session = _selfBet.Session;
            var bets = 0;
            if (session != null)
            {
                var bettors = session.Book.Bettors;
                for (var i = 0; i < bettors.Count; i++)
                {
                    if (bettors[i].HasBet && bettors[i].IsActive)
                        bets++;
                }
            }

            if (FeedbackRules.IsNewBet(_lastBets, bets, phase))
                Play(FeedbackCue.BetPlaced);
            _lastBets = bets;
        }

        private void Play(FeedbackCue cue)
        {
            var clip = _library.Clip(cue);
            if (clip != null)
                _sfx.PlayOneShot(clip, _library.Volume(cue) * _settings.EffectsVolume);
            CuePlayed?.Invoke(cue);
        }

        // ---- Música: la pista nueva entra mientras la anterior se apaga ----

        private void SwitchMusic(int block)
        {
            CurrentMusicBlock = block;
            var clip = _library.Music(block);
            _activeMusic = 1 - _activeMusic;
            var incoming = _musicSources[_activeMusic];
            incoming.clip = clip;
            incoming.volume = 0f;
            if (clip != null)
                incoming.Play();
            else
                incoming.Stop();
        }

        private void FadeMusic(float deltaTime)
        {
            var musicVolume = _library.MusicVolume * _settings.MusicVolume;
            var step = deltaTime * _library.MusicVolume / _library.MusicFadeSeconds;
            for (var i = 0; i < _musicSources.Length; i++)
            {
                var source = _musicSources[i];
                var target = i == _activeMusic && source.clip != null ? musicVolume : 0f;
                source.volume = Mathf.MoveTowards(source.volume, target, step);
                if (target == 0f && source.volume <= 0f && source.isPlaying)
                    source.Stop();
            }
        }
    }
}
