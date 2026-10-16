using System.Collections.Generic;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Players;
using Trivias.Shared.Pooling;
using Trivias.Shared.Tubes;
using UnityEngine;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Dibuja el dinero en cada cliente a partir del libro de cuentas: mesa, bandejas o en manos de un concursante.
    /// Los objetos visibles salen de pools precalentados al cargar; solo se recolocan cuando cambia su estado.
    /// El dinero que se pierde en una bandeja no desaparece: lo succiona el tubo de su cabina (sube de la bandeja a la
    /// entrada del tubo y recorre sus curvas hasta el techo, en fila y acelerando).
    /// </summary>
    public sealed class MoneyVisualPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _bundlePrefab;
        [SerializeField] private GameObject _bagPrefab;
        [SerializeField] private GameObject _goldBarPrefab;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [Tooltip("Tubos de las cabinas: el dinero perdido sube por el tubo del dinero de su cabina.")]
        [SerializeField] private TubeRegistry _tubes;
        [Header("Succión")]
        [SerializeField, Min(0.05f)] private float _riseSeconds = 0.55f;
        [SerializeField, Min(0f)] private float _tubeStartSpeed = 3f;
        [SerializeField, Min(0f)] private float _tubeAcceleration = 22f;
        [Tooltip("Separación entre fajos de la misma bandeja (salen en fila).")]
        [SerializeField, Min(0f)] private float _stagger = 0.07f;
        [SerializeField] private float _spinDegreesPerSecond = 540f;

        private struct Flight
        {
            public GameObject Visual;
            public Denomination Denomination;
            public TubePath Tube;
            public Vector3 Start;
            public Quaternion StartRotation;
            public float Delay;
            public float Elapsed;
        }

        private readonly List<Flight> _flights = new(64);
        private readonly List<MoneyPlace> _visualPlaces = new();
        private readonly List<int> _visualPlaceIndices = new();
        private readonly int[] _queuedPerTube = new int[8];

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

        private void OnEnable() => _avatars.Changed += OnAvatarChanged;

        private void OnDisable() => _avatars.Changed -= OnAvatarChanged;

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
                if (!TrySuck(index))
                    Release(index);
                return;
            }

            var visual = Acquire(index, item.Denomination);
            Place(visual.transform, item);
            _visualPlaces[index] = item.Place;
            _visualPlaceIndices[index] = item.PlaceIndex;
        }

        /// <summary>Si el objeto estaba en una bandeja con tubo, lo succiona ese tubo en vez de desaparecer.</summary>
        private bool TrySuck(int index)
        {
            if (_tubes == null || index >= _visuals.Count || _visuals[index] == null || _visualPlaces[index] != MoneyPlace.Tray)
                return false;
            var tray = _visualPlaceIndices[index];
            if (!_tubes.TryGet(TubeKind.Money, tray, out var tube))
                return false;

            var visual = _visuals[index];
            _visuals[index] = null; // Ya no es de este índice: el libro puede reutilizarlo mientras vuela.
            _visualPlaces[index] = MoneyPlace.Lost;
            var queued = tray < _queuedPerTube.Length ? _queuedPerTube[tray]++ : 0;
            _flights.Add(new Flight
            {
                Visual = visual,
                Denomination = _visualDenominations[index],
                Tube = tube,
                Start = visual.transform.position,
                StartRotation = visual.transform.rotation,
                Delay = queued * _stagger,
            });
            tube.PlaySuction();
            return true;
        }

        private void Update()
        {
            if (_flights.Count == 0)
                return;

            var deltaTime = Time.deltaTime;
            for (var i = _flights.Count - 1; i >= 0; i--)
            {
                var flight = _flights[i];
                flight.Elapsed += deltaTime;
                _flights[i] = flight;
                var t = flight.Elapsed - flight.Delay;
                if (t < 0f)
                    continue;

                var visual = flight.Visual.transform;
                var spin = Quaternion.AngleAxis(t * _spinDegreesPerSecond, Vector3.up);
                if (t < _riseSeconds)
                {
                    // De la bandeja a la entrada del tubo: sube primero y entra por arriba, sin esquinas.
                    var inlet = flight.Tube.Inlet;
                    var control = new Vector3(flight.Start.x, inlet.y + 0.4f, flight.Start.z);
                    visual.SetPositionAndRotation(PathMath.Bezier(flight.Start, control, inlet, PathMath.EaseIn(t / _riseSeconds)),
                        spin * flight.StartRotation);
                    continue;
                }

                var distance = PathMath.SuctionDistance(t - _riseSeconds, _tubeStartSpeed, _tubeAcceleration);
                if (distance >= flight.Tube.Length)
                {
                    Finish(i);
                    continue;
                }
                flight.Tube.Sample(distance, out var position, out _);
                visual.SetPositionAndRotation(position, spin * flight.StartRotation);
            }

            if (_flights.Count == 0)
                System.Array.Clear(_queuedPerTube, 0, _queuedPerTube.Length);
        }

        private void Finish(int flightIndex)
        {
            var flight = _flights[flightIndex];
            _pools[(int)flight.Denomination].Release(flight.Visual);
            _flights.RemoveAt(flightIndex);
        }

        public void ReleaseAll()
        {
            for (var i = 0; i < _visuals.Count; i++)
                Release(i);
            for (var i = _flights.Count - 1; i >= 0; i--)
                Finish(i);
            System.Array.Clear(_queuedPerTube, 0, _queuedPerTube.Length);
        }

        /// <summary>Objetos que están subiendo por un tubo ahora mismo (lo usan las pruebas).</summary>
        public int FlightsInProgress => _flights.Count;

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
                    if (_avatars.TryGet(item.Holder, out var avatar))
                    {
                        visual.SetParent(avatar.CarryPoint, false);
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

        private void OnAvatarChanged(ulong clientId)
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
                _visualPlaces.Add(MoneyPlace.Lost);
                _visualPlaceIndices.Add(0);
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
