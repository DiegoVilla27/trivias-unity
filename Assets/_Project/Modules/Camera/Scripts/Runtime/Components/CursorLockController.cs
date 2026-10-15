using Trivias.Shared.Gameplay;
using Trivias.Shared.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Cameras.Components
{
    /// <summary>
    /// Captura del ratón para mirar con la cámara.
    /// - En la sala de espera (nadie se mueve) el ratón queda libre para usar la interfaz.
    /// - Al empezar la partida se captura solo.
    /// - Esc lo libera; un clic lo vuelve a capturar, salvo si es sobre un panel de interfaz.
    /// Funciona por eventos de entrada, sin comprobar teclas cada frame.
    /// </summary>
    public sealed class CursorLockController : MonoBehaviour
    {
        [SerializeField] private PlayerControlGate _controlGate;
        [SerializeField] private CursorCaptureBlockers _blockers;

        private InputAction _releaseAction;
        private InputAction _captureAction;
        private bool _hasLocalPlayer;

        private bool CaptureAllowed => _hasLocalPlayer && _controlGate.CanMove;

        private void Awake()
        {
            _releaseAction = new InputAction("ReleaseCursor", InputActionType.Button, "<Keyboard>/escape");
            _captureAction = new InputAction("CaptureCursor", InputActionType.Button, "<Mouse>/leftButton");
        }

        private void OnEnable()
        {
            _releaseAction.performed += OnReleasePerformed;
            _captureAction.performed += OnCapturePerformed;
            _controlGate.Changed += OnControlGateChanged;
            _releaseAction.Enable();
            _captureAction.Enable();
        }

        private void OnDisable()
        {
            _releaseAction.performed -= OnReleasePerformed;
            _captureAction.performed -= OnCapturePerformed;
            _controlGate.Changed -= OnControlGateChanged;
            _releaseAction.Disable();
            _captureAction.Disable();
            Apply(false);
        }

        private void OnDestroy()
        {
            _releaseAction.Dispose();
            _captureAction.Dispose();
        }

        /// <summary>Hay (o deja de haber) un concursante local al que seguir con la cámara.</summary>
        public void SetHasLocalPlayer(bool hasLocalPlayer)
        {
            _hasLocalPlayer = hasLocalPlayer;
            Apply(CaptureAllowed);
        }

        private void OnControlGateChanged(bool canMove) => Apply(CaptureAllowed);

        private void OnReleasePerformed(InputAction.CallbackContext _) => Apply(false);

        private void OnCapturePerformed(InputAction.CallbackContext context)
        {
            if (!CaptureAllowed)
                return;

            var pointer = Mouse.current;
            if (pointer != null && _blockers.IsBlocked(pointer.position.ReadValue()))
                return; // El clic es para un panel de interfaz.

            Apply(true);
        }

        private static void Apply(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
