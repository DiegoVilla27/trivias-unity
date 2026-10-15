using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Feedback.Logic;
using Trivias.Match.Logic;

namespace Trivias.Feedback.Tests
{
    public sealed class FeedbackRulesTests
    {
        private readonly List<FeedbackCue> _cues = new();

        [SetUp]
        public void Clear() => _cues.Clear();

        private static MatchSnapshot At(MatchPhase phase, int question = 1, int seconds = 0, bool timed = false, long kept = 0,
            int multiplier = 1, bool expelled = false) => new(phase, question, seconds, timed, kept, multiplier, expelled);

        private List<FeedbackCue> Between(MatchSnapshot previous, MatchSnapshot current)
        {
            FeedbackRules.MatchCues(previous, current, _cues);
            return _cues;
        }

        [Test]
        public void NothingChanges_NothingSounds() =>
            Assert.That(Between(At(MatchPhase.Betting, seconds: 50, timed: true), At(MatchPhase.Betting, seconds: 50, timed: true)), Is.Empty);

        [Test]
        public void NewQuestion_Chimes()
        {
            Assert.That(Between(MatchSnapshot.Lobby, At(MatchPhase.Betting, 1)), Is.EqualTo(new[] { FeedbackCue.QuestionShown }));
            _cues.Clear();
            Assert.That(Between(At(MatchPhase.Reveal, 1), At(MatchPhase.Betting, 2)), Is.EqualTo(new[] { FeedbackCue.QuestionShown }));
        }

        [Test]
        public void Verify_LocksAndStartsCountdown() =>
            Assert.That(Between(At(MatchPhase.Betting), At(MatchPhase.Locked, seconds: 3)),
                Is.EqualTo(new[] { FeedbackCue.Lock, FeedbackCue.CountdownBeep }));

        [Test]
        public void Countdown_BeepsEverySecond_ButNotAtZero()
        {
            Assert.That(Between(At(MatchPhase.Locked, seconds: 3), At(MatchPhase.Locked, seconds: 2)), Is.EqualTo(new[] { FeedbackCue.CountdownBeep }));
            _cues.Clear();
            Assert.That(Between(At(MatchPhase.Locked, seconds: 1), At(MatchPhase.Locked, seconds: 0)), Is.Empty);
        }

        [Test]
        public void TimedBetting_TicksOnlyInTheLastTenSeconds()
        {
            Assert.That(Between(At(MatchPhase.Betting, seconds: 12, timed: true), At(MatchPhase.Betting, seconds: 11, timed: true)), Is.Empty);
            Assert.That(Between(At(MatchPhase.Betting, seconds: 11, timed: true), At(MatchPhase.Betting, seconds: 10, timed: true)),
                Is.EqualTo(new[] { FeedbackCue.ClockTick }));
        }

        [Test]
        public void UntimedBetting_NeverTicks() =>
            Assert.That(Between(At(MatchPhase.Betting, seconds: 5), At(MatchPhase.Betting, seconds: 4)), Is.Empty);

        [Test]
        public void Reveal_WithMoneyKept_IsSuccess_AndBonusWhenMultiplied() =>
            Assert.That(Between(At(MatchPhase.Locked), At(MatchPhase.Reveal, kept: 50_000, multiplier: 3)),
                Is.EqualTo(new[] { FeedbackCue.Trapdoor, FeedbackCue.Success, FeedbackCue.Bonus }));

        [Test]
        public void Reveal_LosingEverything_IsFail_AndExpelledWhenSomeoneFell() =>
            Assert.That(Between(At(MatchPhase.Locked), At(MatchPhase.Reveal, kept: 0, multiplier: 2, expelled: true)),
                Is.EqualTo(new[] { FeedbackCue.Trapdoor, FeedbackCue.Fail, FeedbackCue.Expelled }));

        [Test]
        public void EndOfMatch_PlaysVictoryOrDefeatOnce()
        {
            Assert.That(Between(At(MatchPhase.Reveal), At(MatchPhase.Victory)), Is.EqualTo(new[] { FeedbackCue.Victory }));
            _cues.Clear();
            Assert.That(Between(At(MatchPhase.Victory), At(MatchPhase.Victory)), Is.Empty);
            Assert.That(Between(At(MatchPhase.Reveal), At(MatchPhase.Defeat)), Is.EqualTo(new[] { FeedbackCue.Defeat }));
        }

        [Test]
        public void CorrectAnswer_CelebratesWithColouredConfetti() =>
            Assert.That(FeedbackRules.CelebrationFor(At(MatchPhase.Locked), At(MatchPhase.Reveal, kept: 5_000)),
                Is.EqualTo(Celebration.CorrectAnswer));

        [Test]
        public void WrongAnswer_HasNoConfetti() =>
            Assert.That(FeedbackRules.CelebrationFor(At(MatchPhase.Locked), At(MatchPhase.Reveal, kept: 0)),
                Is.EqualTo(Celebration.None));

        [Test]
        public void Victory_CelebratesWithGoldenRain_Once()
        {
            Assert.That(FeedbackRules.CelebrationFor(At(MatchPhase.Reveal, 15, kept: 1), At(MatchPhase.Victory, 15, kept: 1)),
                Is.EqualTo(Celebration.Victory), "Tras la pregunta 15 no se repiten los chorros: llega la lluvia dorada.");
            Assert.That(FeedbackRules.CelebrationFor(At(MatchPhase.Victory), At(MatchPhase.Victory)), Is.EqualTo(Celebration.None));
        }

        [Test]
        public void SamePhase_NeverCelebratesTwice() =>
            Assert.That(FeedbackRules.CelebrationFor(At(MatchPhase.Reveal, kept: 9), At(MatchPhase.Reveal, kept: 9)),
                Is.EqualTo(Celebration.None));

        [Test]
        public void Money_PickAndDrop_OnlyWhileUnlocked()
        {
            Assert.That(FeedbackRules.TryMoneyCue(0, 3, false, out var cue), Is.True);
            Assert.That(cue, Is.EqualTo(FeedbackCue.MoneyPick));
            Assert.That(FeedbackRules.TryMoneyCue(3, 1, false, out cue), Is.True);
            Assert.That(cue, Is.EqualTo(FeedbackCue.MoneyDrop));
            Assert.That(FeedbackRules.TryMoneyCue(3, 0, true, out _), Is.False, "Al resolver la ronda no suena «dejar dinero».");
            Assert.That(FeedbackRules.TryMoneyCue(2, 2, false, out _), Is.False);
        }

        [Test]
        public void NewBet_OnlyCountsWhileBetting()
        {
            Assert.That(FeedbackRules.IsNewBet(0, 1, MatchPhase.Betting), Is.True);
            Assert.That(FeedbackRules.IsNewBet(1, 0, MatchPhase.Betting), Is.False, "Cancelar no suena como apostarse.");
            Assert.That(FeedbackRules.IsNewBet(0, 1, MatchPhase.Lobby), Is.False);
        }

        [Test]
        public void MusicBlock_FollowsDifficultyThirds()
        {
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Betting, 1, 15), Is.EqualTo(0));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Reveal, 5, 15), Is.EqualTo(0));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Betting, 6, 15), Is.EqualTo(1));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Locked, 10, 15), Is.EqualTo(1));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Betting, 11, 15), Is.EqualTo(2));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Betting, 15, 15), Is.EqualTo(2));
        }

        [Test]
        public void MusicBlock_SilentOutsideTheMatch()
        {
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Lobby, 0, 15), Is.EqualTo(FeedbackRules.NO_MUSIC));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Victory, 15, 15), Is.EqualTo(FeedbackRules.NO_MUSIC));
            Assert.That(FeedbackRules.MusicBlock(MatchPhase.Defeat, 4, 15), Is.EqualTo(FeedbackRules.NO_MUSIC));
        }
    }
}
