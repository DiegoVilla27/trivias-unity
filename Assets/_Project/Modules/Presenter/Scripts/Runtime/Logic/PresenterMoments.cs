using Trivias.Questions.Logic;

namespace Trivias.Presenter.Logic
{
    /// <summary>
    /// Qué comenta el presentador en cada momento (GDD 9.1). C# puro: recibe solo datos públicos de la partida.
    /// </summary>
    public static class PresenterMoments
    {
        /// <summary>
        /// Al empezar una pregunta que no es la primera: la última, el inicio de un bloque o una de matemáticas.
        /// Las demás preguntas empiezan en silencio para no hablar en cada una.
        /// </summary>
        public static bool TryQuestionStart(int question, int total, bool isMath, out PresenterEvent presenterEvent)
        {
            presenterEvent = default;
            if (question <= 1 || question > total)
                return false;

            if (question == total)
                presenterEvent = PresenterEvent.LastQuestion;
            else if (QuestionPlanner.BlockOf(question - 1, total) != QuestionPlanner.BlockOf(question - 2, total))
                presenterEvent = QuestionPlanner.BlockOf(question - 1, total) == QuestionPlanner.DIFFICULTY_LEVELS - 1
                    ? PresenterEvent.HardBlock
                    : PresenterEvent.MediumBlock;
            else if (isMath)
                presenterEvent = PresenterEvent.MathQuestion;
            else
                return false;
            return true;
        }

        /// <summary>Comentario al revelar: expulsión, pérdida, multiplicador, pleno o acierto, por ese orden.</summary>
        public static PresenterEvent Reveal(bool someoneEliminated, long lost, long kept, long bonus)
        {
            if (someoneEliminated)
                return PresenterEvent.Eliminated;
            if (lost > kept)
                return PresenterEvent.Loss;
            if (bonus > 0)
                return PresenterEvent.Multiplied;
            return lost == 0 ? PresenterEvent.Perfect : PresenterEvent.Success;
        }
    }
}
