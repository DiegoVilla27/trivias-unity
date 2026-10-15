using System;
using System.Collections.Generic;
using Trivias.Shared.Rooms;

namespace Trivias.Networking.Logic
{
    /// <summary>Una dirección IPv4 de este ordenador con el adaptador de red al que pertenece.</summary>
    public readonly struct LocalInterfaceAddress
    {
        public readonly string Address;
        public readonly string InterfaceName;
        public readonly string InterfaceDescription;

        public LocalInterfaceAddress(string address, string interfaceName, string interfaceDescription)
        {
            Address = address;
            InterfaceName = interfaceName ?? string.Empty;
            InterfaceDescription = interfaceDescription ?? string.Empty;
        }
    }

    /// <summary>
    /// Decide qué direcciones del host sirven para invitar y en qué orden (C# puro, probado en EditMode):
    /// primero las de una red virtual (para amigos por internet), luego las de la red local; nunca las internas.
    /// </summary>
    public static class HostAddressRanker
    {
        public const string LOOPBACK = "127.0.0.1";

        public static List<RoomAddress> Rank(IEnumerable<LocalInterfaceAddress> addresses)
        {
            var virtualNetworks = new List<RoomAddress>();
            var local = new List<RoomAddress>();
            var seen = new HashSet<string>();

            foreach (var candidate in addresses)
            {
                if (!TryParse(candidate.Address, out var octets) || !seen.Add(candidate.Address))
                    continue;
                if (octets[0] == 127 || octets[0] == 0 || (octets[0] == 169 && octets[1] == 254) || octets[0] >= 224)
                    continue; // Este equipo, sin configurar, autoasignada o multidifusión: no sirven para invitar.

                var network = VirtualNetworkName(candidate, octets);
                if (network.Length > 0)
                    virtualNetworks.Add(new RoomAddress(candidate.Address, RoomAddressKind.VirtualNetwork, network));
                else
                    local.Add(new RoomAddress(candidate.Address, RoomAddressKind.LocalNetwork, string.Empty));
            }

            virtualNetworks.AddRange(local);
            if (virtualNetworks.Count == 0)
                virtualNetworks.Add(new RoomAddress(LOOPBACK, RoomAddressKind.ThisComputer, string.Empty));
            return virtualNetworks;
        }

        /// <summary>Reconoce la red virtual por el adaptador o, si no, por el rango típico de cada programa.</summary>
        private static string VirtualNetworkName(in LocalInterfaceAddress candidate, int[] octets)
        {
            var adapter = (candidate.InterfaceName + " " + candidate.InterfaceDescription).ToLowerInvariant();
            if (adapter.Contains("zerotier") || candidate.InterfaceName.StartsWith("feth", StringComparison.Ordinal) ||
                candidate.InterfaceName.StartsWith("zt", StringComparison.Ordinal))
                return "ZeroTier";
            if (adapter.Contains("radmin") || octets[0] == 26)
                return "Radmin VPN";
            if (adapter.Contains("tailscale") || (octets[0] == 100 && octets[1] >= 64 && octets[1] <= 127))
                return "Tailscale";
            if (adapter.Contains("hamachi") || octets[0] == 25)
                return "Hamachi";
            return string.Empty;
        }

        private static bool TryParse(string address, out int[] octets)
        {
            octets = new int[4];
            if (string.IsNullOrEmpty(address))
                return false;
            var parts = address.Split('.');
            if (parts.Length != 4)
                return false;
            for (var i = 0; i < 4; i++)
            {
                if (!int.TryParse(parts[i], out octets[i]) || octets[i] < 0 || octets[i] > 255)
                    return false;
            }
            return true;
        }
    }
}
