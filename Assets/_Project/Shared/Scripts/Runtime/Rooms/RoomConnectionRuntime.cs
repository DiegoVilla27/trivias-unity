using System;
using UnityEngine;

namespace Trivias.Shared.Rooms
{
    /// <summary>Anclaje en runtime de la conexión a la sala; el menú la consulta aquí y se entera de sus cambios.</summary>
    [CreateAssetMenu(menuName = "Trivias/Rooms/Room Connection Runtime", fileName = "RoomConnectionRuntime")]
    public sealed class RoomConnectionRuntime : ScriptableObject
    {
        public event Action Changed;

        public IRoomConnection Connection { get; private set; }

        public void Attach(IRoomConnection connection)
        {
            Connection = connection;
            Changed?.Invoke();
        }

        public void Detach(IRoomConnection connection)
        {
            if (Connection != connection)
                return;
            Connection = null;
            Changed?.Invoke();
        }

        public void NotifyChanged() => Changed?.Invoke();

        private void OnEnable() => Connection = null;
    }
}
