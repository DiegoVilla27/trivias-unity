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
    public sealed class MoneySessionNetwork : NetworkBehaviour, IMoneySession, IMoneyAuthority
    {
        [SerializeField] private MoneyConfig _config;
        [SerializeField] private MoneyLayout _layout;
        [SerializeField] private MoneyRuntime _runtime;
        [SerializeField] private MoneyVisualPresenter _presenter;

        private NetworkList<MoneyItemNet> _items;
        private readonly NetworkVariable<bool> _locked = new();
        private MoneyLedger _ledger;
        private readonly List<int> _changed = new(); // Reutilizada en cada operación: sin allocations.
        private bool _notifyPending;

        public MoneyLedger Ledger => _ledger;
        public ulong LocalClientId => NetworkManager.LocalClientId;
        public bool IsLocked => _locked.Value;
        public long TotalInPlay => _ledger.TotalInPlay();
        public long UnplacedTotal => _ledger.TotalAt(MoneyPlace.Table, 0) + CarriedTotal();

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
                // Hasta que empiece la partida no se puede tocar el dinero.
                _locked.Value = true;
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

            _locked.OnValueChanged += OnLockedChanged;
            _presenter.RefreshAll();
            _runtime.Attach(this, IsServer ? this : null);
            GameLog.Info($"[Dinero] Sesión lista: {_ledger.Count} objetos, {MoneyFormat.Amount(_ledger.TotalInPlay())} en juego.", this);
        }

        public override void OnNetworkDespawn()
        {
            _locked.OnValueChanged -= OnLockedChanged;
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
            if (_locked.Value || !IsWithinReach(client, _layout.TableSurface, _config.TableReach))
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
            if (_locked.Value || tray >= _layout.TrayCount ||
                !IsWithinReach(client, _layout.TraySurface(tray), _config.TrayReach))
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

        // ---- Autoridad (flujo de la partida) ----

        public void SetLocked(bool locked)
        {
            if (IsServer)
                _locked.Value = locked;
        }

        public RoundOutcome ResolveRound(int correctTray, int multiplier, bool loseUnplaced)
        {
            _changed.Clear();
            var outcome = _ledger.ResolveRound(correctTray, multiplier, loseUnplaced, _changed);
            CommitChanged();
            GameLog.Info($"[Dinero] Ronda resuelta: perdido {MoneyFormat.Amount(outcome.Lost)}, " +
                         $"conservado {MoneyFormat.Amount(outcome.Kept)}, bono {MoneyFormat.Amount(outcome.Bonus)}, " +
                         $"total {MoneyFormat.Amount(outcome.Total)}.", this);
            return outcome;
        }

        private long CarriedTotal()
        {
            long total = 0;
            for (var i = 0; i < _ledger.Count; i++)
            {
                if (_ledger.Items[i].Place == MoneyPlace.Carried)
                    total += _ledger.ValueOf(i);
            }
            return total;
        }

        private void OnLockedChanged(bool previous, bool current) => _notifyPending = true;

        // ---- Autoridad (jugadores) ----

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
                var state = MoneyItemNet.From(_ledger.Items[index]);
                // Los objetos nuevos (bonos) amplían la lista; los demás actualizan su valor.
                if (index < _items.Count)
                    _items[index] = state;
                else
                    _items.Add(state);
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
                    _notifyPending = true;
                    break;
            }
        }

        /// <summary>
        /// Aplica un cambio al estado visible. La interfaz NO se avisa aquí: durante la lectura de un paquete de red
        /// un error en la interfaz haría que NGO descartase el resto de cambios (y los jugadores se desincronizarían).
        /// Se avisa una vez por frame en <see cref="LateUpdate"/>, con todo el paquete ya aplicado.
        /// </summary>
        private void Apply(int index)
        {
            _presenter.Refresh(index);
            _notifyPending = true;
        }

        private void LateUpdate()
        {
            if (!_notifyPending)
                return;

            _notifyPending = false;
            _runtime.NotifyChanged(MoneyRuntime.ALL);
        }
    }
}
