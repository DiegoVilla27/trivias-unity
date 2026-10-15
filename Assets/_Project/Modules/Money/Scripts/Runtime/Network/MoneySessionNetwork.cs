using Trivias.Money.Components;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using System.Collections.Generic;
using Trivias.Shared.Diagnostics;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Money.Network
{
    /// <summary>
    /// Dinero de la sesión en red. El host ejecuta el <see cref="MoneyLedger"/> como autoridad y replica
    /// solo el estado de cada objeto (NetworkList); los clientes mantienen una réplica del libro.
    /// Las posiciones visuales se calculan en cada cliente a partir del estado, sin sincronizar transforms.
    /// </summary>
    public sealed class MoneySessionNetwork : NetworkBehaviour, IMoneySession
    {
        [SerializeField] private MoneyConfig _config;
        [SerializeField] private MoneyLayout _layout;
        [SerializeField] private MoneyRuntime _runtime;
        [SerializeField] private MoneyVisualPresenter _presenter;

        private NetworkList<MoneyItemNet> _items;
        private MoneyLedger _ledger;
        private readonly List<int> _changed = new(); // Reutilizada en cada operación: sin allocations.

        public MoneyLedger Ledger => _ledger;
        public ulong LocalClientId => NetworkManager.LocalClientId;

        private void Awake()
        {
            _items = new NetworkList<MoneyItemNet>();
            _ledger = new MoneyLedger(_config.CreateValues(), _config.CreateCarryLimits());
        }

        public override void OnNetworkSpawn()
        {
            _presenter.Initialize(_config, _layout, _ledger);

            if (IsServer)
            {
                for (var i = 0; i < _config.StartingBundles; i++)
                    _items.Add(MoneyItemNet.From(_ledger.Items[_ledger.AddToTable(Denomination.Bundle)]));
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
            else
            {
                for (var i = 0; i < _items.Count; i++)
                    _ledger.Set(i, _items[i].ToItem());
                _items.OnListChanged += OnItemsChanged;
            }

            _presenter.RefreshAll();
            _runtime.Attach(this);
            GameLog.Info($"[Dinero] Sesión lista: {_ledger.Count} objetos, {MoneyFormat.Amount(_ledger.TotalInPlay())} en juego.", this);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            else
                _items.OnListChanged -= OnItemsChanged;

            _runtime.Detach(this);
            _presenter.ReleaseAll();
            _ledger.Clear();
        }

        // ---- Peticiones de los jugadores (se ejecutan en el host) ----

        public void RequestTableInteraction(InteractionAction action) => TableInteractionRpc(action);

        public void RequestTrayInteraction(int tray, InteractionAction action) => TrayInteractionRpc((byte)tray, action);

        /// <summary>
        /// Mesa: E coge uno, Q coge hasta el límite de ese tipo, R devuelve todo lo que se lleva.
        /// Solo se coge la denominación que ya se lleva (un tipo por viaje).
        /// </summary>
        [Rpc(SendTo.Server)]
        private void TableInteractionRpc(InteractionAction action, RpcParams rpcParams = default)
        {
            var client = rpcParams.Receive.SenderClientId;
            if (!IsWithinReach(client, _layout.TableSurface, _config.TableReach))
                return;

            _changed.Clear();
            switch (action)
            {
                case InteractionAction.Primary:
                    _ledger.PickFromTable(client, 1, _changed);
                    break;
                case InteractionAction.Secondary:
                    _ledger.PickFromTable(client, int.MaxValue, _changed);
                    break;
                case InteractionAction.Tertiary:
                    _ledger.ReturnToTable(client, int.MaxValue, _changed);
                    break;
            }
            CommitChanged();
        }

        /// <summary>Bandeja: con dinero en las manos lo deja (E uno, Q todo); sin dinero lo recupera (E uno, Q varios).</summary>
        [Rpc(SendTo.Server)]
        private void TrayInteractionRpc(byte tray, InteractionAction action, RpcParams rpcParams = default)
        {
            var client = rpcParams.Receive.SenderClientId;
            if (tray >= _layout.TrayCount || !IsWithinReach(client, _layout.TraySurface(tray), _config.TrayReach))
                return;

            var count = action == InteractionAction.Secondary ? int.MaxValue : 1;
            _changed.Clear();
            if (action != InteractionAction.Tertiary)
            {
                if (_ledger.CountCarriedBy(client) > 0)
                    _ledger.DepositToTray(client, tray, count, _changed);
                else
                    _ledger.PickFromTray(client, tray, count, _changed);
            }
            CommitChanged();
        }

        // ---- Autoridad ----

        private void OnClientDisconnected(ulong client)
        {
            // Lo que llevaba un jugador que se va vuelve a la mesa: el dinero del equipo no desaparece.
            _changed.Clear();
            _ledger.ReturnToTable(client, int.MaxValue, _changed);
            CommitChanged();
        }

        private void CommitChanged()
        {
            for (var i = 0; i < _changed.Count; i++)
            {
                var index = _changed[i];
                _items[index] = MoneyItemNet.From(_ledger.Items[index]);
                Apply(index);
            }
        }

        private bool IsWithinReach(ulong client, Vector3 target, float reach)
        {
            if (!NetworkManager.ConnectedClients.TryGetValue(client, out var connected) || connected.PlayerObject == null)
                return false;

            var offset = connected.PlayerObject.transform.position - target;
            offset.y = 0f;
            var maxDistance = reach + _config.ServerReachTolerance;
            return offset.sqrMagnitude <= maxDistance * maxDistance;
        }

        // ---- Réplica ----

        private void OnItemsChanged(NetworkListEvent<MoneyItemNet> change)
        {
            switch (change.Type)
            {
                case NetworkListEvent<MoneyItemNet>.EventType.Add:
                case NetworkListEvent<MoneyItemNet>.EventType.Insert:
                case NetworkListEvent<MoneyItemNet>.EventType.Value:
                    _ledger.Set(change.Index, change.Value.ToItem());
                    Apply(change.Index);
                    break;
                default:
                    _ledger.Clear();
                    for (var i = 0; i < _items.Count; i++)
                        _ledger.Set(i, _items[i].ToItem());
                    _presenter.RefreshAll();
                    _runtime.NotifyChanged(MoneyRuntime.ALL);
                    break;
            }
        }

        private void Apply(int index)
        {
            _presenter.Refresh(index);
            _runtime.NotifyChanged(index);
        }
    }
}
