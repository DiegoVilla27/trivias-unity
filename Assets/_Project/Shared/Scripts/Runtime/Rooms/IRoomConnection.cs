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

        /// <summary>
        /// Host: direcciones de este ordenador para invitar a otros, la más útil primero (red virtual, luego local).
        /// Vacío si no es host.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<RoomAddress> ShareableAddresses { get; }

        void Host();
        void Join(string address);
        void Leave();
        void StartMatch();
    }
}
