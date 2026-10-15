using System;
using System.Collections.Generic;
using System.Globalization;

namespace Trivias.Ranking.Logic
{
    /// <summary>
    /// Reglas de los récords (GDD 12). C# puro, cubierto por tests:
    /// - Solo victorias de partidas completas, por tamaño inicial del grupo (2 a 6).
    /// - Orden: dinero de mayor a menor; a igual dinero, menos tiempo (en milisegundos).
    /// - Dinero y tiempo idénticos comparten puesto (1, 1, 3...).
    /// - Cada categoría guarda las <see cref="TABLE_SIZE"/> mejores.
    /// </summary>
    public static class RankingRules
    {
        public const int MIN_GROUP = 2;
        public const int MAX_GROUP = 6;
        public const int TABLE_SIZE = 10;

        private static readonly Comparison<RecordEntry> ByRank = Compare;

        public static RecordEligibility Eligibility(bool victory, bool startedAtFirstQuestion, int groupSize)
        {
            if (!victory)
                return RecordEligibility.Defeat;
            if (!startedAtFirstQuestion)
                return RecordEligibility.TestRun;
            return groupSize < MIN_GROUP || groupSize > MAX_GROUP ? RecordEligibility.GroupSize : RecordEligibility.Eligible;
        }

        /// <summary>Negativo si <paramref name="a"/> va por delante de <paramref name="b"/>; 0 si empatan exactamente.</summary>
        public static int Compare(RecordEntry a, RecordEntry b)
        {
            if (a.money != b.money)
                return b.money.CompareTo(a.money);
            return a.durationMs.CompareTo(b.durationMs);
        }

        public static long DurationMs(float seconds) => seconds <= 0f ? 0L : (long)Math.Round(seconds * 1000.0);

        /// <summary>Fila de récord a partir del resultado: participantes = supervivientes y expulsados.</summary>
        public static RecordEntry Entry(string matchId, int groupSize, long money, float durationSeconds, string survivors,
            string eliminated, DateTime date)
        {
            survivors ??= string.Empty;
            eliminated ??= string.Empty;
            var players = survivors.Length == 0 ? eliminated
                : eliminated.Length == 0 ? survivors
                : survivors + ", " + eliminated;
            return new RecordEntry
            {
                matchId = matchId,
                groupSize = groupSize,
                money = money,
                durationMs = DurationMs(durationSeconds),
                players = players,
                survivors = survivors,
                date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            };
        }

        /// <summary>Récords de una categoría, del mejor al peor.</summary>
        public static void Category(RecordBook book, int groupSize, List<RecordEntry> into)
        {
            into.Clear();
            foreach (var entry in book.entries)
            {
                if (entry.groupSize == groupSize)
                    into.Add(entry);
            }
            into.Sort(ByRank);
        }

        /// <summary>Puesto de la fila <paramref name="index"/> de una categoría ordenada: 1 + los que van estrictamente por delante.</summary>
        public static int PositionAt(IReadOnlyList<RecordEntry> sorted, int index)
        {
            var position = index;
            while (position > 0 && Compare(sorted[position - 1], sorted[index]) == 0)
                position--;
            return position + 1;
        }

        /// <summary>
        /// Presenta una victoria a los récords: la añade si es válida, recorta su categoría a las mejores y dice en qué
        /// puesto queda. Presentar dos veces la misma partida no la duplica.
        /// </summary>
        public static RecordOutcome Submit(RecordBook book, RecordEntry entry, RecordEligibility eligibility,
            int tableSize = TABLE_SIZE)
        {
            if (eligibility != RecordEligibility.Eligible)
                return RecordOutcome.NotEligible(eligibility, entry.groupSize);

            var existing = IndexOfMatch(book, entry.matchId);
            var stored = existing >= 0 ? book.entries[existing] : entry;
            if (existing < 0)
                book.entries.Add(entry);

            var category = new List<RecordEntry>();
            Category(book, stored.groupSize, category);
            Trim(book, category, tableSize);

            var index = category.IndexOf(stored);
            if (index < 0)
                return new RecordOutcome(RecordResultKind.OutOfTable, eligibility, 0, entry.groupSize);

            var position = PositionAt(category, index);
            var kind = position == 1 ? RecordResultKind.NewRecord : RecordResultKind.Ranked;
            return new RecordOutcome(kind, eligibility, position, entry.groupSize);
        }

        /// <summary>Quita de la categoría (y del libro) lo que queda por debajo de los <paramref name="tableSize"/> primeros puestos.</summary>
        private static void Trim(RecordBook book, List<RecordEntry> category, int tableSize)
        {
            for (var i = category.Count - 1; i >= tableSize; i--)
            {
                if (PositionAt(category, i) <= tableSize)
                    continue; // Empata con un puesto de la tabla: se queda.
                book.entries.Remove(category[i]);
                category.RemoveAt(i);
            }
        }

        private static int IndexOfMatch(RecordBook book, string matchId)
        {
            if (string.IsNullOrEmpty(matchId))
                return -1;
            for (var i = 0; i < book.entries.Count; i++)
            {
                if (book.entries[i].matchId == matchId)
                    return i;
            }
            return -1;
        }
    }
}
