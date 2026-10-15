namespace Trivias.Feedback.Logic
{
    public enum Celebration : byte
    {
        None = 0,

        /// <summary>Pregunta acertada: confeti de colores a los lados de la pantalla.</summary>
        CorrectAnswer = 1,

        /// <summary>Las 15 preguntas superadas: lluvia de confeti dorado en toda la sala.</summary>
        Victory = 2,
    }
}
