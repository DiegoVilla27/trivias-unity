using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Carrying
{
    /// <summary>
    /// Puntos de carga de los concursantes por id de cliente. Cada concursante se registra al aparecer en red,
    /// así cualquier cliente sabe dónde dibujar lo que lleva otro jugador.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Carrying/Carry Socket Registry", fileName = "CarrySocketRegistry")]
    public sealed class CarrySocketRegistry : ScriptableObject
    {
        private readonly Dictionary<ulong, ICarrySocket> _sockets = new();

        /// <summary>Se lanza al registrarse o retirarse un concursante, con su id de cliente.</summary>
        public event Action<ulong> Changed;

        public void Register(ulong clientId, ICarrySocket socket)
        {
            _sockets[clientId] = socket;
            Changed?.Invoke(clientId);
        }

        public void Unregister(ulong clientId)
        {
            if (_sockets.Remove(clientId))
                Changed?.Invoke(clientId);
        }

        public bool TryGet(ulong clientId, out ICarrySocket socket) => _sockets.TryGetValue(clientId, out socket);

        private void OnEnable() => _sockets.Clear();
    }
}
