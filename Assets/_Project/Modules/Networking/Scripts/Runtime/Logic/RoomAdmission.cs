namespace Trivias.Networking.Logic
{
    /// <summary>Reglas de entrada a la sala y de inicio de partida. C# puro, cubierto por tests EditMode.</summary>
    public static class RoomAdmission
    {
        public const string ROOM_FULL_REASON = "La sala está llena (máximo 6 jugadores).";
        public const string MATCH_STARTED_REASON = "La partida ya empezó. Espera a la siguiente.";

        /// <summary>Devuelve null si el jugador puede entrar, o el motivo del rechazo.</summary>
        public static string RejectionReason(int connectedPlayers, int maxPlayers, bool matchStarted)
        {
            if (matchStarted)
                return MATCH_STARTED_REASON;
            return connectedPlayers >= maxPlayers ? ROOM_FULL_REASON : null;
        }

        public static bool CanStartMatch(int connectedPlayers, int minPlayers, int maxPlayers, bool matchStarted) =>
            !matchStarted && connectedPlayers >= minPlayers && connectedPlayers <= maxPlayers;
    }
}
