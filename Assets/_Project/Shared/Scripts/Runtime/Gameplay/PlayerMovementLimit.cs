using UnityEngine;

namespace Trivias.Shared.Gameplay
{
    /// <summary>
    /// Límite de movimiento del concursante local: mientras está apostado no puede salir del círculo de su cabina.
    /// Lo fija el módulo de comodines y lo aplica el movimiento del concursante, sin conocerse entre sí.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Gameplay/Player Movement Limit", fileName = "PlayerMovementLimit")]
    public sealed class PlayerMovementLimit : ScriptableObject
    {
        public bool HasZone { get; private set; }
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }

        public void SetZone(Vector3 center, float radius)
        {
            HasZone = true;
            Center = center;
            Radius = radius;
        }

        public void Clear() => HasZone = false;

        private void OnEnable() => HasZone = false;
    }
}
