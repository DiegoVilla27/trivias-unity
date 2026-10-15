using UnityEngine;

namespace Trivias.Shared.Events
{
    [CreateAssetMenu(menuName = "Trivias/Events/Transform Event Channel", fileName = "TransformEventChannel")]
    public sealed class TransformEventChannel : EventChannel<Transform>
    {
    }
}
