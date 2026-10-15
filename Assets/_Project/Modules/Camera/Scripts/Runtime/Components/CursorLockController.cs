using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Cameras.Components
{
    /// <summary>
    /// Captura el ratón para mirar con la cámara. Esc lo libera y un clic lo vuelve a capturar.
    /// Funciona por eventos de entrada, sin comprobar teclas cada frame.
    /// </summary>
    public sealed class CursorLockController : MonoBehaviour
    {
        private InputAction _releaseAction;
        private InputAction _captureAction;
        private bool _captureAllowed;

        private void Awake()
        {
            _releaseAction = new InputAction("ReleaseCursor", InputActionType.Button, "<Keyboard>/escape");
            _captureAction = new InputAction("CaptureCursor", InputActionType.Button, "<Mouse>/leftButton");
        }

        private void OnEnable()
        {
            _releaseAction.performed += OnReleasePerformed;
            _captureAction.performed += OnCapturePerformed;
            _releaseAction.Enable();
            _captureAction.Enable();
        }

        private void OnDisable()
        {
            _releaseAction.performed -= OnReleasePerformed;
            _captureAction.performed -= OnCapturePerformed;
            _releaseAction.Disable();
            _captureAction.Disable();
            Apply(false);
        }

        private void OnDestroy()
        {
            _releaseAction.Dispose();
            _captureAction.Dispose();
        }

        public void SetCaptureAllowed(bool allowed)
        {
            _captureAllowed = allowed;
            Apply(allowed);
        }

        private void OnReleasePerformed(InputAction.CallbackContext _) => Apply(false);

        private void OnCapturePerformed(InputAction.CallbackContext _)
        {
            if (_captureAllowed)
                Apply(true);
        }

        private static void Apply(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
