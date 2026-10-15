using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using UnityEngine;

namespace Trivias.Ranking.Tests
{
    /// <summary>Récords locales (GDD 12): quién entra, orden, empates, categorías, tabla limitada y guardado.</summary>
    public sealed class RankingRulesTests
    {
        private static int _nextId;

        private static RecordEntry Win(int groupSize, long money, long durationMs, string id = null) => new()
        {
            matchId = id ?? $"m{++_nextId}",
            groupSize = groupSize,
            money = money,
            durationMs = durationMs,
            players = "Jugador 1, Jugador 2",
            survivors = "Jugador 1",
            date = "2026-10-15",
        };

        [Test]
        public void OnlyFullVictories_OfTwoToSixPlayers_AreEligible()
        {
            Assert.That(RankingRules.Eligibility(true, true, 2), Is.EqualTo(RecordEligibility.Eligible));
            Assert.That(RankingRules.Eligibility(true, true, 6), Is.EqualTo(RecordEligibility.Eligible));
            Assert.That(RankingRules.Eligibility(false, true, 4), Is.EqualTo(RecordEligibility.Defeat));
            Assert.That(RankingRules.Eligibility(true, false, 4), Is.EqualTo(RecordEligibility.TestRun));
            Assert.That(RankingRules.Eligibility(true, true, 1), Is.EqualTo(RecordEligibility.GroupSize), "Jugar solo (editor).");
            Assert.That(RankingRules.Eligibility(true, true, 7), Is.EqualTo(RecordEligibility.GroupSize));
        }

        [Test]
        public void MoreMoney_GoesFirst_AndTimeOnlyBreaksTies()
        {
            var book = new RecordBook();
            RankingRules.Submit(book, Win(3, 500_000, 900_000), RecordEligibility.Eligible);
            RankingRules.Submit(book, Win(3, 800_000, 1_500_000), RecordEligibility.Eligible);
            RankingRules.Submit(book, Win(3, 500_000, 600_000), RecordEligibility.Eligible);

            var sorted = new List<RecordEntry>();
            RankingRules.Category(book, 3, sorted);
            Assert.That(sorted[0].money, Is.EqualTo(800_000), "Más dinero gana aunque tarde más.");
            Assert.That(sorted[1].durationMs, Is.EqualTo(600_000), "A igual dinero, el más rápido.");
            Assert.That(sorted[2].durationMs, Is.EqualTo(900_000));
        }

        [Test]
        public void ExactTies_ShareThePosition()
        {
            var book = new RecordBook();
            RankingRules.Submit(book, Win(2, 900, 1000), RecordEligibility.Eligible);
            var tied = RankingRules.Submit(book, Win(2, 900, 1000), RecordEligibility.Eligible);
            var third = RankingRules.Submit(book, Win(2, 900, 1001), RecordEligibility.Eligible);

            Assert.That(tied.Kind, Is.EqualTo(RecordResultKind.NewRecord), "Empatar el primero también es récord.");
            Assert.That(tied.Position, Is.EqualTo(1));
            Assert.That(third.Position, Is.EqualTo(3), "Tras dos primeros va el tercero (1, 1, 3).");
            Assert.That(third.Kind, Is.EqualTo(RecordResultKind.Ranked));

            var sorted = new List<RecordEntry>();
            RankingRules.Category(book, 2, sorted);
            Assert.That(RankingRules.PositionAt(sorted, 0), Is.EqualTo(1));
            Assert.That(RankingRules.PositionAt(sorted, 1), Is.EqualTo(1));
            Assert.That(RankingRules.PositionAt(sorted, 2), Is.EqualTo(3));
        }

        [Test]
        public void EachGroupSize_HasItsOwnTable()
        {
            var book = new RecordBook();
            RankingRules.Submit(book, Win(2, 100, 1000), RecordEligibility.Eligible);
            var otherCategory = RankingRules.Submit(book, Win(5, 1, 1000), RecordEligibility.Eligible);

            Assert.That(otherCategory.Kind, Is.EqualTo(RecordResultKind.NewRecord), "Con 5 jugadores compite solo con grupos de 5.");
            var five = new List<RecordEntry>();
            RankingRules.Category(book, 5, five);
            Assert.That(five, Has.Count.EqualTo(1));
        }

        [Test]
        public void TheTable_KeepsOnlyTheBest()
        {
            var book = new RecordBook();
            for (var i = 0; i < RankingRules.TABLE_SIZE; i++)
                RankingRules.Submit(book, Win(4, 1000 + i, 1000), RecordEligibility.Eligible);

            var worse = RankingRules.Submit(book, Win(4, 10, 1000), RecordEligibility.Eligible);
            Assert.That(worse.Kind, Is.EqualTo(RecordResultKind.OutOfTable));
            Assert.That(worse.Position, Is.Zero);

            var better = RankingRules.Submit(book, Win(4, 5000, 1000), RecordEligibility.Eligible);
            Assert.That(better.Kind, Is.EqualTo(RecordResultKind.NewRecord));
            Assert.That(book.entries, Has.Count.EqualTo(RankingRules.TABLE_SIZE), "El peor sale de la tabla.");
            Assert.That(book.entries.Exists(e => e.money == 1000), Is.False);
        }

        [Test]
        public void TiesAtTheLastPlace_StayInTheTable()
        {
            var book = new RecordBook();
            for (var i = 0; i < 3; i++)
                RankingRules.Submit(book, Win(2, 100, 100), RecordEligibility.Eligible, tableSize: 2);
            Assert.That(book.entries, Has.Count.EqualTo(3), "Los tres comparten el primer puesto.");
        }

        [Test]
        public void SubmittingTheSameMatchTwice_DoesNotDuplicateIt()
        {
            var book = new RecordBook();
            RankingRules.Submit(book, Win(2, 100, 100, "x"), RecordEligibility.Eligible);
            var again = RankingRules.Submit(book, Win(2, 100, 100, "x"), RecordEligibility.Eligible);
            Assert.That(book.entries, Has.Count.EqualTo(1));
            Assert.That(again.Position, Is.EqualTo(1));
        }

        [Test]
        public void IneligibleMatches_AreNotStored()
        {
            var book = new RecordBook();
            var outcome = RankingRules.Submit(book, Win(1, 100, 100), RecordEligibility.GroupSize);
            Assert.That(outcome.Kind, Is.EqualTo(RecordResultKind.NotEligible));
            Assert.That(book.entries, Is.Empty);
        }

        [Test]
        public void Entry_ListsEveryParticipant_AndKeepsMilliseconds()
        {
            var entry = RankingRules.Entry("7", 3, 250_000, 754.3216f, "Ana, Luis", "Eva", new DateTime(2026, 10, 15));
            Assert.That(entry.players, Is.EqualTo("Ana, Luis, Eva"));
            Assert.That(entry.survivors, Is.EqualTo("Ana, Luis"));
            Assert.That(entry.durationMs, Is.EqualTo(754_322));
            Assert.That(entry.date, Is.EqualTo("2026-10-15"));
            Assert.That(RankingRules.Entry("8", 2, 1, 1f, "Ana", string.Empty, DateTime.Today).players, Is.EqualTo("Ana"));
        }

        [Test]
        public void Texts_ExplainTheOutcome()
        {
            Assert.That(RecordText.Duration(754_999), Is.EqualTo("12:34"));
            Assert.That(RecordText.Outcome(new RecordOutcome(RecordResultKind.NewRecord, RecordEligibility.Eligible, 1, 4)),
                Does.Contain("NUEVO RÉCORD").And.Contain("4 jugadores"));
            Assert.That(RecordText.Outcome(new RecordOutcome(RecordResultKind.Ranked, RecordEligibility.Eligible, 3, 2)),
                Does.Contain("Puesto 3"));
            Assert.That(RecordText.Outcome(new RecordOutcome(RecordResultKind.OutOfTable, RecordEligibility.Eligible, 0, 2)),
                Does.Contain("No entra"));
            Assert.That(RecordText.Outcome(RecordOutcome.NotEligible(RecordEligibility.TestRun, 1)), Does.Contain("prueba"));
            Assert.That(RecordText.Outcome(RecordOutcome.NotEligible(RecordEligibility.GroupSize, 1)), Does.Contain("2 a 6"));
            Assert.That(RecordText.Outcome(RecordOutcome.NotEligible(RecordEligibility.Defeat, 3)), Is.Empty,
                "Una derrota no habla de récords (GDD UI).");
        }

        [Test]
        public void LocalRecords_AreSaved_AndSurviveARestart()
        {
            var folder = Path.Combine(Path.GetTempPath(), "trivias-ranking-tests", Path.GetRandomFileName());
            var path = Path.Combine(folder, "records.json");
            var records = ScriptableObject.CreateInstance<LocalRecords>();
            try
            {
                records.UseStoragePath(path);
                var changes = 0;
                records.Changed += () => changes++;

                var first = records.Submit(Win(3, 900, 100, "a"), RecordEligibility.Eligible);
                var repeated = records.Submit(Win(3, 900, 100, "a"), RecordEligibility.Eligible);
                Assert.That(first.Kind, Is.EqualTo(RecordResultKind.NewRecord));
                Assert.That(repeated.Kind, Is.EqualTo(first.Kind), "Los resultados se redibujan: misma respuesta.");
                Assert.That(changes, Is.EqualTo(1), "Se guarda una sola vez.");
                records.Submit(Win(3, 1, 1, "b"), RecordEligibility.TestRun);
                Assert.That(changes, Is.EqualTo(1), "Lo no válido ni se guarda ni avisa.");

                var reopened = ScriptableObject.CreateInstance<LocalRecords>();
                reopened.UseStoragePath(path);
                var list = new List<RecordEntry>();
                reopened.Category(3, list);
                Assert.That(list, Has.Count.EqualTo(1));
                Assert.That(list[0].money, Is.EqualTo(900));
                UnityEngine.Object.DestroyImmediate(reopened);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(records);
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
        }

        [Test]
        public void ADamagedFile_StartsWithoutRecords()
        {
            var folder = Path.Combine(Path.GetTempPath(), "trivias-ranking-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "records.json");
            File.WriteAllText(path, "{ esto no es json");
            try
            {
                UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Récords"));
                var book = RecordStore.Load(path);
                Assert.That(book.entries, Is.Empty);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
