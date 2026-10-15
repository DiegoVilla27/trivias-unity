using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Menu.Logic;
using Trivias.Money.Logic;
using Trivias.Ranking.Logic;

namespace Trivias.Menu.Tests
{
    /// <summary>Tabla de récords del menú: vacía, filas con premio, tiempo, fecha, jugadores y supervivientes, y empates.</summary>
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

        [Test]
        public void AnEmptyCategory_InvitesToWin()
        {
            var text = RecordsTable.Build(new List<RecordEntry>(), 4);
            Assert.That(text, Does.Contain("Aún no tenéis victorias con 4 jugadores"));
        }

        [Test]
        public void EachRow_ShowsPrizeTimeDatePlayersAndSurvivors()
        {
            var text = RecordsTable.Build(new List<RecordEntry> { Win(6_400_000, 754_000) }, 3);
            Assert.That(text, Does.Contain("PREMIO").And.Contain("TIEMPO").And.Contain("FECHA"));
            Assert.That(text, Does.Contain(MoneyFormat.Amount(6_400_000)));
            Assert.That(text, Does.Contain("12:34"));
            Assert.That(text, Does.Contain("15/10/2026"));
            Assert.That(text, Does.Contain("Ana, Luis, Eva"));
            Assert.That(text, Does.Contain("supervivientes: Ana"));
        }

        [Test]
        public void TiedRows_ShareTheirPosition()
        {
            var sorted = new List<RecordEntry> { Win(900, 100), Win(900, 100), Win(500, 100) };
            var text = RecordsTable.Build(sorted, 3);
            Assert.That(text.Split("<b>1.</b>").Length - 1, Is.EqualTo(2), "Dos primeros puestos.");
            Assert.That(text, Does.Contain("<b>3.</b>"));
            Assert.That(text, Does.Not.Contain("<b>2.</b>"));
        }

        [Test]
        public void Dates_AreShownDayFirst()
        {
            Assert.That(RecordText.Date("2026-01-31"), Is.EqualTo("31/01/2026"));
            Assert.That(RecordText.Date("raro"), Is.EqualTo("raro"));
            Assert.That(RecordText.Date(null), Is.Empty);
        }
    }
}
