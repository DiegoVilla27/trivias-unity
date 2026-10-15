using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Presenter.Logic;

namespace Trivias.Presenter.Tests
{
    public sealed class PresenterLogicTests
    {
        private static IReadOnlyList<int>[] ByEvent(params int[] welcomePhrases)
        {
            var byEvent = new IReadOnlyList<int>[PresenterEvents.COUNT];
            for (var i = 0; i < byEvent.Length; i++)
                byEvent[i] = new List<int>();
            byEvent[(int)PresenterEvent.Welcome] = new List<int>(welcomePhrases);
            return byEvent;
        }

        [Test]
        public void Selector_DoesNotRepeatTheLastTwo()
        {
            var selector = new PhraseSelector(ByEvent(0, 1, 2), new System.Random(4));
            var previous = new List<int>();
            for (var i = 0; i < 60; i++)
            {
                var phrase = selector.Next(PresenterEvent.Welcome);
                if (previous.Count >= 1) Assert.That(phrase, Is.Not.EqualTo(previous[previous.Count - 1]));
                if (previous.Count >= 2) Assert.That(phrase, Is.Not.EqualTo(previous[previous.Count - 2]));
                previous.Add(phrase);
            }
        }

        [Test]
        public void Selector_WithOnePhrase_RepeatsIt()
        {
            var selector = new PhraseSelector(ByEvent(7), new System.Random(1));
            Assert.That(selector.Next(PresenterEvent.Welcome), Is.EqualTo(7));
            Assert.That(selector.Next(PresenterEvent.Welcome), Is.EqualTo(7));
        }

        [Test]
        public void Selector_WithoutPhrases_ReturnsMinusOne() =>
            Assert.That(new PhraseSelector(ByEvent(), new System.Random(1)).Next(PresenterEvent.Defeat), Is.EqualTo(-1));

        [Test]
        public void Variables_FillAllPlaceholders()
        {
            var variables = new PhraseVariables(45_000, 1_250_000, 3, 7, 2);
            var text = variables.Format("{cantidad} · {premio} · {supervivientes} · {ronda} · x{multiplicador}");
            Assert.That(text, Is.EqualTo("45.000 · 1.250.000 · 3 · 7 · x2"));
        }

        [Test]
        public void Validator_AcceptsValidCatalog()
        {
            var phrases = new List<(string, string, string)>();
            foreach (var key in new[] { "welcome", "long_discussion", "self_bet", "countdown", "success", "loss", "victory", "defeat" })
                phrases.Add(($"{key}-01", key, "Texto con {premio}."));
            Assert.That(PhraseValidator.Validate(phrases), Is.Empty);
        }

        [Test]
        public void Validator_RejectsUnknownVariableAndEvent()
        {
            var errors = PhraseValidator.Validate(new List<(string, string, string)>
            {
                ("a", "welcome", "Hola {nombre}"),
                ("b", "fiesta", "Texto"),
            });
            Assert.That(errors.Exists(e => e.Contains("{nombre}")), Is.True);
            Assert.That(errors.Exists(e => e.Contains("fiesta")), Is.True);
            Assert.That(errors.Exists(e => e.Contains("Falta al menos una frase")), Is.True);
        }
    }
}
