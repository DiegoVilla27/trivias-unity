using TMPro;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Presenter.Components
{
    /// <summary>
    /// Viñeta de cómic del presentador, junto a su cabeza: aparece con un rebote, escribe la frase letra a letra (cada
    /// letra salta al aparecer) y cambia de forma, color y movimiento según el ánimo de la frase. Solo se anima mientras
    /// se ve; al callar, se encoge y desaparece.
    /// </summary>
    public sealed class PresenterBubbleView : MonoBehaviour
    {
        private const float INTRO_SECONDS = 0.28f;
        private const float OUTRO_SECONDS = 0.16f;
        private const int MAX_CHARACTERS = 512;

        [SerializeField] private PresenterRuntime _runtime;
        [SerializeField] private PresenterConfig _config;
        [Tooltip("Lo que se muestra u oculta (la viñeta entera, que gira hacia la cámara).")]
        [SerializeField] private GameObject _container;
        [Tooltip("Lo que rebota, late y se inclina.")]
        [SerializeField] private RectTransform _body;
        [SerializeField] private Image _shape;
        [SerializeField] private Image _shadow;
        [SerializeField] private Image _tail;
        [SerializeField] private Image _rays;
        [SerializeField] private TextMeshProUGUI _label;
        [Tooltip("Un estilo por ánimo, en el orden de PresenterMood.")]
        [SerializeField] private BubbleStyle[] _styles = new BubbleStyle[PresenterMoods.COUNT];

        private readonly float[] _revealTimes = new float[MAX_CHARACTERS];
        private PresenterMood _mood;
        private float _shownAt;
        private float _hideStartedAt;
        private bool _hiding;
        private int _total;
        private int _visible;
        private float _budget;
        private float _charsPerSecond;

        public PresenterMood Mood => _mood;
        public int VisibleCharacters => _visible;
        public int TotalCharacters => _total;
        public bool IsShowing => _container.activeSelf && !_hiding;

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
                if (_container.activeSelf && !_hiding)
                {
                    _hiding = true;
                    _hideStartedAt = Time.time;
                }
                return;
            }

            _mood = _runtime.Mood;
            ApplyStyle(_styles[(int)_mood]);
            _label.text = line;
            _label.maxVisibleCharacters = 0;
            _container.SetActive(true);
            _label.ForceMeshUpdate();
            _total = Mathf.Min(_label.textInfo.characterCount, MAX_CHARACTERS);
            _visible = 0;
            _budget = 0f;
            _hiding = false;
            _shownAt = Time.time;
            _charsPerSecond = SpeechRhythm.CharsPerSecond(_mood, _total, _config.LineSeconds);
        }

        private void ApplyStyle(in BubbleStyle style)
        {
            _shape.sprite = style.Shape;
            _shape.color = style.Fill;
            _shadow.sprite = style.Shape;
            _tail.color = style.Fill;
            _label.color = style.Text;
            _rays.gameObject.SetActive(style.Rays);
            _rays.color = style.RaysColor;
        }

        private void Update()
        {
            if (!_container.activeSelf)
                return;

            var now = Time.time;
            if (_hiding)
            {
                var left = 1f - (now - _hideStartedAt) / OUTRO_SECONDS;
                if (left <= 0f)
                {
                    _hiding = false;
                    _container.SetActive(false);
                    return;
                }
                _body.localScale = Vector3.one * left;
                return;
            }

            var intro = SpeechRhythm.IntroScale((now - _shownAt) / INTRO_SECONDS);
            SpeechRhythm.BubbleWobble(_mood, now, out var wobble, out var degrees);
            _body.localScale = Vector3.one * (intro * wobble);
            _body.localRotation = Quaternion.Euler(0f, 0f, degrees);
            if (_rays.gameObject.activeSelf)
                _rays.rectTransform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(now * 8f));

            Type(now);
            AnimateLetters(now);
        }

        /// <summary>Saca las letras que tocan este fotograma (con sus pausas en los signos).</summary>
        private void Type(float now)
        {
            if (_visible >= _total)
                return;
            _budget += Time.deltaTime;
            var characters = _label.textInfo.characterInfo;
            while (_visible < _total && _budget >= 0f)
            {
                _revealTimes[_visible] = now;
                _budget -= 1f / _charsPerSecond + SpeechRhythm.PauseAfter(characters[_visible].character, _mood);
                _visible++;
            }
            _label.maxVisibleCharacters = _visible;
        }

        /// <summary>Cada letra salta al aparecer y luego se mueve según el ánimo (vértices del texto, sin reservar memoria).</summary>
        private void AnimateLetters(float now)
        {
            _label.ForceMeshUpdate();
            var info = _label.textInfo;
            var count = Mathf.Min(_visible, info.characterCount);
            var size = _label.fontSize;
            for (var i = 0; i < count; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible)
                    continue;
                var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                var first = character.vertexIndex;
                var center = (vertices[first] + vertices[first + 2]) * 0.5f;
                var pop = SpeechRhythm.PopScale(now - _revealTimes[i]);
                SpeechRhythm.LetterOffset(_mood, i, now, out var x, out var y);
                var offset = new Vector3(x * size, y * size, 0f);
                for (var v = 0; v < 4; v++)
                    vertices[first + v] = center + (vertices[first + v] - center) * pop + offset;
            }
            _label.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
