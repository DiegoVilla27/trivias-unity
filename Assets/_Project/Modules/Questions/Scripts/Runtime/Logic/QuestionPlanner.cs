using System;
using System.Collections.Generic;

namespace Trivias.Questions.Logic
{
    /// <summary>
    /// Elige las preguntas de una partida (GDD 6): sin repetir, en bloques de dificultad creciente
    /// (fácil, media, difícil). Si un bloque no tiene suficientes preguntas, completa con dificultades cercanas.
    /// Prefiere las que el grupo no ha visto (o vio hace más tiempo) y, entre las igual de nuevas, evita dos seguidas
    /// de la misma categoría. C# puro: recibe solo números del banco.
    /// </summary>
    public static class QuestionPlanner
    {
        public const int DIFFICULTY_LEVELS = 3;
        private const int NO_CATEGORY = -1;

        /// <summary>Bloque de dificultad (0, 1, 2) de la pregunta en la posición <paramref name="index"/> (desde 0).</summary>
        public static int BlockOf(int index, int total) => total <= 0 ? 0 : index * DIFFICULTY_LEVELS / total;

        /// <param name="difficulties">Dificultad (1-3) de cada pregunta del banco, por índice.</param>
        /// <param name="total">Preguntas por partida (15).</param>
        /// <returns>Índices del banco en el orden en que se preguntarán.</returns>
        public static int[] Plan(IReadOnlyList<int> difficulties, int total, Random random) =>
            Plan(difficulties, null, null, total, random);

        /// <param name="categories">Categoría de cada pregunta como número (o null si no importa).</param>
        /// <param name="penalties">
        /// Cuánto se ha visto cada pregunta (0 = nadie del grupo la ha visto; más alto = vista por más gente o más
        /// recientemente). Null si nadie tiene historial. Ver <see cref="QuestionHistoryRules.Penalties"/>.
        /// </param>
        public static int[] Plan(IReadOnlyList<int> difficulties, IReadOnlyList<int> categories, IReadOnlyList<int> penalties,
            int total, Random random)
        {
            if (difficulties.Count < total)
                throw new ArgumentException($"El banco tiene {difficulties.Count} preguntas y se necesitan {total}.");

            var pools = new List<int>[DIFFICULTY_LEVELS];
            for (var level = 0; level < DIFFICULTY_LEVELS; level++)
                pools[level] = new List<int>();
            for (var i = 0; i < difficulties.Count; i++)
                pools[Clamp(difficulties[i]) - 1].Add(i);
            foreach (var pool in pools)
                ShuffleThenSortByPenalty(pool, penalties, random);

            var plan = new int[total];
            var previousCategory = NO_CATEGORY;
            for (var n = 0; n < total; n++)
            {
                plan[n] = TakeNearest(pools, BlockOf(n, total), categories, penalties, previousCategory);
                previousCategory = categories == null ? NO_CATEGORY : categories[plan[n]];
            }
            return plan;
        }

        private static int TakeNearest(List<int>[] pools, int level, IReadOnlyList<int> categories, IReadOnlyList<int> penalties,
            int previousCategory)
        {
            for (var distance = 0; distance < DIFFICULTY_LEVELS; distance++)
            {
                foreach (var candidate in new[] { level - distance, level + distance })
                {
                    if (candidate < 0 || candidate >= DIFFICULTY_LEVELS || pools[candidate].Count == 0)
                        continue;
                    return TakeFrom(pools[candidate], categories, penalties, previousCategory);
                }
            }
            throw new InvalidOperationException("No quedan preguntas.");
        }

        /// <summary>
        /// Toma la primera de la lista (la menos vista). Si es de la misma categoría que la anterior, busca otra igual de
        /// nueva: la variedad nunca se consigue a costa de repetir una pregunta más vista.
        /// </summary>
        private static int TakeFrom(List<int> pool, IReadOnlyList<int> categories, IReadOnlyList<int> penalties, int previousCategory)
        {
            var chosen = 0;
            if (categories != null && previousCategory != NO_CATEGORY)
            {
                var bestPenalty = PenaltyOf(penalties, pool[0]);
                for (var i = 0; i < pool.Count && PenaltyOf(penalties, pool[i]) == bestPenalty; i++)
                {
                    if (categories[pool[i]] != previousCategory)
                    {
                        chosen = i;
                        break;
                    }
                }
            }

            var index = pool[chosen];
            pool.RemoveAt(chosen);
            return index;
        }

        /// <summary>Baraja y después ordena de forma estable por penalización: el azar solo decide entre las igual de vistas.</summary>
        private static void ShuffleThenSortByPenalty(List<int> list, IReadOnlyList<int> penalties, Random random)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            if (penalties == null)
                return;
            // Inserción estable (las listas son pequeñas y esto se hace una vez por partida).
            for (var i = 1; i < list.Count; i++)
            {
                var item = list[i];
                var penalty = PenaltyOf(penalties, item);
                var j = i - 1;
                while (j >= 0 && PenaltyOf(penalties, list[j]) > penalty)
                {
                    list[j + 1] = list[j];
                    j--;
                }
                list[j + 1] = item;
            }
        }

        private static int PenaltyOf(IReadOnlyList<int> penalties, int index) =>
            penalties == null || index >= penalties.Count ? 0 : penalties[index];

        private static int Clamp(int difficulty) => difficulty < 1 ? 1 : difficulty > DIFFICULTY_LEVELS ? DIFFICULTY_LEVELS : difficulty;
    }
}
