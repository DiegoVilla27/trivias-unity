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

        public static bool AcceptsBets(MatchPhase phase) => phase == MatchPhase.Lobby || phase == MatchPhase.Betting;

        public static bool IsFinished(MatchPhase phase) => phase == MatchPhase.Victory || phase == MatchPhase.Defeat;
    }
}
