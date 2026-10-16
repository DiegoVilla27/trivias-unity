using UnityEngine;

namespace Trivias.Shared.Presentation
{
    /// <summary>Latido suave (para el logo del menú): crece y vuelve, sin parar.</summary>
    public sealed class UiPulse : MonoBehaviour
    {
        [SerializeField] private float _amount = 0.035f;
        [SerializeField] private float _speed = 2.2f;

        private Vector3 _restScale;

        private void Awake() => _restScale = transform.localScale;

        private void Update() =>
            transform.localScale = _restScale * (1f + _amount * Mathf.Sin(Time.unscaledTime * _speed));
    }
}
