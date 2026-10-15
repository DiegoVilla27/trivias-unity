namespace Trivias.Shared.Rooms
{
    /// <summary>
    /// Lo que el menú necesita de la conexión a la sala, sin conocer el módulo de red ni el transporte
    /// (local ahora, Steam más adelante).
    /// </summary>
    public interface IRoomConnection
    {
        RoomConnectionState State { get; }

        /// <summary>Último mensaje para el jugador (error de conexión, sala llena...).</summary>
        string StatusMessage { get; }

        bool IsHost { get; }
        int MaxPlayers { get; }
        int MinPlayersToStart { get; }
        bool CanStartMatch { get; }
        string DefaultAddress { get; }

        void Host();
        void Join(string address);
        void Leave();
        void StartMatch();
    }
}
