using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Lee la entrada del jugador local. Empieza desactivado en el prefab y solo lo activa el dueño:
    /// las acciones son compartidas en el proceso y no deben activarse ni desactivarse desde jugadores remotos.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference _move;
        [SerializeField] private InputActionReference _look;
        [SerializeField] private InputActionReference _interact;

        public event Action InteractPressed;

        public Vector2 Move => _move.action.ReadValue<Vector2>();
        public Vector2 Look => _look.action.ReadValue<Vector2>();

        private void OnEnable()
        {
            _move.action.Enable();
            _look.action.Enable();
            _interact.action.Enable();
            _interact.action.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            _interact.action.performed -= OnInteractPerformed;
            _move.action.Disable();
            _look.action.Disable();
            _interact.action.Disable();
        }

        private void OnInteractPerformed(InputAction.CallbackContext _) => InteractPressed?.Invoke();
    }
}
