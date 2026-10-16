using UnityEngine;

namespace Trivias.Shared.Presentation
{
    /// <summary>Los paneles aparecen con un pequeño rebote: crecen desde un poco más pequeños y se funden (0,22 s).</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UiPanelIntro : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float _seconds = 0.22f;
        [SerializeField] private float _startScale = 0.9f;

        private CanvasGroup _group;
        private Vector3 _restScale;
        private float _elapsed;
        private bool _playing;

        public bool IsPlaying => _playing;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _restScale = transform.localScale;
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            _playing = true;
            Apply(0f);
        }

        private void OnDisable()
        {
            _playing = false;
            Apply(1f);
        }

        private void Update()
        {
            if (!_playing)
                return;
            _elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_elapsed / _seconds);
            Apply(t);
            if (t >= 1f)
                _playing = false;
        }

        private void Apply(float t)
        {
            _group.alpha = Mathf.Clamp01(t * 1.6f);
            transform.localScale = _restScale * Mathf.LerpUnclamped(_startScale, 1f, EaseOutBack(t));
        }

        /// <summary>Se pasa un poco y vuelve: da vida sin marear.</summary>
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
