namespace Trivias.Match.Logic
{
    /// <summary>Reglas de victoria y derrota (GDD 3.2). C# puro, cubierto por tests.</summary>
    public static class MatchRules
    {
        /// <summary>
        /// Qué pasa tras revelar la pregunta <paramref name="questionNumber"/> (empezando en 1): derrota sin dinero o sin
        /// concursantes activos; victoria al resolver la última con ambos; si no, siguiente pregunta.
        /// </summary>
        public static MatchPhase AfterReveal(long teamMoney, int activePlayers, int questionNumber, int totalQuestions)
        {
            if (teamMoney <= 0 || activePlayers <= 0)
                return MatchPhase.Defeat;
            return questionNumber >= totalQuestions ? MatchPhase.Victory : MatchPhase.Betting;
        }

        /// <summary>Pregunta con la que empieza la partida (pruebas): siempre entre 1 y el total.</summary>
        public static int StartQuestion(int requested, int totalQuestions) =>
            totalQuestions <= 0 ? 1 : requested < 1 ? 1 : requested > totalQuestions ? totalQuestions : requested;

        /// <summary>
        /// Preguntas superadas: las resueltas dejando dinero. Perder en la pregunta 15 cuenta como 14 (GDD 11).
        /// </summary>
        public static int QuestionsPassed(MatchPhase phase, int questionNumber) => phase switch
        {
            MatchPhase.Victory => questionNumber,
            MatchPhase.Defeat => questionNumber - 1,
            MatchPhase.Reveal => questionNumber,
            _ => questionNumber > 0 ? questionNumber - 1 : 0,
        };

        /// <summary>
        /// Fichas para cada jugador (GDD 11): se entrega solo el tramo alcanzado. Cada tramo exige un mínimo de preguntas
        /// superadas; el último (las 15) exige además la victoria. <paramref name="minQuestions"/> va en orden creciente.
        /// </summary>
        public static int TokensFor(int questionsPassed, bool victory, int[] minQuestions, int[] tokens, int totalQuestions)
        {
            var reward = 0;
            for (var tier = 0; tier < minQuestions.Length; tier++)
            {
                var needsVictory = minQuestions[tier] >= totalQuestions;
                if (questionsPassed >= minQuestions[tier] && (!needsVictory || victory))
                    reward = tokens[tier];
            }
            return reward;
        }

        public static bool AcceptsBets(MatchPhase phase) => phase == MatchPhase.Lobby || phase == MatchPhase.Betting;

        public static bool IsFinished(MatchPhase phase) => phase == MatchPhase.Victory || phase == MatchPhase.Defeat;
    }
}
