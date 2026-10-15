using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Ambiente de luz del plató: los focos de colores, la luz ambiente y la luz principal cambian con suavidad según el
    /// momento (bloque de dificultad, cuenta atrás, acierto, fallo, victoria). La luz principal se obtiene de la escena
    /// (<see cref="RenderSettings.sun"/>) y todo se restaura al desactivarse.
    /// </summary>
    public sealed class StageLighting : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private StageMoodConfig _moods;
        [SerializeField] private Light[] _spots = new Light[0];

        private Color _baseSky;
        private Color _baseEquator;
        private Color _baseGround;
        private Light _keyLight;
        private float _baseKeyIntensity;

        private MoodLight _current;
        private bool _dirty = true;

        public StageMood Mood { get; private set; } = StageMood.Lobby;

        /// <summary>Color actual de los focos (lo usan las pruebas).</summary>
        public Color SpotColor => _current.SpotColor;

        private void OnEnable()
        {
            _baseSky = RenderSettings.ambientSkyColor;
            _baseEquator = RenderSettings.ambientEquatorColor;
            _baseGround = RenderSettings.ambientGroundColor;
            _keyLight = RenderSettings.sun;
            _baseKeyIntensity = _keyLight != null ? _keyLight.intensity : 0f;

            _current = _moods.For(StageMood.Lobby);
            Apply(1f);
            _match.Changed += OnMatchChanged;
        }

        private void OnDisable()
        {
            _match.Changed -= OnMatchChanged;
            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = _baseSky;
                RenderSettings.ambientEquatorColor = _baseEquator;
                RenderSettings.ambientGroundColor = _baseGround;
            }
            if (_keyLight != null)
                _keyLight.intensity = _baseKeyIntensity;
        }

        private void OnMatchChanged() => _dirty = true;

        private void LateUpdate()
        {
            var session = _match.Session;
            if (_dirty || Mood == StageMood.Countdown)
            {
                _dirty = false;
                var snapshot = MatchSnapshots.From(session);
                Mood = FeedbackRules.MoodFor(snapshot, session?.TotalQuestions ?? 0);
            }

            var target = _moods.For(Mood);
            var blend = 1f - Mathf.Exp(-_moods.FadeSpeed * Time.deltaTime);
            _current.SpotColor = Color.Lerp(_current.SpotColor, target.SpotColor, blend);
            _current.SpotIntensity = Mathf.Lerp(_current.SpotIntensity, target.SpotIntensity, blend);
            _current.AmbientTint = Color.Lerp(_current.AmbientTint, target.AmbientTint, blend);
            _current.KeyLightScale = Mathf.Lerp(_current.KeyLightScale, target.KeyLightScale, blend);

            var pulse = Mood == StageMood.Countdown && session != null
                ? 1f + _moods.CountdownPulse * FeedbackRules.CountdownPulse(session.PhaseSecondsLeft)
                : 1f;
            Apply(pulse);
        }

        private void Apply(float pulse)
        {
            for (var i = 0; i < _spots.Length; i++)
            {
                _spots[i].color = _current.SpotColor;
                _spots[i].intensity = _current.SpotIntensity * pulse;
            }

            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = _baseSky * _current.AmbientTint;
                RenderSettings.ambientEquatorColor = _baseEquator * _current.AmbientTint;
                RenderSettings.ambientGroundColor = _baseGround * _current.AmbientTint;
            }
            if (_keyLight != null)
                _keyLight.intensity = _baseKeyIntensity * _current.KeyLightScale;
        }
    }
}
