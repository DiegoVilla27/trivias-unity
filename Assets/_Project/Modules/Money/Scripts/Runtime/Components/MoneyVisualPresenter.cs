using System.Collections.Generic;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Carrying;
using Trivias.Shared.Pooling;
using UnityEngine;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Dibuja el dinero en cada cliente a partir del libro de cuentas: mesa, bandejas o en manos de un concursante.
    /// Los objetos visibles salen de pools precalentados al cargar; solo se recolocan cuando cambia su estado.
    /// </summary>
    public sealed class MoneyVisualPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _bundlePrefab;
        [SerializeField] private GameObject _bagPrefab;
        [SerializeField] private GameObject _goldBarPrefab;
        [SerializeField] private CarrySocketRegistry _carrySockets;

        private readonly List<GameObject> _visuals = new();
        private readonly List<Denomination> _visualDenominations = new();
        private GameObjectPool[] _pools;
        private MoneyLayout _layout;
        private MoneyLedger _ledger;

        public void Initialize(MoneyConfig config, MoneyLayout layout, MoneyLedger ledger)
        {
            _layout = layout;
            _ledger = ledger;
            if (_pools != null)
                return;

            _pools = new[]
            {
                new GameObjectPool(_bundlePrefab, transform, config.PoolSize(Denomination.Bundle)),
                new GameObjectPool(_bagPrefab, transform, config.PoolSize(Denomination.Bag)),
                new GameObjectPool(_goldBarPrefab, transform, config.PoolSize(Denomination.GoldBar)),
            };
        }

        private void OnEnable() => _carrySockets.Changed += OnCarrySocketChanged;

        private void OnDisable() => _carrySockets.Changed -= OnCarrySocketChanged;

        public void RefreshAll()
        {
            for (var i = 0; i < _ledger.Count; i++)
                Refresh(i);
        }

        public void Refresh(int index)
        {
            var item = _ledger.Items[index];
            if (item.Place == MoneyPlace.Lost)
            {
                Release(index);
                return;
            }

            var visual = Acquire(index, item.Denomination);
            Place(visual.transform, item);
        }

        public void ReleaseAll()
        {
            for (var i = 0; i < _visuals.Count; i++)
                Release(i);
        }

        private void Place(Transform visual, in MoneyItem item)
        {
            switch (item.Place)
            {
                case MoneyPlace.Table:
                    visual.SetParent(transform, false);
                    visual.SetPositionAndRotation(_layout.TableSlot(item.Slot), Quaternion.identity);
                    visual.gameObject.SetActive(true);
                    break;
                case MoneyPlace.Tray:
                    visual.SetParent(transform, false);
                    visual.SetPositionAndRotation(_layout.TraySlot(item.PlaceIndex, item.Slot), _layout.TrayRotation(item.PlaceIndex));
                    visual.gameObject.SetActive(true);
                    break;
                case MoneyPlace.Carried:
                    // Si el concursante aún no ha aparecido en este cliente, se oculta hasta que se registre.
                    // Si se va, se recupera antes de que el concursante se destruya (no se pierde la instancia del pool).
                    if (_carrySockets.TryGet(item.Holder, out var socket))
                    {
                        visual.SetParent(socket.CarryPoint, false);
                        visual.SetLocalPositionAndRotation(_layout.CarryOffset((int)item.Denomination, item.Slot), Quaternion.identity);
                        visual.gameObject.SetActive(true);
                    }
                    else
                    {
                        visual.SetParent(transform, false);
                        visual.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void OnCarrySocketChanged(ulong clientId)
        {
            if (_ledger == null)
                return;

            for (var i = 0; i < _ledger.Count; i++)
            {
                var item = _ledger.Items[i];
                if (item.Place == MoneyPlace.Carried && item.Holder == clientId)
                    Refresh(i);
            }
        }

        private GameObject Acquire(int index, Denomination denomination)
        {
            while (_visuals.Count <= index)
            {
                _visuals.Add(null);
                _visualDenominations.Add(default);
            }

            if (_visuals[index] != null && _visualDenominations[index] == denomination)
                return _visuals[index];

            Release(index);
            var visual = _pools[(int)denomination].Get();
            _visuals[index] = visual;
            _visualDenominations[index] = denomination;
            return visual;
        }

        private void Release(int index)
        {
            if (index >= _visuals.Count || _visuals[index] == null)
                return;

            _pools[(int)_visualDenominations[index]].Release(_visuals[index]);
            _visuals[index] = null;
        }
    }
}
