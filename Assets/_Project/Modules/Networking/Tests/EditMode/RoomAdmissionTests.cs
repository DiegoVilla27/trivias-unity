using NUnit.Framework;
using Trivias.Networking.Logic;

namespace Trivias.Networking.Tests
{
    public sealed class RoomAdmissionTests
    {
        [TestCase(0, 6)]
        [TestCase(5, 6)]
        public void RejectionReason_AllowsWhileThereIsRoom(int connected, int max) =>
            Assert.That(RoomAdmission.RejectionReason(connected, max, matchStarted: false), Is.Null);

        [TestCase(6, 6)]
        [TestCase(7, 6)]
        public void RejectionReason_RejectsWhenFull(int connected, int max) =>
            Assert.That(RoomAdmission.RejectionReason(connected, max, matchStarted: false),
                Is.EqualTo(RoomAdmission.ROOM_FULL_REASON));

        [Test]
        public void RejectionReason_RejectsWhenMatchStarted() =>
            Assert.That(RoomAdmission.RejectionReason(2, 6, matchStarted: true),
                Is.EqualTo(RoomAdmission.MATCH_STARTED_REASON));

        [TestCase(1, 2, false)]
        [TestCase(2, 2, true)]
        [TestCase(6, 2, true)]
        [TestCase(1, 1, true)]
        public void CanStartMatch_RespectsMinimum(int connected, int min, bool expected) =>
            Assert.That(RoomAdmission.CanStartMatch(connected, min, 6, matchStarted: false), Is.EqualTo(expected));

        [Test]
        public void CanStartMatch_FalseWhenAlreadyStarted() =>
            Assert.That(RoomAdmission.CanStartMatch(3, 2, 6, matchStarted: true), Is.False);
    }
}
