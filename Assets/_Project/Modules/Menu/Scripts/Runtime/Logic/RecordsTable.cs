using System.Collections.Generic;
using System.Text;
using Trivias.Money.Logic;
using Trivias.Ranking.Logic;

namespace Trivias.Menu.Logic
{
    /// <summary>
    /// Texto de la tabla de récords de una categoría (C# puro, probado en EditMode). Cada victoria ocupa dos líneas:
    /// puesto, premio, tiempo y fecha; debajo, quiénes jugaron y quiénes sobrevivieron.
    /// </summary>
    public static class RecordsTable
    {
        private const string GOLD = "#FFD15C";
        private const string MUTED = "#FFFFFFAA";

        public static string Build(IReadOnlyList<RecordEntry> sorted, int groupSize)
        {
            if (sorted.Count == 0)
                return $"<align=center><color={MUTED}>Aún no tenéis victorias con {RecordText.Category(groupSize)}.\n" +
                       "Superad las 15 preguntas para estrenar la tabla.</color></align>";

            var text = new StringBuilder();
            text.Append($"<color={MUTED}><size=85%>#<pos=6%>PREMIO<pos=42%>TIEMPO<pos=62%>FECHA</size></color>");
            for (var i = 0; i < sorted.Count; i++)
            {
                var entry = sorted[i];
                var position = RankingRules.PositionAt(sorted, i);
                var color = position == 1 ? GOLD : "#FFFFFF";
                text.Append('\n')
                    .Append($"<color={color}><b>{position}.</b><pos=6%><b>{MoneyFormat.Amount(entry.money)}</b>")
                    .Append($"<pos=42%>{RecordText.Duration(entry.durationMs)}<pos=62%>{RecordText.Date(entry.date)}</color>")
                    .Append($"\n<pos=6%><size=80%><color={MUTED}>{entry.players}");
                if (!string.IsNullOrEmpty(entry.survivors))
                    text.Append($" · supervivientes: {entry.survivors}");
                text.Append("</color></size>");
            }
            return text.ToString();
        }
    }
}
