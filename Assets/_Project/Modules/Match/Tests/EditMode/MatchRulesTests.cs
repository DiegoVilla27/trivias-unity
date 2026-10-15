using NUnit.Framework;
using Trivias.Match.Logic;

namespace Trivias.Match.Tests
{
    public sealed class MatchRulesTests
    {
        [Test]
        public void NoMoney_IsDefeat() =>
            Assert.That(MatchRules.AfterReveal(0, 2, 3, 15), Is.EqualTo(MatchPhase.Defeat));

        [Test]
        public void MoneyLeft_ContinuesToNextQuestion() =>
            Assert.That(MatchRules.AfterReveal(5_000, 2, 3, 15), Is.EqualTo(MatchPhase.Betting));

        [Test]
        public void LastQuestionWithMoney_IsVictory() =>
            Assert.That(MatchRules.AfterReveal(5_000, 2, 15, 15), Is.EqualTo(MatchPhase.Victory));

        [Test]
        public void LosingTheLastQuestion_IsDefeat() =>
            Assert.That(MatchRules.AfterReveal(0, 2, 15, 15), Is.EqualTo(MatchPhase.Defeat));

        [Test]
        public void NoActivePlayers_IsDefeatEvenWithMoney() =>
            Assert.That(MatchRules.AfterReveal(80_000, 0, 4, 15), Is.EqualTo(MatchPhase.Defeat));

        [Test]
        public void DefeatOnQuestion15_Counts14Passed() =>
            Assert.That(MatchRules.QuestionsPassed(MatchPhase.Defeat, 15), Is.EqualTo(14));

        [Test]
        public void Victory_CountsAllQuestions() =>
            Assert.That(MatchRules.QuestionsPassed(MatchPhase.Victory, 15), Is.EqualTo(15));

        private static readonly int[] Tiers = { 3, 6, 11, 15 };
        private static readonly int[] Rewards = { 50, 100, 150, 250 };

        [TestCase(0, false, 0)]
        [TestCase(2, false, 0)]
        [TestCase(3, false, 50)]
        [TestCase(5, false, 50)]
        [TestCase(6, false, 100)]
        [TestCase(10, false, 100)]
        [TestCase(11, false, 150)]
        [TestCase(14, false, 150)]
        [TestCase(15, true, 250)]
        public void Tokens_FollowGddTiers(int passed, bool victory, int expected) =>
            Assert.That(MatchRules.TokensFor(passed, victory, Tiers, Rewards, 15), Is.EqualTo(expected));

        [Test]
        public void LosingOnQuestion15_Gives150Tokens() =>
            Assert.That(MatchRules.TokensFor(MatchRules.QuestionsPassed(MatchPhase.Defeat, 15), false, Tiers, Rewards, 15),
                Is.EqualTo(150));

        [TestCase(0f, "00:00")]
        [TestCase(65.9f, "01:05")]
        [TestCase(754f, "12:34")]
        public void Duration_IsMinutesAndSeconds(float seconds, string expected) =>
            Assert.That(MatchResult.FormatDuration(seconds), Is.EqualTo(expected));

        [TestCase(MatchPhase.Lobby, true)]
        [TestCase(MatchPhase.Betting, true)]
        [TestCase(MatchPhase.Locked, false)]
        [TestCase(MatchPhase.Reveal, false)]
        public void Bets_AreOnlyAcceptedBeforeLocking(MatchPhase phase, bool expected) =>
            Assert.That(MatchRules.AcceptsBets(phase), Is.EqualTo(expected));
    }
}
