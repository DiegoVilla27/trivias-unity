using Trivias.Shared.Events;
using Unity.Cinemachine;
using UnityEngine;

namespace Trivias.Cameras.Components
{
    /// <summary>
    /// Conecta la cámara en tercera persona con el jugador local cuando este aparece.
    /// No conoce el módulo Player: solo escucha el canal de eventos compartido.
    /// </summary>
    public sealed class LocalPlayerCameraBinder : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _followCamera;
        [SerializeField] private CursorLockController _cursorLock;
        [SerializeField] private TransformEventChannel _localPlayerCameraTarget;

        private void OnEnable() => _localPlayerCameraTarget.Register(OnLocalPlayerCameraTargetChanged);

        private void OnDisable() => _localPlayerCameraTarget.Unregister(OnLocalPlayerCameraTargetChanged);

        private void OnLocalPlayerCameraTargetChanged(Transform target)
        {
            _followCamera.Target.TrackingTarget = target;
            _cursorLock.SetCaptureAllowed(target != null);
        }
    }
}
