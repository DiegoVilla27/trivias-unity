using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Presenter.Logic;

namespace Trivias.Presenter.Tests
{
    public sealed class PresenterLogicTests
    {
        private static readonly string[] AllKeys =
        {
            "welcome", "long_discussion", "self_bet", "countdown", "success", "loss", "victory", "defeat", "time_running_out",
            "player_left", "medium_block", "hard_block", "last_question", "math_question", "eliminated", "multiplied", "perfect",
        };

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
        public void Selector_SaysEveryPhraseBeforeRepeatingAny()
        {
            var selector = new PhraseSelector(ByEvent(0, 1, 2, 3, 4, 5), new System.Random(9));
            for (var round = 0; round < 5; round++)
            {
                var said = new HashSet<int>();
                for (var i = 0; i < 6; i++)
                    said.Add(selector.Next(PresenterEvent.Welcome));
                Assert.That(said, Has.Count.EqualTo(6), $"Vuelta {round + 1}: no repite hasta decirlas todas.");
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
        public void QuestionStart_CommentsBlocksTheLastAndMath_ButNotEveryQuestion()
        {
            Assert.That(Start(6, false), Is.EqualTo(PresenterEvent.MediumBlock));
            Assert.That(Start(11, true), Is.EqualTo(PresenterEvent.HardBlock), "El cambio de bloque va antes que las matemáticas.");
            Assert.That(Start(15, true), Is.EqualTo(PresenterEvent.LastQuestion));
            Assert.That(Start(7, true), Is.EqualTo(PresenterEvent.MathQuestion));
            Assert.That(PresenterMoments.TryQuestionStart(7, 15, false, out _), Is.False, "Una pregunta normal empieza en silencio.");
            Assert.That(PresenterMoments.TryQuestionStart(1, 15, true, out _), Is.False, "La primera la presenta la bienvenida.");
        }

        [Test]
        public void Reveal_PicksExpulsionLossMultiplierPerfectOrSuccess()
        {
            Assert.That(PresenterMoments.Reveal(true, 0, 100, 50), Is.EqualTo(PresenterEvent.Eliminated));
            Assert.That(PresenterMoments.Reveal(false, 80, 20, 0), Is.EqualTo(PresenterEvent.Loss));
            Assert.That(PresenterMoments.Reveal(false, 10, 90, 90), Is.EqualTo(PresenterEvent.Multiplied));
            Assert.That(PresenterMoments.Reveal(false, 0, 100, 0), Is.EqualTo(PresenterEvent.Perfect));
            Assert.That(PresenterMoments.Reveal(false, 10, 90, 0), Is.EqualTo(PresenterEvent.Success));
        }

        [Test]
        public void EveryEvent_HasAContentKey()
        {
            Assert.That(AllKeys, Has.Length.EqualTo(PresenterEvents.COUNT));
            var parsed = new HashSet<PresenterEvent>();
            foreach (var key in AllKeys)
            {
                Assert.That(PresenterEvents.TryParse(key, out var presenterEvent), Is.True, key);
                parsed.Add(presenterEvent);
            }
            Assert.That(parsed, Has.Count.EqualTo(PresenterEvents.COUNT));
        }

        private static PresenterEvent Start(int question, bool isMath)
        {
            Assert.That(PresenterMoments.TryQuestionStart(question, 15, isMath, out var presenterEvent), Is.True, $"Pregunta {question}");
            return presenterEvent;
        }

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
            foreach (var key in AllKeys)
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
