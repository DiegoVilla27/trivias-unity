namespace Trivias.Feedback.Logic
{
    /// <summary>Momentos del juego que suenan (GDD 13: «sonido y sensaciones»).</summary>
    public enum FeedbackCue : byte
    {
        /// <summary>El jugador local coge dinero.</summary>
        MoneyPick = 0,

        /// <summary>El jugador local deja o devuelve dinero.</summary>
        MoneyDrop = 1,

        /// <summary>Alguien se apuesta en una cabina.</summary>
        BetPlaced = 2,

        /// <summary>Aparece una pregunta nueva en la pantalla.</summary>
        QuestionShown = 3,

        /// <summary>Tic del reloj en los últimos segundos de las apuestas con tiempo.</summary>
        ClockTick = 4,

        /// <summary>Apuestas cerradas (verificar).</summary>
        Lock = 5,

        /// <summary>Cada segundo de la cuenta atrás tras verificar.</summary>
        CountdownBeep = 6,

        /// <summary>Se abren las trampillas.</summary>
        Trapdoor = 7,

        /// <summary>El equipo conserva dinero.</summary>
        Success = 8,

        /// <summary>El equipo lo pierde todo en la ronda.</summary>
        Fail = 9,

        /// <summary>Alguien es expulsado.</summary>
        Expelled = 10,

        /// <summary>El comodín multiplica el dinero.</summary>
        Bonus = 11,

        Victory = 12,
        Defeat = 13,
    }

    public static class FeedbackCues
    {
        public const int COUNT = 14;
    }
}
