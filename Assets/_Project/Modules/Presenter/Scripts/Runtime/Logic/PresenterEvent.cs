namespace Trivias.Presenter.Logic
{
    /// <summary>Momentos en los que habla el presentador (GDD 9.1).</summary>
    public enum PresenterEvent : byte
    {
        Welcome = 0,
        LongDiscussion = 1,
        SelfBet = 2,
        Countdown = 3,

        /// <summary>Acierto con alguna pérdida (se conserva más de lo que se pierde).</summary>
        Success = 4,

        /// <summary>Se pierde más de lo que se conserva, sin expulsiones.</summary>
        Loss = 5,
        Victory = 6,
        Defeat = 7,

        /// <summary>Quedan pocos segundos para apostar (solo con límite de tiempo).</summary>
        TimeRunningOut = 8,

        /// <summary>Un concursante se ha desconectado a mitad de partida.</summary>
        PlayerLeft = 9,

        /// <summary>Empieza el bloque de preguntas medias.</summary>
        MediumBlock = 10,

        /// <summary>Empieza el bloque de preguntas difíciles.</summary>
        HardBlock = 11,

        /// <summary>Empieza la última pregunta.</summary>
        LastQuestion = 12,

        /// <summary>Empieza una pregunta de matemáticas.</summary>
        MathQuestion = 13,

        /// <summary>Alguien apostado en una respuesta falsa ha sido expulsado.</summary>
        Eliminated = 14,

        /// <summary>Los apostados en la correcta han multiplicado el premio.</summary>
        Multiplied = 15,

        /// <summary>Acierto sin perder nada.</summary>
        Perfect = 16,
    }

    public static class PresenterEvents
    {
        public const int COUNT = 17;

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
                case "time_running_out": presenterEvent = PresenterEvent.TimeRunningOut; return true;
                case "player_left": presenterEvent = PresenterEvent.PlayerLeft; return true;
                case "medium_block": presenterEvent = PresenterEvent.MediumBlock; return true;
                case "hard_block": presenterEvent = PresenterEvent.HardBlock; return true;
                case "last_question": presenterEvent = PresenterEvent.LastQuestion; return true;
                case "math_question": presenterEvent = PresenterEvent.MathQuestion; return true;
                case "eliminated": presenterEvent = PresenterEvent.Eliminated; return true;
                case "multiplied": presenterEvent = PresenterEvent.Multiplied; return true;
                case "perfect": presenterEvent = PresenterEvent.Perfect; return true;
                default: presenterEvent = default; return false;
            }
        }
    }
}
