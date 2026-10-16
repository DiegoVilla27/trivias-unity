using NUnit.Framework;
using Trivias.Feedback.Logic;
using Trivias.Match.Logic;

namespace Trivias.Feedback.Tests
{
    /// <summary>Plató vivo: reacción del público, bombillas y focos según el momento.</summary>
    public sealed class StageShowRulesTests
    {
        private static MatchSnapshot At(MatchPhase phase, long kept = 0, bool expelled = false) =>
            new(phase, 3, 0, false, kept, 1, expelled);

        [Test]
        public void Crowd_ReactsToTheMoment()
        {
            Assert.That(StageShowRules.Reaction(MatchSnapshot.Lobby, At(MatchPhase.Betting)), Is.EqualTo(CrowdReaction.Applause), "Empieza el concurso.");
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Locked), At(MatchPhase.Reveal, 5000)), Is.EqualTo(CrowdReaction.Applause));
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Locked), At(MatchPhase.Reveal, 5000, true)), Is.EqualTo(CrowdReaction.Aww), "Expulsión.");
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Locked), At(MatchPhase.Reveal)), Is.EqualTo(CrowdReaction.Aww), "Se pierde todo.");
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Reveal), At(MatchPhase.Victory)), Is.EqualTo(CrowdReaction.Cheer));
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Reveal), At(MatchPhase.Defeat)), Is.EqualTo(CrowdReaction.Aww));
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Reveal), At(MatchPhase.Betting)), Is.EqualTo(CrowdReaction.None), "Una pregunta más: silencio.");
            Assert.That(StageShowRules.Reaction(At(MatchPhase.Reveal), At(MatchPhase.Reveal)), Is.EqualTo(CrowdReaction.None), "Solo al cambiar.");
        }

        [Test]
        public void Bulbs_BlinkOnCountdown_AndLightTheCorrectBooth()
        {
            Assert.That(StageShowRules.BulbLevel(StageMood.Countdown, 0, -1, 0, 0.01f), Is.GreaterThan(1f));
            Assert.That(StageShowRules.BulbLevel(StageMood.Countdown, 0, -1, 0, 0.2f), Is.LessThan(0.2f), "Parpadeo.");
            Assert.That(StageShowRules.BulbLevel(StageMood.Correct, 2, 2, 0, 1f), Is.GreaterThan(1f), "La correcta brilla.");
            Assert.That(StageShowRules.BulbLevel(StageMood.Correct, 1, 2, 0, 1f), Is.LessThan(0.1f), "Las falsas se apagan.");
            Assert.That(StageShowRules.BulbLevel(StageMood.Wrong, 2, 2, 0, 1f), Is.GreaterThan(1f), "Aunque se pierda, se ve cuál era.");
            Assert.That(StageShowRules.BulbLevel(StageMood.Defeat, 0, -1, 0, 1f), Is.LessThan(0.3f), "Derrota: casi apagadas.");
        }

        [Test]
        public void Bulbs_ChaseAlongTheRow()
        {
            var first = StageShowRules.BulbLevel(StageMood.Easy, 0, -1, 0, 1f);
            var later = StageShowRules.BulbLevel(StageMood.Easy, 0, -1, 3, 1f);
            Assert.That(first, Is.Not.EqualTo(later).Within(0.01f), "Cada bombilla de la fila va a su ritmo: una onda.");
            for (var order = 0; order < 10; order++)
                Assert.That(StageShowRules.BulbLevel(StageMood.Easy, StageShowRules.MARQUEE, -1, order, 2f), Is.InRange(0.6f, 1.05f));
        }

        [Test]
        public void Fixtures_SwayMoreAtTheParty()
        {
            var calm = 0f;
            var party = 0f;
            for (var t = 0f; t < 20f; t += 0.05f)
            {
                calm = System.Math.Max(calm, System.Math.Abs(StageShowRules.SwayDegrees(0, t, false)));
                party = System.Math.Max(party, System.Math.Abs(StageShowRules.SwayDegrees(0, t, true)));
            }
            Assert.That(calm, Is.InRange(5f, 12f));
            Assert.That(party, Is.GreaterThan(20f));
        }
    }
}
