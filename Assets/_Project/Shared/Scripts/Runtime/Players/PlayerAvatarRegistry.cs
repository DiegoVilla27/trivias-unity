using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Players
{
    /// <summary>
    /// Concursantes presentes por id de cliente. Cada concursante se registra al aparecer en red y avisa cuando
    /// cambia su nombre o su estado; así cualquier módulo los consulta sin buscarlos en la escena.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Players/Player Avatar Registry", fileName = "PlayerAvatarRegistry")]
    public sealed class PlayerAvatarRegistry : ScriptableObject
    {
        private readonly Dictionary<ulong, IPlayerAvatar> _avatars = new();

        /// <summary>Se lanza al registrarse, retirarse o cambiar un concursante, con su id de cliente.</summary>
        public event Action<ulong> Changed;

        public IEnumerable<IPlayerAvatar> All => _avatars.Values;

        public void Register(IPlayerAvatar avatar)
        {
            _avatars[avatar.ClientId] = avatar;
            Changed?.Invoke(avatar.ClientId);
        }

        public void Unregister(ulong clientId)
        {
            if (_avatars.Remove(clientId))
                Changed?.Invoke(clientId);
        }

        public void NotifyChanged(ulong clientId) => Changed?.Invoke(clientId);

        public bool TryGet(ulong clientId, out IPlayerAvatar avatar) => _avatars.TryGetValue(clientId, out avatar);

        public bool IsEliminated(ulong clientId) => _avatars.TryGetValue(clientId, out var avatar) && avatar.IsEliminated;

        public string NameOf(ulong clientId) =>
            _avatars.TryGetValue(clientId, out var avatar) ? avatar.DisplayName : $"Jugador {clientId + 1}";

        private void OnEnable() => _avatars.Clear();
    }
}
