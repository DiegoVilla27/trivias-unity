using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.Questions.Data;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>Contenido del prototipo (CONTENIDO.md): banco de preguntas y frases del presentador ya importados.</summary>
    public sealed class ContentTests
    {
        private const string BANK_PATH = "Assets/_Project/Modules/Questions/Config/QuestionBank_es.asset";
        private const string CATALOG_PATH = "Assets/_Project/Modules/Presenter/Config/PresenterCatalog_es.asset";

        private static QuestionBank Bank() => AssetDatabase.LoadAssetAtPath<QuestionBank>(BANK_PATH);
        private static PresenterCatalog Catalog() => AssetDatabase.LoadAssetAtPath<PresenterCatalog>(CATALOG_PATH);

        [Test]
        public void Bank_HasThirtyQuestions_TenPerDifficulty()
        {
            var bank = Bank();
            Assert.That(bank.Count, Is.GreaterThanOrEqualTo(30), "El prototipo pide 30 preguntas.");
            for (var difficulty = 1; difficulty <= 3; difficulty++)
                Assert.That(bank.Questions.Count(q => q.Difficulty == difficulty), Is.GreaterThanOrEqualTo(10),
                    $"Faltan preguntas de dificultad {difficulty}.");
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
        public void Catalog_HasAtLeastTwentyPhrasesAndEveryEvent()
        {
            var catalog = Catalog();
            Assert.That(catalog.Count, Is.GreaterThanOrEqualTo(20), "El prototipo pide 20 frases.");
            var byEvent = catalog.BuildIndexByEvent();
            for (var e = 0; e < PresenterEvents.COUNT; e++)
                Assert.That(byEvent[e].Count, Is.GreaterThan(0), $"No hay frases para {(PresenterEvent)e}.");
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
