using System.Collections.Generic;
using Trivias.Money.Logic;
using Trivias.Ranking.Logic;

namespace Trivias.Menu.Logic
{
    /// <summary>Una fila de la tabla de récords, ya en texto.</summary>
    public readonly struct RecordRow
    {
        public readonly int Position;
        public readonly string Money;
        public readonly string Time;
        public readonly string Date;
        public readonly string Players;
        public readonly string Survivors;

        public RecordRow(int position, string money, string time, string date, string players, string survivors)
        {
            Position = position;
            Money = money;
            Time = time;
            Date = date;
            Players = players;
            Survivors = survivors;
        }
    }

    /// <summary>
    /// Tabla de récords de una categoría (C# puro, probado en EditMode): puesto (los empates lo comparten), premio, tiempo,
    /// fecha, participantes y supervivientes («2/3»).
    /// </summary>
    public static class RecordsTable
    {
        public const string EMPTY = "Todavía no hay victorias en esta categoría.";

        public static void Rows(IReadOnlyList<RecordEntry> sorted, int groupSize, List<RecordRow> into)
        {
            into.Clear();
            for (var i = 0; i < sorted.Count; i++)
            {
                var entry = sorted[i];
                into.Add(new RecordRow(RankingRules.PositionAt(sorted, i), $"{MoneyFormat.Amount(entry.money)} $",
                    RecordText.Duration(entry.durationMs), RecordText.ShortDate(entry.date), entry.players,
                    $"{CountNames(entry.survivors)}/{groupSize}"));
            }
        }

        /// <summary>Nombres en una lista «Ana, Luis, Eva».</summary>
        public static int CountNames(string names)
        {
            if (string.IsNullOrWhiteSpace(names))
                return 0;
            var count = 1;
            foreach (var c in names)
            {
                if (c == ',')
                    count++;
            }
            return count;
        }
    }
}
