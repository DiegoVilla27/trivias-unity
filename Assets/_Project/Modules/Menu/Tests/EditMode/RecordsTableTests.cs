using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Menu.Logic;
using Trivias.Money.Logic;
using Trivias.Ranking.Logic;

namespace Trivias.Menu.Tests
{
    /// <summary>Tabla de récords del menú: filas con premio, tiempo, fecha, participantes y supervivientes, y empates.</summary>
    public sealed class RecordsTableTests
    {
        private static RecordEntry Win(long money, long ms, string survivors = "Ana") => new()
        {
            matchId = money + "-" + ms,
            groupSize = 3,
            money = money,
            durationMs = ms,
            players = "Ana, Luis, Eva",
            survivors = survivors,
            date = "2026-10-15",
        };

        private static List<RecordRow> Rows(params RecordEntry[] sorted)
        {
            var rows = new List<RecordRow>();
            RecordsTable.Rows(sorted, 3, rows);
            return rows;
        }

        [Test]
        public void AnEmptyCategory_SaysSo()
        {
            Assert.That(Rows(), Is.Empty);
            Assert.That(RecordsTable.EMPTY, Is.EqualTo("Todavía no hay victorias en esta categoría."));
        }

        [Test]
        public void EachRow_HasPrizeTimeDatePlayersAndSurvivors()
        {
            var row = Rows(Win(6_400_000, 754_000, "Ana, Eva"))[0];
            Assert.That(row.Position, Is.EqualTo(1));
            Assert.That(row.Money, Is.EqualTo(MoneyFormat.Amount(6_400_000) + " $"));
            Assert.That(row.Time, Is.EqualTo("12:34"));
            Assert.That(row.Date, Is.EqualTo("15/10/26"));
            Assert.That(row.Players, Is.EqualTo("Ana, Luis, Eva"));
            Assert.That(row.Survivors, Is.EqualTo("2/3"), "Supervivientes de los que empezaron.");
        }

        [Test]
        public void TiedRows_ShareTheirPosition()
        {
            var rows = Rows(Win(900, 100), Win(900, 100), Win(500, 100));
            Assert.That(rows.ConvertAll(r => r.Position), Is.EqualTo(new[] { 1, 1, 3 }));
        }

        [Test]
        public void Dates_AreShownDayFirst()
        {
            Assert.That(RecordText.Date("2026-01-31"), Is.EqualTo("31/01/2026"));
            Assert.That(RecordText.ShortDate("2026-01-31"), Is.EqualTo("31/01/26"));
            Assert.That(RecordText.Date("raro"), Is.EqualTo("raro"));
            Assert.That(RecordText.ShortDate("raro"), Is.EqualTo("raro"));
            Assert.That(RecordText.Date(null), Is.Empty);
        }

        [Test]
        public void Names_AreCounted()
        {
            Assert.That(RecordsTable.CountNames(""), Is.Zero);
            Assert.That(RecordsTable.CountNames("Ana"), Is.EqualTo(1));
            Assert.That(RecordsTable.CountNames("Ana, Luis, Eva"), Is.EqualTo(3));
        }
    }
}
