using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>Interruptor de los menús: mueve el mando a un lado u otro y escribe «Sí» o «No» según su estado.</summary>
    public sealed class UiSwitch : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private RectTransform _knob;
        [SerializeField] private TextMeshProUGUI _state;
        [SerializeField] private float _offX;
        [SerializeField] private float _onX;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(Show);
            Show(_toggle.isOn);
        }

        private void OnDisable() => _toggle.onValueChanged.RemoveListener(Show);

        /// <summary>Al cambiarlo por código sin aviso (<c>SetIsOnWithoutNotify</c>) hay que redibujarlo.</summary>
        public void Refresh() => Show(_toggle.isOn);

        private void Show(bool on)
        {
            var position = _knob.anchoredPosition;
            position.x = on ? _onX : _offX;
            _knob.anchoredPosition = position;
            _state.text = on ? "Sí" : "No";
        }
    }
}
