using Trivias.Interaction.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Interaction.Network
{
    /// <summary>
    /// En el jugador local: busca a intervalos el interactivo más cercano, publica su aviso cuando cambia
    /// y lo ejecuta al pulsar E (una unidad), Q (varias) o R (acción especial).
    /// En los jugadores remotos queda desactivado.
    /// </summary>
    public sealed class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] private InteractableRegistry _registry;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private StringEventChannel _promptChannel;
        [SerializeField] private InputActionReference _primaryAction;
        [SerializeField] private InputActionReference _secondaryAction;
        [SerializeField] private InputActionReference _tertiaryAction;
        [SerializeField, Min(0.02f)] private float _scanInterval = 0.1f;

        private IInteractable _focused;
        private string _currentPrompt = string.Empty;
        private float _nextScanTime;

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!IsOwner)
                return;

            Bind(_primaryAction, OnPrimary, true);
            Bind(_secondaryAction, OnSecondary, true);
            Bind(_tertiaryAction, OnTertiary, true);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
                return;

            Bind(_primaryAction, OnPrimary, false);
            Bind(_secondaryAction, OnSecondary, false);
            Bind(_tertiaryAction, OnTertiary, false);
            _focused = null;
            PublishPrompt(string.Empty);
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextScanTime)
                return;

            _nextScanTime = Time.unscaledTime + _scanInterval;
            Refresh();
        }

        private static void Bind(InputActionReference reference, System.Action<InputAction.CallbackContext> handler, bool bind)
        {
            var action = reference.action;
            if (bind)
            {
                action.Enable();
                action.performed += handler;
            }
            else
            {
                action.performed -= handler;
                action.Disable();
            }
        }

        private void OnPrimary(InputAction.CallbackContext _) => Interact(InteractionAction.Primary);

        private void OnSecondary(InputAction.CallbackContext _) => Interact(InteractionAction.Secondary);

        private void OnTertiary(InputAction.CallbackContext _) => Interact(InteractionAction.Tertiary);

        private void Interact(InteractionAction action)
        {
            if (_focused == null || Cursor.lockState != CursorLockMode.Locked)
                return;

            _focused.Interact(new InteractionContext(OwnerClientId, action));
            Refresh();
        }

        private void Refresh()
        {
            // Un concursante eliminado es espectador: no interactúa con nada.
            if (_avatars.IsEliminated(OwnerClientId))
            {
                _focused = null;
                PublishPrompt(string.Empty);
                return;
            }

            _focused = InteractionSelector.FindClosest(_registry.Items, transform.position);
            var context = new InteractionContext(OwnerClientId);
            if (_focused == null || !_focused.TryGetPrompt(context, out var prompt))
            {
                _focused = null;
                prompt = string.Empty;
            }

            PublishPrompt(prompt);
        }

        private void PublishPrompt(string prompt)
        {
            // Solo se publica cuando el texto cambia: la interfaz no se toca si nada ha cambiado.
            if (prompt == _currentPrompt)
                return;

            _currentPrompt = prompt;
            _promptChannel.Raise(prompt);
        }
    }
}
