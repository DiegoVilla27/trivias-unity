using System.Collections.Generic;
using Trivias.Match.Logic;

namespace Trivias.Feedback.Logic
{
    /// <summary>Decide qué suena al cambiar el estado (C# puro, probado en EditMode).</summary>
    public static class FeedbackRules
    {
        /// <summary>El reloj solo hace tic en los últimos segundos de las apuestas con tiempo.</summary>
        public const int CLOCK_WARNING_SECONDS = 10;

        public const int NO_MUSIC = -1;
        public const int MUSIC_BLOCKS = 3;

        /// <summary>Sonidos de la partida entre dos instantes. Añade a <paramref name="cues"/> (sin vaciarla).</summary>
        public static void MatchCues(in MatchSnapshot previous, in MatchSnapshot current, List<FeedbackCue> cues)
        {
            if (current.Phase != previous.Phase)
            {
                switch (current.Phase)
                {
                    case MatchPhase.Betting:
                        if (current.Question != previous.Question)
                            cues.Add(FeedbackCue.QuestionShown);
                        break;
                    case MatchPhase.Locked:
                        cues.Add(FeedbackCue.Lock);
                        cues.Add(FeedbackCue.CountdownBeep);
                        break;
                    case MatchPhase.Reveal:
                        cues.Add(FeedbackCue.Trapdoor);
                        cues.Add(current.Kept > 0 ? FeedbackCue.Success : FeedbackCue.Fail);
                        if (current.Kept > 0 && current.Multiplier > 1)
                            cues.Add(FeedbackCue.Bonus);
                        if (current.SomeoneExpelled)
                            cues.Add(FeedbackCue.Expelled);
                        break;
                    case MatchPhase.Victory:
                        cues.Add(FeedbackCue.Victory);
                        break;
                    case MatchPhase.Defeat:
                        cues.Add(FeedbackCue.Defeat);
                        break;
                }
                return;
            }

            if (current.SecondsLeft >= previous.SecondsLeft || current.SecondsLeft <= 0)
                return;

            if (current.Phase == MatchPhase.Locked)
                cues.Add(FeedbackCue.CountdownBeep);
            else if (current.Phase == MatchPhase.Betting && current.IsTimed && current.SecondsLeft <= CLOCK_WARNING_SECONDS)
                cues.Add(FeedbackCue.ClockTick);
        }

        /// <summary>
        /// Confeti: de colores a los lados de la pantalla en cada acierto; lluvia dorada en toda la sala al ganar.
        /// </summary>
        public static Celebration CelebrationFor(in MatchSnapshot previous, in MatchSnapshot current)
        {
            if (current.Phase == previous.Phase)
                return Celebration.None;
            if (current.Phase == MatchPhase.Victory)
                return Celebration.Victory;
            return current.Phase == MatchPhase.Reveal && current.Kept > 0 ? Celebration.CorrectAnswer : Celebration.None;
        }

        /// <summary>Coger o dejar dinero: solo cuenta mientras el dinero se puede mover (no al resolver la ronda).</summary>
        public static bool TryMoneyCue(int previousCarried, int currentCarried, bool moneyLocked, out FeedbackCue cue)
        {
            cue = currentCarried > previousCarried ? FeedbackCue.MoneyPick : FeedbackCue.MoneyDrop;
            return !moneyLocked && currentCarried != previousCarried;
        }

        /// <summary>Alguien se apuesta: sube el número de apostados durante las apuestas.</summary>
        public static bool IsNewBet(int previousBets, int currentBets, MatchPhase phase) =>
            phase == MatchPhase.Betting && currentBets > previousBets;

        /// <summary>Música por bloque de dificultad: preguntas fáciles (0), medias (1) y difíciles (2); sin música fuera de la partida.</summary>
        public static int MusicBlock(MatchPhase phase, int question, int totalQuestions)
        {
            if (phase == MatchPhase.Lobby || MatchRules.IsFinished(phase) || question <= 0 || totalQuestions <= 0)
                return NO_MUSIC;
            var block = (question - 1) * MUSIC_BLOCKS / totalQuestions;
            return block < MUSIC_BLOCKS ? block : MUSIC_BLOCKS - 1;
        }
    }
}
