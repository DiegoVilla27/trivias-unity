using Trivias.Shared.Events;
using Unity.Cinemachine;
using UnityEngine;

namespace Trivias.Cameras.Components
{
    /// <summary>
    /// Conecta la cámara en tercera persona con el jugador local cuando este aparece.
    /// Sin jugador (antes de unirse, al salir o al ser eliminado) vuelve a la vista general del plató.
    /// No conoce el módulo Player: solo escucha el canal de eventos compartido.
    /// </summary>
    public sealed class LocalPlayerCameraBinder : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _followCamera;
        [SerializeField] private CursorLockController _cursorLock;
        [SerializeField] private TransformEventChannel _localPlayerCameraTarget;

        private Vector3 _overviewPosition;
        private Quaternion _overviewRotation;

        private void Awake()
        {
            var cameraTransform = _followCamera.transform;
            _overviewPosition = cameraTransform.position;
            _overviewRotation = cameraTransform.rotation;
        }

        private void OnEnable() => _localPlayerCameraTarget.Register(OnLocalPlayerCameraTargetChanged);

        private void OnDisable() => _localPlayerCameraTarget.Unregister(OnLocalPlayerCameraTargetChanged);

        private void OnLocalPlayerCameraTargetChanged(Transform target)
        {
            _followCamera.Target.TrackingTarget = target;
            if (target == null)
            {
                _followCamera.transform.SetPositionAndRotation(_overviewPosition, _overviewRotation);
                _followCamera.PreviousStateIsValid = false;
            }
            _cursorLock.SetHasLocalPlayer(target != null);
        }
    }
}
