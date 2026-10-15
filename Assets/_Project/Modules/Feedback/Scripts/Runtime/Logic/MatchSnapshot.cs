using Trivias.Match.Logic;

namespace Trivias.Feedback.Logic
{
    /// <summary>Lo que el sonido necesita saber de la partida en un instante. Inmutable y sin allocations.</summary>
    public readonly struct MatchSnapshot
    {
        public readonly MatchPhase Phase;
        public readonly int Question;

        /// <summary>Segundos que quedan de la fase, redondeados hacia arriba (0 si no tiene tiempo).</summary>
        public readonly int SecondsLeft;

        public readonly bool IsTimed;

        /// <summary>Dinero conservado en la última ronda.</summary>
        public readonly long Kept;

        public readonly int Multiplier;
        public readonly bool SomeoneExpelled;

        public MatchSnapshot(MatchPhase phase, int question, int secondsLeft, bool isTimed, long kept, int multiplier,
            bool someoneExpelled)
        {
            Phase = phase;
            Question = question;
            SecondsLeft = secondsLeft;
            IsTimed = isTimed;
            Kept = kept;
            Multiplier = multiplier;
            SomeoneExpelled = someoneExpelled;
        }

        public static MatchSnapshot Lobby => new(MatchPhase.Lobby, 0, 0, false, 0, 1, false);
    }
}
