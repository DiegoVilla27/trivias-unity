using NUnit.Framework;
using Trivias.Menu.Logic;
using Trivias.Shared.Rooms;

namespace Trivias.Menu.Tests
{
    public sealed class MenuLogicTests
    {
        [Test]
        public void Pager_StaysWithinThePages()
        {
            Assert.That(Pager.Clamp(-1, 6), Is.EqualTo(0));
            Assert.That(Pager.Clamp(9, 6), Is.EqualTo(5));
            Assert.That(Pager.HasPrevious(0), Is.False);
            Assert.That(Pager.HasNext(5, 6), Is.False);
            Assert.That(Pager.HasNext(4, 6), Is.True);
            Assert.That(Pager.Label(0, 6), Is.EqualTo("1 / 6"));
        }

        [Test]
        public void Invite_ShowsTheVirtualNetworkFirst_AndHowToUseIt()
        {
            var addresses = new[]
            {
                new RoomAddress("10.147.17.5", RoomAddressKind.VirtualNetwork, "ZeroTier"),
                new RoomAddress("192.168.1.20", RoomAddressKind.LocalNetwork, ""),
            };
            var text = InviteText.Build(addresses);
            Assert.That(InviteText.Primary(addresses), Is.EqualTo("10.147.17.5"));
            Assert.That(text.IndexOf("10.147.17.5"), Is.LessThan(text.IndexOf("192.168.1.20")));
            Assert.That(text, Does.Contain("ZeroTier"));
            Assert.That(text, Does.Contain("Unirme a una sala"));
        }

        [Test]
        public void Invite_WithOnlyLocalNetwork_SuggestsAVirtualNetwork()
        {
            var text = InviteText.Build(new[] { new RoomAddress("192.168.1.20", RoomAddressKind.LocalNetwork, "") });
            Assert.That(text, Does.Contain("misma wifi"));
            Assert.That(text, Does.Contain(InviteText.GUIDE));
        }

        [Test]
        public void Invite_WithoutNetwork_SaysOnlyThisComputer()
        {
            var text = InviteText.Build(new[] { new RoomAddress("127.0.0.1", RoomAddressKind.ThisComputer, "") });
            Assert.That(text, Does.Contain("solo este ordenador"));
        }

        [Test]
        public void Invite_Empty_WhenNotHosting()
        {
            Assert.That(InviteText.Build(new RoomAddress[0]), Is.Empty);
            Assert.That(InviteText.Primary(null), Is.Empty);
        }
    }
}
