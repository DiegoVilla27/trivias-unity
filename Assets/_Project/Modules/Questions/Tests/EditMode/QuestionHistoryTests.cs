using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using UnityEngine;

namespace Trivias.Questions.Tests
{
    /// <summary>Historial de preguntas vistas y cómo cambia la elección de las preguntas de la partida.</summary>
    public sealed class QuestionHistoryTests
    {
        private string _folder;

        [SetUp]
        public void NewFolder() => _folder = Path.Combine(Path.GetTempPath(), "trivias-history-tests", Path.GetRandomFileName());

        [TearDown]
        public void DeleteFiles()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        // ---- Planificación con historial ----

        [Test]
        public void Plan_PrefersQuestionsNobodyHasSeen()
        {
            var difficulties = Enumerable.Repeat(1, 10).Concat(Enumerable.Repeat(2, 10)).Concat(Enumerable.Repeat(3, 10)).ToArray();
            var unseen = new HashSet<int> { 0, 1, 2, 3, 4, 10, 11, 12, 13, 14, 20, 21, 22, 23, 24 };
            var penalties = Enumerable.Range(0, 30).Select(i => unseen.Contains(i) ? 0 : 1).ToArray();

            for (var seed = 0; seed < 20; seed++)
            {
                var plan = QuestionPlanner.Plan(difficulties, null, penalties, 15, new System.Random(seed));
                Assert.That(plan, Is.SubsetOf(unseen), $"Semilla {seed}: eligió una ya vista habiendo nuevas.");
            }
        }

        [Test]
        public void Plan_WhenEverythingWasSeen_TakesTheOldestFirst()
        {
            var difficulties = Enumerable.Repeat(1, 6).ToArray();
            var penalties = new[] { 6, 5, 4, 3, 2, 1 }; // El 5 es el que se vio hace más tiempo.
            var plan = QuestionPlanner.Plan(difficulties, null, penalties, 3, new System.Random(3));
            Assert.That(plan, Is.EqualTo(new[] { 5, 4, 3 }));
        }

        [Test]
        public void Plan_AvoidsTheSameCategoryTwiceInARow()
        {
            var difficulties = Enumerable.Repeat(1, 20).ToArray();
            var categories = Enumerable.Range(0, 20).Select(i => i < 10 ? 0 : 1).ToArray();
            for (var seed = 0; seed < 20; seed++)
            {
                var plan = QuestionPlanner.Plan(difficulties, categories, null, 6, new System.Random(seed));
                for (var n = 1; n < plan.Length; n++)
                    Assert.That(categories[plan[n]], Is.Not.EqualTo(categories[plan[n - 1]]), $"Semilla {seed}, pregunta {n + 1}.");
            }
        }

        [Test]
        public void Plan_NeverRepeatsASeenQuestionJustForVariety()
        {
            var difficulties = new[] { 1, 1, 1 };
            var categories = new[] { 0, 0, 1 };
            var penalties = new[] { 0, 0, 5 };
            for (var seed = 0; seed < 20; seed++)
                Assert.That(QuestionPlanner.Plan(difficulties, categories, penalties, 2, new System.Random(seed)), Has.No.Member(2));
        }

        [Test]
        public void BlockOf_SplitsFifteenQuestionsInThreeBlocksOfFive()
        {
            Assert.That(QuestionPlanner.BlockOf(0, 15), Is.Zero);
            Assert.That(QuestionPlanner.BlockOf(4, 15), Is.Zero);
            Assert.That(QuestionPlanner.BlockOf(5, 15), Is.EqualTo(1));
            Assert.That(QuestionPlanner.BlockOf(9, 15), Is.EqualTo(1));
            Assert.That(QuestionPlanner.BlockOf(10, 15), Is.EqualTo(2));
            Assert.That(QuestionPlanner.BlockOf(14, 15), Is.EqualTo(2));
        }

        // ---- Reglas del historial ----

        [Test]
        public void MarkSeen_MovesTheQuestionToTheEnd_AndKeepsALimit()
        {
            var seen = new List<string>();
            Assert.That(QuestionHistoryRules.MarkSeen(seen, "a"), Is.True);
            QuestionHistoryRules.MarkSeen(seen, "b");
            Assert.That(QuestionHistoryRules.MarkSeen(seen, "b"), Is.False, "La misma pregunta seguida no cambia nada.");
            QuestionHistoryRules.MarkSeen(seen, "a");
            Assert.That(seen, Is.EqualTo(new[] { "b", "a" }), "Vista de nuevo: pasa a ser la última.");
            Assert.That(QuestionHistoryRules.MarkSeen(seen, null), Is.False);

            for (var i = 0; i < 10; i++)
                QuestionHistoryRules.MarkSeen(seen, "q" + i, limit: 5);
            Assert.That(seen, Has.Count.EqualTo(5));
            Assert.That(seen.Last(), Is.EqualTo("q9"));
        }

        [Test]
        public void Penalties_AddUpTheGroup_AndIgnoreQuestionsOutsideTheBank()
        {
            var ana = new List<ushort> { 3, 1 };   // Vio la 3 y después la 1.
            var luis = new List<ushort> { 1, 9 };  // La 9 ya no existe en el banco.
            var penalties = QuestionHistoryRules.Penalties(5, new List<IReadOnlyList<ushort>> { ana, luis });
            Assert.That(penalties, Is.EqualTo(new[] { 0, 2 + 1, 0, 1, 0 }));
            Assert.That(QuestionHistoryRules.Penalties(3, null), Is.EqualTo(new[] { 0, 0, 0 }));
        }

        // ---- Archivo del historial ----

        [Test]
        public void History_IsSaved_AndMapsToTheCurrentBank()
        {
            var path = Path.Combine(_folder, "seen.json");
            var history = ScriptableObject.CreateInstance<QuestionHistory>();
            var reloaded = ScriptableObject.CreateInstance<QuestionHistory>();
            var bank = ScriptableObject.CreateInstance<QuestionBank>();
            try
            {
                history.UseStoragePath(path);
                history.MarkSeen("geo-001");
                history.MarkSeen("retirada-001");
                history.MarkSeen("cie-001");

                reloaded.UseStoragePath(path);
                Assert.That(reloaded.Seen, Is.EqualTo(new[] { "geo-001", "retirada-001", "cie-001" }));

                bank.EditorSet("es", new List<QuestionData> { Question("cie-001"), Question("nat-001"), Question("geo-001") });
                var indices = new List<ushort>();
                reloaded.CopyIndices(bank, indices);
                Assert.That(indices, Is.EqualTo(new ushort[] { 2, 0 }), "En orden de visto; la retirada se omite.");
            }
            finally
            {
                Object.DestroyImmediate(history);
                Object.DestroyImmediate(reloaded);
                Object.DestroyImmediate(bank);
            }
        }

        [Test]
        public void History_DamagedFile_StartsEmptyWithoutCrashing()
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, "broken.json");
            File.WriteAllText(path, "{ esto no es json");
            var history = ScriptableObject.CreateInstance<QuestionHistory>();
            try
            {
                UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No se pudo leer el historial"));
                history.UseStoragePath(path);
                Assert.That(history.Seen, Is.Empty);
                history.MarkSeen("geo-001");
                Assert.That(history.Seen, Has.Count.EqualTo(1), "Y vuelve a guardar con normalidad.");
            }
            finally
            {
                Object.DestroyImmediate(history);
            }
        }

        private static QuestionData Question(string id) =>
            new(id, "Geografía", 1, "¿" + id + "?", new[] { "a", "b", "c", "d" }, 0, "Porque sí.", "https://es.wikipedia.org");
    }
}
