using System;

namespace Trivias.Presenter.Logic
{
    /// <summary>
    /// Ritmo de las letras del rótulo del presentador: a qué velocidad salen según el ánimo y dónde se para. Sin
    /// botes ni temblores: el texto se escribe tranquilo y se lee bien.
    /// </summary>
    public static class SpeechRhythm
    {
        /// <summary>Parte del tiempo de la frase en que debe estar escrita entera (el resto es para leerla).</summary>
        public const float TYPING_SHARE = 0.5f;

        /// <summary>Letras por segundo: cada ánimo a su ritmo, pero la frase siempre se termina a tiempo de leerla.</summary>
        public static float CharsPerSecond(PresenterMood mood, int length, float lineSeconds)
        {
            var natural = mood switch
            {
                PresenterMood.Excited => 46f,
                PresenterMood.Suspense => 20f,
                PresenterMood.Sad => 24f,
                _ => 34f,
            };
            var needed = length / Math.Max(0.5f, lineSeconds * TYPING_SHARE - TotalPauseAllowance(mood));
            return Math.Max(natural, needed);
        }

        /// <summary>Pausa después de un signo: en suspense, los puntos se hacen esperar.</summary>
        public static float PauseAfter(char character, PresenterMood mood)
        {
            var suspense = mood == PresenterMood.Suspense;
            switch (character)
            {
                case '.':
                case '…':
                case '!':
                case '?':
                    return suspense ? 0.32f : 0.1f;
                case ',':
                case ':':
                case ';':
                    return suspense ? 0.14f : 0.04f;
                default:
                    return 0f;
            }
        }

        /// <summary>Margen que se descuenta del tiempo de escritura para las pausas de los signos.</summary>
        private static float TotalPauseAllowance(PresenterMood mood) => mood == PresenterMood.Suspense ? 0.9f : 0.3f;
    }
}
