using UnityEngine;

namespace Trivias.Shared.Events
{
    [CreateAssetMenu(menuName = "Trivias/Events/String Event Channel", fileName = "StringEventChannel")]
    public sealed class StringEventChannel : EventChannel<string>
    {
    }
}
