using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Trivias.Questions.Logic;

namespace Trivias.Questions.Tests
{
    public sealed class QuestionsLogicTests
    {
        // ---- Orden de opciones ----

        [Test]
        public void OptionOrder_RoundTripsPermutation()
        {
            var order = OptionOrder.FromPermutation(2, 0, 3, 1);
            Assert.That(order.OriginalAt(0), Is.EqualTo(2));
            Assert.That(order.OriginalAt(3), Is.EqualTo(1));
            Assert.That(order.BoothOf(3), Is.EqualTo(2));
            Assert.That(order.IsValidPermutation(), Is.True);
        }

        [Test]
        public void OptionOrder_ShuffleIsAlwaysAValidPermutation()
        {
            var random = new Random(7);
            for (var i = 0; i < 200; i++)
                Assert.That(OptionOrder.Shuffle(random).IsValidPermutation(), Is.True);
        }

        [Test]
        public void OptionOrder_ShuffleMovesTheCorrectAnswerAround()
        {
            var random = new Random(3);
            var booths = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                booths.Add(OptionOrder.Shuffle(random).BoothOf(0));
            Assert.That(booths.Count, Is.EqualTo(4), "La correcta debe poder caer en cualquier cabina.");
        }

        // ---- Planificación ----

        [Test]
        public void Plan_PicksFifteenWithoutRepeating()
        {
            var difficulties = Enumerable.Repeat(1, 6).Concat(Enumerable.Repeat(2, 6)).Concat(Enumerable.Repeat(3, 6)).ToArray();
            var plan = QuestionPlanner.Plan(difficulties, 15, new Random(1));
            Assert.That(plan.Length, Is.EqualTo(15));
            Assert.That(plan.Distinct().Count(), Is.EqualTo(15));
        }

        [Test]
        public void Plan_GoesFromEasyToHard()
        {
            var difficulties = Enumerable.Repeat(1, 6).Concat(Enumerable.Repeat(2, 6)).Concat(Enumerable.Repeat(3, 6)).ToArray();
            var plan = QuestionPlanner.Plan(difficulties, 15, new Random(5));
            Assert.That(plan.Take(5).All(i => difficulties[i] == 1), Is.True, "Bloque 1: fáciles.");
            Assert.That(plan.Skip(5).Take(5).All(i => difficulties[i] == 2), Is.True, "Bloque 2: medias.");
            Assert.That(plan.Skip(10).All(i => difficulties[i] == 3), Is.True, "Bloque 3: difíciles.");
        }

        [Test]
        public void Plan_FillsShortBlocksWithNearbyDifficulty()
        {
            var difficulties = Enumerable.Repeat(1, 10).Concat(Enumerable.Repeat(3, 5)).ToArray(); // sin medias
            var plan = QuestionPlanner.Plan(difficulties, 15, new Random(2));
            Assert.That(plan.Distinct().Count(), Is.EqualTo(15));
        }

        [Test]
        public void Plan_FailsWhenBankIsTooSmall() =>
            Assert.Throws<ArgumentException>(() => QuestionPlanner.Plan(new[] { 1, 2, 3 }, 15, new Random(0)));

        // ---- Validación ----

        private static QuestionValidator.Entry Valid(string id = "q1") =>
            new(id, "¿Pregunta?", 1, new[] { "a", "b", "c", "d" }, new[] { "Uno", "Dos", "Tres", "Cuatro" }, "b", "Porque sí.");

        [Test]
        public void Validator_AcceptsValidQuestion() =>
            Assert.That(QuestionValidator.Validate(new[] { Valid() }), Is.Empty);

        [Test]
        public void Validator_RejectsDuplicateIds() =>
            Assert.That(QuestionValidator.Validate(new[] { Valid("x"), Valid("x") }), Has.Count.EqualTo(1));

        [Test]
        public void Validator_RejectsWrongOptionCount()
        {
            var entry = new QuestionValidator.Entry("q", "¿?", 1, new[] { "a", "b", "c" }, new[] { "1", "2", "3" }, "a", "x");
            Assert.That(QuestionValidator.Validate(new[] { entry }), Is.Not.Empty);
        }

        [Test]
        public void Validator_RejectsUnknownCorrectOption()
        {
            var entry = new QuestionValidator.Entry("q", "¿?", 1, new[] { "a", "b", "c", "d" }, new[] { "1", "2", "3", "4" }, "z", "x");
            Assert.That(QuestionValidator.Validate(new[] { entry }), Is.Not.Empty);
        }

        [Test]
        public void Validator_RejectsRepeatedOptionTexts()
        {
            var entry = new QuestionValidator.Entry("q", "¿?", 1, new[] { "a", "b", "c", "d" }, new[] { "1", "1", "3", "4" }, "a", "x");
            Assert.That(QuestionValidator.Validate(new[] { entry }), Is.Not.Empty);
        }
    }
}
