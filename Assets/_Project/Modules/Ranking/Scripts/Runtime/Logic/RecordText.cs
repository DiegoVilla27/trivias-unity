namespace Trivias.Ranking.Logic
{
    /// <summary>Textos de los récords para resultados y menú. C# puro, cubierto por tests.</summary>
    public static class RecordText
    {
        /// <summary>«4 jugadores».</summary>
        public static string Category(int groupSize) => $"{groupSize} jugadores";

        /// <summary>Tiempo en formato mm:ss (los milisegundos solo desempatan, no se muestran).</summary>
        public static string Duration(long milliseconds)
        {
            var total = milliseconds < 0 ? 0 : milliseconds / 1000;
            return $"{total / 60:00}:{total % 60:00}";
        }

        /// <summary>«2026-10-15» → «15/10/2026». Si no tiene ese formato, se muestra tal cual.</summary>
        public static string Date(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate) || isoDate.Length != 10 || isoDate[4] != '-' || isoDate[7] != '-')
                return isoDate ?? string.Empty;
            return $"{isoDate.Substring(8, 2)}/{isoDate.Substring(5, 2)}/{isoDate.Substring(0, 4)}";
        }

        /// <summary>«2026-10-15» → «15/10/26» (tabla de récords). Si no tiene ese formato, se muestra tal cual.</summary>
        public static string ShortDate(string isoDate)
        {
            var date = Date(isoDate);
            return date.Length == 10 && date[2] == '/' && date[5] == '/' ? date.Substring(0, 6) + date.Substring(8, 2) : date;
        }

        /// <summary>Línea de la pantalla de resultados sobre los récords.</summary>
        public static string Outcome(RecordOutcome outcome) => outcome.Kind switch
        {
            RecordResultKind.NewRecord => $"¡NUEVO RÉCORD! Primer puesto con {Category(outcome.GroupSize)}",
            RecordResultKind.Ranked => $"Puesto {outcome.Position} de tus récords con {Category(outcome.GroupSize)}",
            RecordResultKind.OutOfTable =>
                $"No entra en tus {RankingRules.TABLE_SIZE} mejores victorias con {Category(outcome.GroupSize)}",
            _ => outcome.Eligibility switch
            {
                RecordEligibility.TestRun => "Partida de prueba (no empezó en la pregunta 1): no cuenta para los récords",
                RecordEligibility.GroupSize =>
                    $"Los récords son para grupos de {RankingRules.MIN_GROUP} a {RankingRules.MAX_GROUP} jugadores",
                _ => string.Empty,
            },
        };
    }
}
