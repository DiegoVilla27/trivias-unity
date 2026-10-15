namespace Trivias.Match.Logic
{
    /// <summary>Fases de la partida (GDD 3.1). Las apuestas no tienen tiempo límite.</summary>
    public enum MatchPhase : byte
    {
        /// <summary>Sala abierta: esperando a que el host empiece.</summary>
        Lobby = 0,

        /// <summary>Pregunta en pantalla; se mueve dinero sin límite de tiempo hasta que alguien verifica.</summary>
        Betting = 1,

        /// <summary>Apuestas cerradas; cuenta atrás de 3 segundos.</summary>
        Locked = 2,

        /// <summary>Respuesta revelada y ronda resuelta.</summary>
        Reveal = 3,

        Victory = 4,
        Defeat = 5,
    }
}
