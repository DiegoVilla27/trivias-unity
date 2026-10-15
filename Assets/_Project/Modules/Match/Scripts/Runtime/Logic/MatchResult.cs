namespace Trivias.Match.Logic
{
    /// <summary>Resultado final de la partida para la interfaz.</summary>
    public readonly struct MatchResult
    {
        public readonly bool Victory;
        public readonly int QuestionsPassed;
        public readonly long FinalMoney;
        public readonly float DurationSeconds;
        public readonly int TokensEach;
        public readonly string Survivors;
        public readonly string Eliminated;

        public MatchResult(bool victory, int questionsPassed, long finalMoney, float durationSeconds, int tokensEach,
            string survivors, string eliminated)
        {
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
