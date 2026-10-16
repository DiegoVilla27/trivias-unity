using System;

namespace Trivias.Presenter.Logic
{
    /// <summary>
    /// Ritmo y movimiento de las letras de la viñeta (estilo cómic): a qué velocidad salen, dónde se para, cómo aparece
    /// cada letra y cómo se mueven según el ánimo. Los desplazamientos van en fracciones del tamaño de la letra.
    /// </summary>
    public static class SpeechRhythm
    {
        /// <summary>Segundos que tarda una letra nueva en pasar de grande a su tamaño.</summary>
        public const float POP_SECONDS = 0.14f;
        public const float POP_START_SCALE = 1.7f;

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

        /// <summary>Cada letra aparece grande y se encoge a su tamaño.</summary>
        public static float PopScale(float age)
        {
            if (age >= POP_SECONDS)
                return 1f;
            var left = 1f - Math.Max(0f, age) / POP_SECONDS;
            return 1f + (POP_START_SCALE - 1f) * left * left;
        }

        /// <summary>Movimiento continuo de la letra <paramref name="index"/> según el ánimo.</summary>
        public static void LetterOffset(PresenterMood mood, int index, float time, out float x, out float y)
        {
            x = 0f;
            switch (mood)
            {
                case PresenterMood.Excited:
                    y = 0.07f * Math.Abs((float)Math.Sin(time * 9f + index * 0.55f)); // Botan.
                    return;
                case PresenterMood.Suspense:
                    y = 0.05f * (float)Math.Sin(time * 2.6f + index * 0.45f); // Ondulan despacio.
                    x = 0.012f * (float)Math.Sin(time * 23f + index * 1.7f); // Tiemblan un poco.
                    return;
                case PresenterMood.Sad:
                    y = -0.05f * (0.5f + 0.5f * (float)Math.Sin(time * 1.4f + index * 0.3f)); // Se hunden.
                    return;
                default:
                    y = 0f;
                    return;
            }
        }

        /// <summary>Latido y giro de la viñeta entera según el ánimo.</summary>
        public static void BubbleWobble(PresenterMood mood, float time, out float scale, out float degrees)
        {
            switch (mood)
            {
                case PresenterMood.Excited:
                    scale = 1f + 0.03f * (float)Math.Sin(time * 7f);
                    degrees = 2.2f * (float)Math.Sin(time * 5f);
                    return;
                case PresenterMood.Suspense:
                    scale = 1f + 0.022f * (float)Math.Sin(time * 2.2f);
                    degrees = 0f;
                    return;
                case PresenterMood.Sad:
                    scale = 1f;
                    degrees = -3f + 0.6f * (float)Math.Sin(time * 1.5f);
                    return;
                default:
                    scale = 1f + 0.01f * (float)Math.Sin(time * 2f);
                    degrees = 0f;
                    return;
            }
        }

        /// <summary>Entrada de la viñeta: crece con un pequeño rebote (0 → 1 en <paramref name="t"/> = 0..1).</summary>
        public static float IntroScale(float t)
        {
            if (t >= 1f)
                return 1f;
            const float c1 = 1.9f;
            const float c3 = c1 + 1f;
            var x = Math.Max(0f, t) - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }

        /// <summary>Margen que se descuenta del tiempo de escritura para las pausas de los signos.</summary>
        private static float TotalPauseAllowance(PresenterMood mood) => mood == PresenterMood.Suspense ? 0.9f : 0.3f;
    }
}
