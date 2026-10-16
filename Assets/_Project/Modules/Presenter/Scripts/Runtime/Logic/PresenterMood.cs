namespace Trivias.Presenter.Logic
{
    /// <summary>Cómo dice el presentador su frase: cambia la forma y el color de la viñeta y cómo salen las letras.</summary>
    public enum PresenterMood : byte
    {
        Neutral = 0,

        /// <summary>Ánimo y celebración: viñeta de estallido, letras que botan.</summary>
        Excited = 1,

        /// <summary>Suspense: viñeta oscura, letras lentas que ondulan y pausas en los puntos.</summary>
        Suspense = 2,

        /// <summary>Pena: viñeta azulada y caída, letras que se hunden.</summary>
        Sad = 3,
    }

    public static class PresenterMoods
    {
        public const int COUNT = 4;

        public static PresenterMood For(PresenterEvent presenterEvent)
        {
            switch (presenterEvent)
            {
                case PresenterEvent.Welcome:
                case PresenterEvent.Success:
                case PresenterEvent.Victory:
                case PresenterEvent.Multiplied:
                case PresenterEvent.Perfect:
                    return PresenterMood.Excited;
                case PresenterEvent.SelfBet:
                case PresenterEvent.Countdown:
                case PresenterEvent.TimeRunningOut:
                case PresenterEvent.HardBlock:
                case PresenterEvent.LastQuestion:
                    return PresenterMood.Suspense;
                case PresenterEvent.Loss:
                case PresenterEvent.Defeat:
                case PresenterEvent.PlayerLeft:
                case PresenterEvent.Eliminated:
                    return PresenterMood.Sad;
                default:
                    return PresenterMood.Neutral;
            }
        }
    }
}
