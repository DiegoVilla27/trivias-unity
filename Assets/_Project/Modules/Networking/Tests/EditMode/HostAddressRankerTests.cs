using System.Linq;
using NUnit.Framework;
using Trivias.Networking.Logic;
using Trivias.Shared.Rooms;

namespace Trivias.Networking.Tests
{
    public sealed class HostAddressRankerTests
    {
        private static LocalInterfaceAddress At(string address, string name = "en0", string description = "") =>
            new(address, name, description);

        [Test]
        public void VirtualNetworks_ComeFirst()
        {
            var ranked = HostAddressRanker.Rank(new[] { At("192.168.1.20"), At("10.147.17.5", "feth1234") });
            Assert.That(ranked.Select(a => a.Address), Is.EqualTo(new[] { "10.147.17.5", "192.168.1.20" }));
            Assert.That(ranked[0].Kind, Is.EqualTo(RoomAddressKind.VirtualNetwork));
            Assert.That(ranked[0].NetworkName, Is.EqualTo("ZeroTier"));
            Assert.That(ranked[1].Kind, Is.EqualTo(RoomAddressKind.LocalNetwork));
        }

        [TestCase("26.11.22.33", "Ethernet 2", "", "Radmin VPN")]
        [TestCase("10.0.0.7", "Ethernet 3", "Radmin VPN", "Radmin VPN")]
        [TestCase("100.101.5.6", "utun4", "", "Tailscale")]
        [TestCase("172.27.1.2", "ZeroTier One [8056c2e21c000001]", "ZeroTier Virtual Port", "ZeroTier")]
        [TestCase("25.1.2.3", "Hamachi", "LogMeIn Hamachi Virtual Ethernet Adapter", "Hamachi")]
        public void RecognisesVirtualNetworks(string address, string name, string description, string expected)
        {
            var ranked = HostAddressRanker.Rank(new[] { At(address, name, description) });
            Assert.That(ranked[0].Kind, Is.EqualTo(RoomAddressKind.VirtualNetwork));
            Assert.That(ranked[0].NetworkName, Is.EqualTo(expected));
        }

        [Test]
        public void InternalAddresses_AreNeverOffered()
        {
            var ranked = HostAddressRanker.Rank(new[] { At("127.0.0.1", "lo0"), At("169.254.10.2"), At("0.0.0.0"), At("192.168.0.4") });
            Assert.That(ranked.Select(a => a.Address), Is.EqualTo(new[] { "192.168.0.4" }));
        }

        [Test]
        public void WithoutAnyNetwork_OffersThisComputer()
        {
            var ranked = HostAddressRanker.Rank(new[] { At("127.0.0.1", "lo0") });
            Assert.That(ranked.Count, Is.EqualTo(1));
            Assert.That(ranked[0].Kind, Is.EqualTo(RoomAddressKind.ThisComputer));
            Assert.That(ranked[0].Address, Is.EqualTo(HostAddressRanker.LOOPBACK));
        }

        [Test]
        public void Duplicates_AndGarbage_AreIgnored()
        {
            var ranked = HostAddressRanker.Rank(new[] { At("192.168.1.2"), At("192.168.1.2"), At("no-ip"), At(null), At("300.1.1.1") });
            Assert.That(ranked.Select(a => a.Address), Is.EqualTo(new[] { "192.168.1.2" }));
        }
    }
}
