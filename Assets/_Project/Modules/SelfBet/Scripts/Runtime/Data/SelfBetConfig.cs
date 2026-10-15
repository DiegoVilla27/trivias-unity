using UnityEngine;

namespace Trivias.SelfBet.Data
{
    [CreateAssetMenu(menuName = "Trivias/SelfBet/Self Bet Config", fileName = "SelfBetConfig")]
    public sealed class SelfBetConfig : ScriptableObject
    {
        [Tooltip("Radio de la zona de apuesta delante de cada cabina.")]
        [SerializeField, Min(0.5f)] private float _zoneRadius = 1.3f;
        [Tooltip("Margen que acepta el host (latencia) al comprobar que quien se apuesta está en la zona.")]
        [SerializeField, Min(0f)] private float _serverTolerance = 0.5f;
        [Tooltip("Margen interior: el apostado se mueve dentro del círculo sin llegar a su borde.")]
        [SerializeField, Min(0f)] private float _insideMargin = 0.25f;

        public float ZoneRadius => _zoneRadius;
        public float ServerTolerance => _serverTolerance;

        /// <summary>Radio del círculo del que no puede salir un apostado.</summary>
        public float ConfinementRadius => Mathf.Max(0.1f, _zoneRadius - _insideMargin);
    }
}
