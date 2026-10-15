using TMPro;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>Nombre flotante sobre el concursante, siempre mirando a la cámara local.</summary>
    public sealed class PlayerNameTag : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _label;

        private Transform _cameraTransform;

        public void SetName(string displayName) => _label.text = displayName;

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                    return;
                _cameraTransform = mainCamera.transform;
            }

            transform.rotation = _cameraTransform.rotation;
        }
    }
}
