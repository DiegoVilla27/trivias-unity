using UnityEngine;

namespace Trivias.Shared.Presentation
{
    /// <summary>Orienta un texto del mundo hacia la cámara local para que se lea desde cualquier lado.</summary>
    public sealed class Billboard : MonoBehaviour
    {
        private Transform _cameraTransform;

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
