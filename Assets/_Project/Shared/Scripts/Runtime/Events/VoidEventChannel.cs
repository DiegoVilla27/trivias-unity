using System;
using UnityEngine;

namespace Trivias.Shared.Events
{
    [CreateAssetMenu(menuName = "Trivias/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public sealed class VoidEventChannel : ScriptableObject
    {
        private event Action Raised;

        public void Register(Action listener) => Raised += listener;

        public void Unregister(Action listener) => Raised -= listener;

        public void Raise() => Raised?.Invoke();
    }
}
