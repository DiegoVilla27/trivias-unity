using Trivias.Player.Components;
using Trivias.Shared.Events;
using Trivias.Shared.Spawning;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Player.Network
{
    /// <summary>
    /// Une el concursante con la red: activa control y cámara solo para el dueño, lo coloca en su punto
    /// de aparición y sincroniza su nombre (escrito por el host).
    /// </summary>
    public sealed class PlayerNetwork : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private PlayerNameTag _nameTag;
        [SerializeField] private SpawnPointSet _spawnPoints;
        [SerializeField] private TransformEventChannel _localPlayerCameraTarget;

        private readonly NetworkVariable<FixedString32Bytes> _displayName = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public string DisplayName => _displayName.Value.ToString();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                _displayName.Value = new FixedString32Bytes($"Jugador {OwnerClientId + 1}");

            _displayName.OnValueChanged += OnDisplayNameChanged;
            _nameTag.SetName(DisplayName);

            if (!IsOwner)
                return;

            _spawnPoints.Get((int)OwnerClientId, out var position, out var rotation);
            _motor.Teleport(position, rotation);
            SetLocalControl(true);
            _nameTag.gameObject.SetActive(false);
            _localPlayerCameraTarget.Raise(_look.CameraRoot);
        }

        public override void OnNetworkDespawn()
        {
            _displayName.OnValueChanged -= OnDisplayNameChanged;
            if (!IsOwner)
                return;

            SetLocalControl(false);
            _localPlayerCameraTarget.Raise(null);
        }

        private void SetLocalControl(bool isEnabled)
        {
            _input.enabled = isEnabled;
            _motor.enabled = isEnabled;
            _look.enabled = isEnabled;
        }

        private void OnDisplayNameChanged(FixedString32Bytes previous, FixedString32Bytes current) =>
            _nameTag.SetName(current.ToString());
    }
}
