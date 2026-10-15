using Trivias.Money.Logic;
using UnityEngine;

namespace Trivias.Money.Data
{
    /// <summary>Balance del dinero (GDD 4.1, propuesta por validar). Nunca se escriben estos valores en el código.</summary>
    [CreateAssetMenu(menuName = "Trivias/Money/Money Config", fileName = "MoneyConfig")]
    public sealed class MoneyConfig : ScriptableObject
    {
        [Header("Capital y denominaciones")]
        [SerializeField] private long _startingCapital = 100_000;
        [SerializeField] private long _bundleValue = 5_000;
        [SerializeField] private long _bagValue = 50_000;
        [SerializeField] private long _goldBarValue = 500_000;

        [Header("Transporte (un solo tipo por viaje)")]
        [Tooltip("Fajos que se pueden llevar a la vez.")]
        [SerializeField, Range(1, 20)] private int _maxCarriedBundles = 5;
        [Tooltip("Bolsas que se pueden llevar a la vez.")]
        [SerializeField, Range(1, 20)] private int _maxCarriedBags = 2;
        [Tooltip("Lingotes que se pueden llevar a la vez.")]
        [SerializeField, Range(1, 20)] private int _maxCarriedGoldBars = 5;

        [Header("Interacción")]
        [SerializeField, Min(0.5f)] private float _tableReach = 2.4f;
        [SerializeField, Min(0.5f)] private float _trayReach = 1.6f;
        [Tooltip("Margen extra que acepta el host al validar la distancia (latencia de red).")]
        [SerializeField, Min(0f)] private float _serverReachTolerance = 1.0f;

        [Header("Pool de objetos visibles (precalentado al cargar)")]
        [SerializeField, Min(0)] private int _bundlePool = 40;
        [SerializeField, Min(0)] private int _bagPool = 20;
        [SerializeField, Min(0)] private int _goldBarPool = 12;

        public long StartingCapital => _startingCapital;

        public float TableReach => _tableReach;
        public float TrayReach => _trayReach;
        public float ServerReachTolerance => _serverReachTolerance;

        /// <summary>El capital inicial se entrega solo en fajos (GDD 3.1).</summary>
        public int StartingBundles => (int)(_startingCapital / _bundleValue);

        public DenominationValues CreateValues() => new(_bundleValue, _bagValue, _goldBarValue);

        public CarryLimits CreateCarryLimits() => new(_maxCarriedBundles, _maxCarriedBags, _maxCarriedGoldBars);

        public int PoolSize(Denomination denomination) => denomination switch
        {
            Denomination.Bundle => _bundlePool,
            Denomination.Bag => _bagPool,
            _ => _goldBarPool,
        };

        private void OnValidate()
        {
            if (_startingCapital % _bundleValue != 0)
                Debug.LogWarning($"[{name}] El capital inicial debe ser múltiplo del valor del fajo.", this);
        }
    }
}
