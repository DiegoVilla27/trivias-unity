using System;
using UnityEngine;

namespace Trivias.Shared.Events
{
    /// <summary>
    /// Canal de eventos como asset. Desacopla módulos: quien publica y quien escucha
    /// solo comparten la referencia al asset, no sus clases.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        private event Action<T> Raised;

        public void Register(Action<T> listener) => Raised += listener;

        public void Unregister(Action<T> listener) => Raised -= listener;

        public void Raise(T value) => Raised?.Invoke(value);
    }
}
