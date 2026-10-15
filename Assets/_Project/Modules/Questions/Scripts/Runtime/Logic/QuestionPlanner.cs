using System;
using System.Collections.Generic;

namespace Trivias.Questions.Logic
{
    /// <summary>
    /// Elige las preguntas de una partida (GDD 6): sin repetir, en bloques de dificultad creciente
    /// (fácil, media, difícil). Si un bloque no tiene suficientes preguntas, completa con dificultades cercanas.
    /// C# puro: recibe solo las dificultades del banco.
    /// </summary>
    public static class QuestionPlanner
    {
        public const int DIFFICULTY_LEVELS = 3;

        /// <param name="difficulties">Dificultad (1-3) de cada pregunta del banco, por índice.</param>
        /// <param name="total">Preguntas por partida (15).</param>
        /// <returns>Índices del banco en el orden en que se preguntarán.</returns>
        public static int[] Plan(IReadOnlyList<int> difficulties, int total, Random random)
        {
            if (difficulties.Count < total)
                throw new ArgumentException($"El banco tiene {difficulties.Count} preguntas y se necesitan {total}.");

            var pools = new List<int>[DIFFICULTY_LEVELS];
            for (var level = 0; level < DIFFICULTY_LEVELS; level++)
                pools[level] = new List<int>();
            for (var i = 0; i < difficulties.Count; i++)
                pools[Clamp(difficulties[i]) - 1].Add(i);
            foreach (var pool in pools)
                ShuffleInPlace(pool, random);

            var plan = new int[total];
            for (var n = 0; n < total; n++)
            {
                var block = n * DIFFICULTY_LEVELS / total; // 0, 1, 2 en tercios
                plan[n] = TakeNearest(pools, block);
            }
            return plan;
        }

        private static int TakeNearest(List<int>[] pools, int level)
        {
            for (var distance = 0; distance < DIFFICULTY_LEVELS; distance++)
            {
                foreach (var candidate in new[] { level - distance, level + distance })
                {
                    if (candidate < 0 || candidate >= DIFFICULTY_LEVELS || pools[candidate].Count == 0)
                        continue;
                    var pool = pools[candidate];
                    var index = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                    return index;
                }
            }
            throw new InvalidOperationException("No quedan preguntas.");
        }

        private static void ShuffleInPlace(List<int> list, Random random)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static int Clamp(int difficulty) => difficulty < 1 ? 1 : difficulty > DIFFICULTY_LEVELS ? DIFFICULTY_LEVELS : difficulty;
    }
}
