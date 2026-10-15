namespace Trivias.Ranking.Logic
{
    /// <summary>Si una partida puede entrar en los récords.</summary>
    public enum RecordEligibility
    {
        Eligible,

        /// <summary>Solo las victorias entran (GDD 12).</summary>
        Defeat,

        /// <summary>Partida de prueba que no empezó en la pregunta 1.</summary>
        TestRun,

        /// <summary>Grupo fuera de las categorías (2 a 6 jugadores), p. ej. jugar solo en el editor.</summary>
        GroupSize,
    }

    public enum RecordResultKind
    {
        NotEligible,

        /// <summary>Primer puesto de su categoría (solo o compartido).</summary>
        NewRecord,

        /// <summary>Entra en la tabla, por debajo del primero.</summary>
        Ranked,

        /// <summary>Victoria válida que no entra entre las mejores de su categoría.</summary>
        OutOfTable,
    }

    /// <summary>Qué pasó al presentar una partida a los récords.</summary>
    public readonly struct RecordOutcome
    {
        public readonly RecordResultKind Kind;
        public readonly RecordEligibility Eligibility;

        /// <summary>Puesto en su categoría (1 = el mejor); 0 si no entra.</summary>
        public readonly int Position;

        public readonly int GroupSize;

        public RecordOutcome(RecordResultKind kind, RecordEligibility eligibility, int position, int groupSize)
        {
            Kind = kind;
            Eligibility = eligibility;
            Position = position;
            GroupSize = groupSize;
        }

        public static RecordOutcome NotEligible(RecordEligibility eligibility, int groupSize) =>
            new(RecordResultKind.NotEligible, eligibility, 0, groupSize);
    }
}
