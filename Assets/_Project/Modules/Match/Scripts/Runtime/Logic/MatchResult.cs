namespace Trivias.Match.Logic
{
    /// <summary>Resultado final de la partida para la interfaz.</summary>
    public readonly struct MatchResult
    {
        /// <summary>Identificador único de la partida (para entregar las fichas una sola vez).</summary>
        public readonly uint MatchId;
        public readonly bool Victory;
        public readonly int QuestionsPassed;
        public readonly long FinalMoney;
        public readonly float DurationSeconds;
        public readonly int TokensEach;
        public readonly string Survivors;
        public readonly string Eliminated;

        public MatchResult(uint matchId, bool victory, int questionsPassed, long finalMoney, float durationSeconds,
            int tokensEach, string survivors, string eliminated)
        {
            MatchId = matchId;
            Victory = victory;
            QuestionsPassed = questionsPassed;
            FinalMoney = finalMoney;
            DurationSeconds = durationSeconds;
            TokensEach = tokensEach;
            Survivors = survivors;
            Eliminated = eliminated;
        }

        /// <summary>Tiempo en formato mm:ss.</summary>
        public static string FormatDuration(float seconds)
        {
            var total = seconds < 0 ? 0 : (int)seconds;
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
