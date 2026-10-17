using TMPro;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Presenter.Components
{
    /// <summary>
    /// Rótulo de televisión del presentador, abajo a la izquierda (como un «lower third»): su cara en un círculo,
    /// «PRESENTADOR» y la frase escribiéndose letra a letra. Siempre del mismo tamaño y en el mismo sitio, se lee mires
    /// donde mires y nunca tapa el plató. El ánimo solo cambia el color del nombre y de su raya: sin botes ni temblores.
    /// Por fotograma solo trabaja mientras escribe o se va.
    /// </summary>
    public sealed class PresenterCaptionView : MonoBehaviour
    {
        private const float OUTRO_SECONDS = 0.2f;

        [SerializeField] private PresenterRuntime _runtime;
        [SerializeField] private PresenterConfig _config;
        [Tooltip("La tarjeta entera (se muestra u oculta).")]
        [SerializeField] private GameObject _card;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _accent;
        [SerializeField] private TextMeshProUGUI _label;
        [Tooltip("Color del nombre y de su raya por ánimo, en el orden de PresenterMood.")]
        [SerializeField] private Color[] _moodColors = new Color[PresenterMoods.COUNT];

        private PresenterMood _mood;
        private float _hideStartedAt;
        private bool _hiding;
        private int _total;
        private int _visible;
        private float _budget;
        private float _charsPerSecond;

        public PresenterMood Mood => _mood;
        public int VisibleCharacters => _visible;
        public int TotalCharacters => _total;
        public bool IsShowing => _card.activeSelf && !_hiding;
        public string Text => _label.text;
        public Color AccentColor => _name.color;

        private void OnEnable()
        {
            _runtime.Spoke += Show;
            Show(_runtime.CurrentLine);
        }

        private void OnDisable() => _runtime.Spoke -= Show;

        private void Show(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                if (_card.activeSelf && !_hiding)
                {
                    _hiding = true;
                    _hideStartedAt = Time.time;
                }
                return;
            }

            _mood = _runtime.Mood;
            var color = _moodColors[(int)_mood];
            _name.color = color;
            _accent.color = color;
            _label.text = line;
            _label.maxVisibleCharacters = 0;
            _hiding = false;
            _group.alpha = 1f;
            _card.SetActive(true);
            _label.ForceMeshUpdate();
            _total = _label.textInfo.characterCount;
            _visible = 0;
            _budget = 0f;
            _charsPerSecond = SpeechRhythm.CharsPerSecond(_mood, _total, _config.LineSeconds);
        }

        private void Update()
        {
            if (!_card.activeSelf)
                return;

            if (_hiding)
            {
                var left = 1f - (Time.time - _hideStartedAt) / OUTRO_SECONDS;
                if (left <= 0f)
                {
                    _hiding = false;
                    _card.SetActive(false);
                    return;
                }
                _group.alpha = left;
                return;
            }

            if (_visible >= _total)
                return;
            // Saca las letras que tocan este fotograma, con sus pausas en los signos.
            _budget += Time.deltaTime;
            var characters = _label.textInfo.characterInfo;
            while (_visible < _total && _budget >= 0f)
            {
                _budget -= 1f / _charsPerSecond + SpeechRhythm.PauseAfter(characters[_visible].character, _mood);
                _visible++;
            }
            _label.maxVisibleCharacters = _visible;
        }
    }
}
