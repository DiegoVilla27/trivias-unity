using System.Collections.Generic;
using Trivias.SelfBet.Data;
using Trivias.SelfBet.Logic;
using Trivias.Shared.Diagnostics;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Players;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.SelfBet.Network
{
    /// <summary>
    /// Comodines en red. El host lleva el <see cref="SelfBetBook"/> y replica el estado de cada concursante; los clientes
    /// aplican lo recibido y avisan a la interfaz una vez por frame (nunca durante la lectura de un paquete).
    /// En cada cliente, si su concursante está apostado, lo confina al círculo de la cabina.
    /// </summary>
    public sealed class SelfBetSessionNetwork : NetworkBehaviour, ISelfBetSession, ISelfBetAuthority
    {
        [SerializeField] private SelfBetConfig _config;
        [SerializeField] private SelfBetLayout _layout;
        [SerializeField] private SelfBetRuntime _runtime;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private PlayerMovementLimit _movementLimit;

        private NetworkList<BettorNet> _bettors;
        private readonly NetworkVariable<byte> _reserve = new();
        private readonly NetworkVariable<bool> _open = new();

        private readonly SelfBetBook _book = new();
        private readonly List<Bettor> _replicaBuffer = new(6);
        private bool _notifyPending;

        public SelfBetBook Book => _book;
        public ulong LocalClientId => NetworkManager.LocalClientId;
        public bool IsOpen => _open.Value;
        public int ActivePlayers => _book.ActivePlayers;
        public bool AllActiveHaveBet => _book.AllActiveHaveBet;
        public bool HasBet(ulong client) => _book.HasBet(client);

        private void Awake() => _bettors = new NetworkList<BettorNet>();

        public override void OnNetworkSpawn()
        {
            _bettors.OnListChanged += OnBettorsChanged;
            _reserve.OnValueChanged += OnReserveChanged;
            _open.OnValueChanged += OnOpenChanged;
            if (!IsServer)
                ReadReplica();
            _runtime.Attach(this, IsServer ? this : null);
            _notifyPending = true;
        }

        public override void OnNetworkDespawn()
        {
            _bettors.OnListChanged -= OnBettorsChanged;
            _reserve.OnValueChanged -= OnReserveChanged;
            _open.OnValueChanged -= OnOpenChanged;
            _runtime.Detach(this);
            _movementLimit.Clear();
        }

        // ---- Jugadores ----

        public void RequestToggle(int booth) => ToggleRpc((byte)booth);

        [Rpc(SendTo.Server)]
        private void ToggleRpc(byte booth, RpcParams rpcParams = default)
        {
            var client = rpcParams.Receive.SenderClientId;
            if (!_open.Value || booth >= _layout.BoothCount || !IsInZone(client, booth, _config.ZoneRadius + _config.ServerTolerance))
                return;

            var index = _book.IndexOf(client);
            if (index < 0)
                return;

            var changed = _book.Bettors[index].Booth == booth ? _book.Cancel(client) : _book.Prepare(client, booth);
            if (changed)
                Publish();
        }

        // ---- Autoridad (partida) ----

        public void StartMatch(IReadOnlyList<ulong> clients)
        {
            _book.Start(clients);
            GameLog.Info($"[Comodines] {clients.Count} jugadores · reserva {_book.Reserve}.", this);
            Publish();
        }

        public void SetOpen(bool open) => _open.Value = open;

        public void LockBets()
        {
            _open.Value = false;
            _book.LockAll();
            Publish();
        }

        public int Resolve(int correctBooth, List<ulong> eliminated)
        {
            var multiplier = _book.Resolve(correctBooth, eliminated);
            foreach (var client in eliminated)
            {
                if (_avatars.TryGet(client, out var avatar))
                    avatar.Eliminate();
            }
            Publish();
            return multiplier;
        }

        public void RemovePlayer(ulong client)
        {
            if (_book.RemovePlayer(client))
                Publish();
        }

        public void ResetForNewMatch()
        {
            _open.Value = false;
            _book.Start(System.Array.Empty<ulong>());
            Publish();
        }

        // ---- Validación del host ----

        private bool IsInZone(ulong client, int booth, float radius)
        {
            if (!_avatars.TryGet(client, out var avatar) || avatar.IsEliminated)
                return false;

            var offset = avatar.Position - _layout.ZoneCenter(booth);
            offset.y = 0f;
            return offset.sqrMagnitude <= radius * radius;
        }

        /// <summary>Host: escribe en red solo las entradas que cambiaron.</summary>
        private void Publish()
        {
            var bettors = _book.Bettors;
            for (var i = 0; i < bettors.Count; i++)
            {
                var state = BettorNet.From(bettors[i]);
                if (i >= _bettors.Count)
                    _bettors.Add(state);
                else if (!_bettors[i].Equals(state))
                    _bettors[i] = state;
            }
            while (_bettors.Count > bettors.Count)
                _bettors.RemoveAt(_bettors.Count - 1);

            _reserve.Value = (byte)_book.Reserve;
            _notifyPending = true;
        }

        // ---- Réplica ----

        private void ReadReplica()
        {
            _replicaBuffer.Clear();
            for (var i = 0; i < _bettors.Count; i++)
                _replicaBuffer.Add(_bettors[i].ToBettor());
            _book.Set(_replicaBuffer, _reserve.Value);
        }

        private void OnBettorsChanged(NetworkListEvent<BettorNet> _) => _notifyPending = true;
        private void OnReserveChanged(byte previous, byte current) => _notifyPending = true;
        private void OnOpenChanged(bool previous, bool current) => _notifyPending = true;

        private void LateUpdate()
        {
            if (!_notifyPending)
                return;

            _notifyPending = false;
            if (!IsServer)
                ReadReplica();
            UpdateLocalConfinement();
            _runtime.NotifyChanged();
        }

        /// <summary>El concursante local apostado no puede salir del círculo de su cabina; sin apuesta, se mueve libre.</summary>
        private void UpdateLocalConfinement()
        {
            var index = _book.IndexOf(NetworkManager.LocalClientId);
            if (index >= 0 && _book.Bettors[index].HasBet && !_book.Bettors[index].IsEliminated)
                _movementLimit.SetZone(_layout.ZoneCenter(_book.Bettors[index].Booth), _config.ConfinementRadius);
            else
                _movementLimit.Clear();
        }
    }
}
