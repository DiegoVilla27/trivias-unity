using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>
    /// Botón con tacto: crece un poco al pasar el ratón y se hunde al pulsar (solo si se puede usar). El crecimiento tiene
    /// un tope en píxeles: un botón grande apenas cambia y nunca se sale de su panel.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class UiButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.95f;
        [Tooltip("Lo máximo que crece cada borde al pasar el ratón (o se encoge al pulsar), en píxeles de la interfaz.")]
        [SerializeField, Min(0f)] private float _maxGrowPixels = 5f;
        [Tooltip("Cambio de escala por segundo (rápido: se nota el toque sin retrasar nada).")]
        [SerializeField, Min(0.1f)] private float _speed = 1.4f;

        private Selectable _selectable;
        private Vector3 _restScale;
        private float _target = 1f;
        private float _current = 1f;
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

        private void OnDisable()
        {
            _hovered = false;
            _target = _current = 1f;
            transform.localScale = _restScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            _target = _selectable.IsInteractable() ? HoverScale : 1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _target = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable.IsInteractable())
                _target = PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData) =>
            _target = _hovered && _selectable.IsInteractable() ? HoverScale : 1f;

        private void Update()
        {
            if (Mathf.Approximately(_current, _target))
                return;
            _current = Mathf.MoveTowards(_current, _target, Time.unscaledDeltaTime * _speed);
            transform.localScale = _restScale * _current;
        }
    }
}
