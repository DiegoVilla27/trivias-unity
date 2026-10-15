using NUnit.Framework;
using Trivias.Player.Logic;

namespace Trivias.Player.Tests
{
    public sealed class LookAnglesTests
    {
        [TestCase(0f, 0f)]
        [TestCase(190f, -170f)]
        [TestCase(-190f, 170f)]
        [TestCase(720f, 0f)]
        [TestCase(180f, 180f)]
        public void Wrap_NormalizesToHalfOpenRange(float input, float expected) =>
            Assert.That(LookAngles.Wrap(input), Is.EqualTo(expected).Within(0.001f));

        [TestCase(-50f, -30f)]
        [TestCase(90f, 70f)]
        [TestCase(10f, 10f)]
        [TestCase(350f, -10f)]
        public void ClampPitch_StaysWithinLimits(float input, float expected) =>
            Assert.That(LookAngles.ClampPitch(input, -30f, 70f), Is.EqualTo(expected).Within(0.001f));

        [Test]
        public void MoveYaw_ForwardInputFollowsCameraYaw() =>
            Assert.That(LookAngles.MoveYaw(0f, 1f, 45f), Is.EqualTo(45f).Within(0.001f));

        [Test]
        public void MoveYaw_RightInputTurnsNinetyDegrees() =>
            Assert.That(LookAngles.MoveYaw(1f, 0f, 0f), Is.EqualTo(90f).Within(0.001f));
    }
}
