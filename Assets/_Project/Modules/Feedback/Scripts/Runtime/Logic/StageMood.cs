namespace Trivias.Feedback.Logic
{
    /// <summary>Ambiente de luz del plató según el momento del concurso.</summary>
    public enum StageMood : byte
    {
        Lobby = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,

        /// <summary>Cuenta atrás tras verificar: la luz late con cada segundo.</summary>
        Countdown = 4,

        Correct = 5,
        Wrong = 6,
        Victory = 7,
        Defeat = 8,
    }

    public static class StageMoods
    {
        public const int COUNT = 9;
    }
}
