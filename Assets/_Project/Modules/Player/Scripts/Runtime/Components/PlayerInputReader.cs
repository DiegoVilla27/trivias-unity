using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Lee el movimiento y la mirada del jugador local. Empieza desactivado en el prefab y solo lo activa el dueño:
    /// las acciones son compartidas en el proceso y no deben activarse ni desactivarse desde jugadores remotos.
    /// Las teclas de interacción las gestiona el módulo Interaction.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference _move;
        [SerializeField] private InputActionReference _look;

        public Vector2 Move => _move.action.ReadValue<Vector2>();
        public Vector2 Look => _look.action.ReadValue<Vector2>();

        private void OnEnable()
        {
            _move.action.Enable();
            _look.action.Enable();
        }

        private void OnDisable()
        {
            _move.action.Disable();
            _look.action.Disable();
        }
    }
}
