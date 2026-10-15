using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.Questions.Data;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>Contenido (CONTENIDO.md): banco de preguntas y frases del presentador ya importados.</summary>
    public sealed class ContentTests
    {
        private const string BANK_PATH = "Assets/_Project/Modules/Questions/Config/QuestionBank_es.asset";
        private const string CATALOG_PATH = "Assets/_Project/Modules/Presenter/Config/PresenterCatalog_es.asset";

        private static QuestionBank Bank() => AssetDatabase.LoadAssetAtPath<QuestionBank>(BANK_PATH);
        private static PresenterCatalog Catalog() => AssetDatabase.LoadAssetAtPath<PresenterCatalog>(CATALOG_PATH);

        [Test]
        public void Bank_HasThreeHundredQuestions_AHundredPerDifficulty()
        {
            var bank = Bank();
            Assert.That(bank.Count, Is.GreaterThanOrEqualTo(300), "Unas 300 preguntas para que no se repitan pronto.");
            for (var difficulty = 1; difficulty <= 3; difficulty++)
                Assert.That(bank.Questions.Count(q => q.Difficulty == difficulty), Is.GreaterThanOrEqualTo(100),
                    $"Faltan preguntas de dificultad {difficulty}.");
        }

        [Test]
        public void Bank_HasEasyMediumAndHardMath()
        {
            for (var difficulty = 1; difficulty <= 3; difficulty++)
                Assert.That(Bank().Questions.Count(q => q.IsMath && q.Difficulty == difficulty), Is.GreaterThanOrEqualTo(20),
                    $"Faltan operaciones de dificultad {difficulty}.");
        }

        [Test]
        public void Bank_MixesManyCategories()
        {
            var categories = Bank().Questions.GroupBy(q => q.Category).ToDictionary(g => g.Key, g => g.Count());
            Assert.That(categories.Count, Is.GreaterThanOrEqualTo(10), "No solo cultura general de un par de temas.");
            Assert.That(categories.Values.Max(), Is.LessThan(Bank().Count / 4), "Ninguna categoría acapara el banco.");
        }

        [Test]
        public void Bank_TextsOnlyUseCharactersTheFontHas()
        {
            // La fuente del juego trae Latin-1, comillas tipográficas, rayas, puntos suspensivos y €.
            var extra = new HashSet<char> { '\u2013', '\u2014', '\u2018', '\u2019', '\u201C', '\u201D', '\u2026', '\u20AC' };
            var problems = new List<string>();
            foreach (var question in Bank().Questions)
            {
                var all = question.Text + question.Explanation;
                for (var option = 0; option < 4; option++)
                    all += question.Option(option);
                foreach (var c in all)
                {
                    if (c > '\u00FF' && !extra.Contains(c))
                        problems.Add($"{question.Id}: «{c}» (U+{(int)c:X4})");
                }
            }
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void Bank_HasNoRepeatedIdsOrTexts()
        {
            var bank = Bank();
            Assert.That(bank.Questions.Select(q => q.Id).Distinct().Count(), Is.EqualTo(bank.Count));
            Assert.That(bank.Questions.Select(q => q.Text.Trim().ToLowerInvariant()).Distinct().Count(), Is.EqualTo(bank.Count));
        }

        [Test]
        public void Bank_CorrectAnswersAreSpreadAcrossPositions()
        {
            var positions = Bank().Questions.GroupBy(q => q.CorrectOption).ToDictionary(g => g.Key, g => g.Count());
            for (var option = 0; option < 4; option++)
                Assert.That(positions.TryGetValue(option, out var count) && count >= 4, Is.True,
                    $"La respuesta correcta casi nunca está en la posición {option} del contenido original.");
        }

        [Test]
        public void Bank_EveryQuestionHasExplanationAndSource()
        {
            foreach (var question in Bank().Questions)
            {
                Assert.That(question.Explanation, Is.Not.Empty, question.Id);
                Assert.That(question.Source, Does.StartWith("http"), question.Id);
            }
        }

        [Test]
        public void Catalog_HasAHundredPhrases_AndSeveralPerEvent()
        {
            var catalog = Catalog();
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(100), "Unas 100 frases para que no se repita.");
            var byEvent = catalog.BuildIndexByEvent();
            for (var e = 0; e < PresenterEvents.COUNT; e++)
                Assert.That(byEvent[e].Count, Is.GreaterThanOrEqualTo(4), $"Pocas frases para {(PresenterEvent)e}.");
        }

        [Test]
        public void Catalog_PhrasesAreShort()
        {
            var catalog = Catalog();
            for (var i = 0; i < catalog.Count; i++)
                Assert.That(catalog[i].Text.Split(' ').Length, Is.LessThanOrEqualTo(20), $"{catalog[i].Id}: máximo 20 palabras.");
        }

        [Test]
        public void Catalog_EveryPhraseFormatsWithoutLeftoverVariables()
        {
            var catalog = Catalog();
            var variables = new PhraseVariables(45_000, 100_000, 3, 4, 2);
            var leftovers = new List<string>();
            for (var i = 0; i < catalog.Count; i++)
            {
                var text = variables.Format(catalog[i].Text);
                if (text.Contains("{") || text.Contains("}"))
                    leftovers.Add($"{catalog[i].Id}: {text}");
            }
            Assert.That(leftovers, Is.Empty, string.Join("\n", leftovers));
        }
    }
}
