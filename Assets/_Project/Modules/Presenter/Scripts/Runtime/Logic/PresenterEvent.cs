namespace Trivias.Presenter.Logic
{
    /// <summary>Momentos en los que habla el presentador (GDD 9.1).</summary>
    public enum PresenterEvent : byte
    {
        Welcome = 0,
        LongDiscussion = 1,
        SelfBet = 2,
        Countdown = 3,
        Success = 4,
        Loss = 5,
        Victory = 6,
        Defeat = 7,
    }

    public static class PresenterEvents
    {
        public const int COUNT = 8;

        /// <summary>Clave del evento en el archivo de contenido.</summary>
        public static bool TryParse(string key, out PresenterEvent presenterEvent)
        {
            switch (key)
            {
                case "welcome": presenterEvent = PresenterEvent.Welcome; return true;
                case "long_discussion": presenterEvent = PresenterEvent.LongDiscussion; return true;
                case "self_bet": presenterEvent = PresenterEvent.SelfBet; return true;
                case "countdown": presenterEvent = PresenterEvent.Countdown; return true;
                case "success": presenterEvent = PresenterEvent.Success; return true;
                case "loss": presenterEvent = PresenterEvent.Loss; return true;
                case "victory": presenterEvent = PresenterEvent.Victory; return true;
                case "defeat": presenterEvent = PresenterEvent.Defeat; return true;
                default: presenterEvent = default; return false;
            }
        }
    }
}
