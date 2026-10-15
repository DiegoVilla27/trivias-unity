using Trivias.Player.Components;
using Trivias.Shared.Events;
using Trivias.Shared.Players;
using Trivias.Shared.Spawning;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Player.Network
{
    /// <summary>
    /// Une el concursante con la red: activa control y cámara solo para el dueño, lo coloca en su punto de aparición,
    /// sincroniza su nombre y su eliminación (escritos por el host) y lo registra para el resto de módulos.
    /// </summary>
    public sealed class PlayerNetwork : NetworkBehaviour, IPlayerAvatar
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private PlayerNameTag _nameTag;
        [SerializeField] private PlayerCarrySocket _carrySocket;
        [SerializeField] private PlayerExpulsionView _expulsion;
        [SerializeField] private SpawnPointSet _spawnPoints;
        [SerializeField] private TransformEventChannel _localPlayerCameraTarget;
        [SerializeField] private PlayerAvatarRegistry _avatars;

        private readonly NetworkVariable<FixedString32Bytes> _displayName = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _eliminated = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Cada incremento pide al dueño volver a su punto de aparición (nueva partida).
        private readonly NetworkVariable<byte> _respawnSequence = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // ---- IPlayerAvatar ----

        public ulong ClientId => OwnerClientId;
        public string DisplayName => _displayName.Value.ToString();
        public Vector3 Position => transform.position;
        public Transform CarryPoint => _carrySocket.CarryPoint;
        public bool IsEliminated => _eliminated.Value;

        public void Eliminate()
        {
            if (IsServer)
                _eliminated.Value = true;
        }

        public void Respawn()
        {
            if (!IsServer)
                return;
            _eliminated.Value = false;
            _respawnSequence.Value++;
        }

        // ---- Red ----

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                _displayName.Value = new FixedString32Bytes($"Jugador {OwnerClientId + 1}");

            _displayName.OnValueChanged += OnDisplayNameChanged;
            _eliminated.OnValueChanged += OnEliminatedChanged;
            _respawnSequence.OnValueChanged += OnRespawnRequested;
            _nameTag.SetName(DisplayName);
            _avatars.Register(this);

            if (!IsOwner)
                return;

            PlaceAtSpawn();
            SetLocalControl(true);
            _nameTag.gameObject.SetActive(false);
            _localPlayerCameraTarget.Raise(_look.CameraRoot);
        }

        private void PlaceAtSpawn()
        {
            _spawnPoints.Get((int)OwnerClientId, out var position, out var rotation);
            _motor.Teleport(position, rotation);
        }

        public override void OnNetworkDespawn()
        {
            _displayName.OnValueChanged -= OnDisplayNameChanged;
            _eliminated.OnValueChanged -= OnEliminatedChanged;
            _respawnSequence.OnValueChanged -= OnRespawnRequested;
            _avatars.Unregister(OwnerClientId);
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

        private void OnDisplayNameChanged(FixedString32Bytes previous, FixedString32Bytes current)
        {
            _nameTag.SetName(current.ToString());
            _avatars.NotifyChanged(OwnerClientId);
        }

        private void OnRespawnRequested(byte previous, byte current)
        {
            _expulsion.Restore(showNameTag: !IsOwner);
            _avatars.NotifyChanged(OwnerClientId);
            if (!IsOwner)
                return;

            PlaceAtSpawn();
            SetLocalControl(true);
            _localPlayerCameraTarget.Raise(_look.CameraRoot);
        }

        private void OnEliminatedChanged(bool previous, bool current)
        {
            if (!current)
                return;

            _expulsion.Play();
            _avatars.NotifyChanged(OwnerClientId);

            if (!IsOwner)
                return;

            // El eliminado pasa a espectador: sin control y con la cámara en la vista general del plató.
            SetLocalControl(false);
            _localPlayerCameraTarget.Raise(null);
        }
    }
}
