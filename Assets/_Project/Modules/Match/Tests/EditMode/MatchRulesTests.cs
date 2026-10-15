using NUnit.Framework;
using Trivias.Match.Logic;

namespace Trivias.Match.Tests
{
    public sealed class MatchRulesTests
    {
        [Test]
        public void NoMoney_IsDefeat() =>
            Assert.That(MatchRules.AfterReveal(0, 3, 15), Is.EqualTo(MatchPhase.Defeat));

        [Test]
        public void MoneyLeft_ContinuesToNextQuestion() =>
            Assert.That(MatchRules.AfterReveal(5_000, 3, 15), Is.EqualTo(MatchPhase.Betting));

        [Test]
        public void LastQuestionWithMoney_IsVictory() =>
            Assert.That(MatchRules.AfterReveal(5_000, 15, 15), Is.EqualTo(MatchPhase.Victory));

        [Test]
        public void LosingTheLastQuestion_IsDefeat() =>
            Assert.That(MatchRules.AfterReveal(0, 15, 15), Is.EqualTo(MatchPhase.Defeat));

        [Test]
        public void DefeatOnQuestion15_Counts14Passed() =>
            Assert.That(MatchRules.QuestionsPassed(MatchPhase.Defeat, 15), Is.EqualTo(14));

        [Test]
        public void Victory_CountsAllQuestions() =>
            Assert.That(MatchRules.QuestionsPassed(MatchPhase.Victory, 15), Is.EqualTo(15));

        [TestCase(MatchPhase.Lobby, true)]
        [TestCase(MatchPhase.Betting, true)]
        [TestCase(MatchPhase.Locked, false)]
        [TestCase(MatchPhase.Reveal, false)]
        public void Bets_AreOnlyAcceptedBeforeLocking(MatchPhase phase, bool expected) =>
            Assert.That(MatchRules.AcceptsBets(phase), Is.EqualTo(expected));
    }
}
