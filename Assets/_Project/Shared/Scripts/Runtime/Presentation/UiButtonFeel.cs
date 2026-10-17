using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>
    /// Botón con tacto suave: crece apenas al pasar el ratón y se hunde un poco al pulsar (solo si se puede usar), con una
    /// curva que arranca y frena con suavidad (sin saltos). El crecimiento tiene un tope en píxeles: un botón grande apenas
    /// cambia y nunca se sale de su panel.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class UiButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.025f;
        [SerializeField] private float _pressedScale = 0.975f;
        [Tooltip("Lo máximo que crece cada borde al pasar el ratón (o se encoge al pulsar), en píxeles de la interfaz.")]
        [SerializeField, Min(0f)] private float _maxGrowPixels = 3f;
        [Tooltip("Segundos (aprox.) en llegar al ratón encima o al soltar: suave.")]
        [SerializeField, Min(0.01f)] private float _hoverSeconds = 0.16f;
        [Tooltip("Segundos (aprox.) en hundirse al pulsar: algo más rápido, pero sin golpe.")]
        [SerializeField, Min(0.01f)] private float _pressSeconds = 0.08f;

        private Selectable _selectable;
        private Vector3 _restScale;
        private float _target = 1f;
        private float _current = 1f;
        private float _velocity;
        private float _smoothTime;
        private bool _hovered;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            _restScale = transform.localScale;
        }

        /// <summary>Cuánto crece al pasar el ratón: lo pedido, sin pasar del tope en píxeles para su tamaño.</summary>
        public float HoverScale => Mathf.Min(_hoverScale, 1f + Limit());

        /// <summary>Cuánto se hunde al pulsar, con el mismo tope.</summary>
        public float PressedScale => Mathf.Max(_pressedScale, 1f - Limit());

        private float Limit()
        {
            var rect = (RectTransform)transform;
            var size = Mathf.Max(rect.rect.width, rect.rect.height, 1f);
            return 2f * _maxGrowPixels / size;
        }

        /// <summary>Escala a la que va ahora (1: en reposo).</summary>
        public float Target => _target;

        /// <summary>Escala que tiene ahora (va hacia <see cref="Target"/> poco a poco).</summary>
        public float Current => _current;

        private void OnDisable()
        {
            _hovered = false;
            _target = _current = 1f;
            _velocity = 0f;
            transform.localScale = _restScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            GoTo(_selectable.IsInteractable() ? HoverScale : 1f, _hoverSeconds);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            GoTo(1f, _hoverSeconds);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable.IsInteractable())
                GoTo(PressedScale, _pressSeconds);
        }

        public void OnPointerUp(PointerEventData eventData) =>
            GoTo(_hovered && _selectable.IsInteractable() ? HoverScale : 1f, _hoverSeconds);

        private void GoTo(float target, float seconds)
        {
            _target = target;
            _smoothTime = seconds;
        }

        private void Update()
        {
            if (Mathf.Abs(_current - _target) < 0.0005f && Mathf.Abs(_velocity) < 0.001f)
            {
                if (_current != _target)
                {
                    _current = _target;
                    transform.localScale = _restScale * _current;
                }
                return;
            }
            // Muelle amortiguado: arranca y frena con suavidad, sin pasarse.
            _current = Mathf.SmoothDamp(_current, _target, ref _velocity, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            transform.localScale = _restScale * _current;
        }
    }
}
