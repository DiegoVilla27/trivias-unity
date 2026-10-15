using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>Punto delante del pecho donde el concursante sujeta el dinero.</summary>
    public sealed class PlayerCarrySocket : MonoBehaviour
    {
        [SerializeField] private Transform _carryPoint;

        public Transform CarryPoint => _carryPoint;
    }
}
