using Trivias.Shared.Carrying;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>Expone el punto delante del pecho donde el concursante sujeta el dinero.</summary>
    public sealed class PlayerCarrySocket : MonoBehaviour, ICarrySocket
    {
        [SerializeField] private Transform _carryPoint;

        public Transform CarryPoint => _carryPoint;
    }
}
