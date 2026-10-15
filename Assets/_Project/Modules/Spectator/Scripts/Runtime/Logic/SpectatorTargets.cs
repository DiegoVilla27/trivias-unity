using System.Collections.Generic;

namespace Trivias.Spectator.Logic
{
    /// <summary>
    /// A quién mira el espectador (C# puro, cubierto por tests). Los candidatos son los concursantes activos, ordenados
    /// por id de cliente, sin contarse a sí mismo. Cambiar de concursante da la vuelta al llegar al final.
    /// </summary>
    public static class SpectatorTargets
    {
        public const int NONE = -1;

        /// <summary>
        /// Índice del candidato al que pasar desde <paramref name="current"/> (<paramref name="step"/>: +1 siguiente,
        /// -1 anterior, 0 quedarse). Si el actual ya no está, se elige el siguiente id mayor (o el primero).
        /// <see cref="NONE"/> si no queda nadie a quien mirar.
        /// </summary>
        public static int Step(IReadOnlyList<ulong> sortedCandidates, bool hasCurrent, ulong current, int step)
        {
            var count = sortedCandidates.Count;
            if (count == 0)
                return NONE;
            if (!hasCurrent)
                return step < 0 ? count - 1 : 0;

            var index = IndexOf(sortedCandidates, current);
            if (index < 0)
            {
                // El que mirábamos cayó o se fue: el siguiente en el orden (o el anterior, si íbamos hacia atrás).
                var after = FirstGreater(sortedCandidates, current);
                if (step < 0)
                    return after == 0 ? count - 1 : after - 1;
                return after >= count ? 0 : after;
            }

            return ((index + step) % count + count) % count;
        }

        private static int IndexOf(IReadOnlyList<ulong> list, ulong value)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                    return i;
            }
            return -1;
        }

        private static int FirstGreater(IReadOnlyList<ulong> list, ulong value)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] > value)
                    return i;
            }
            return list.Count;
        }
    }
}
