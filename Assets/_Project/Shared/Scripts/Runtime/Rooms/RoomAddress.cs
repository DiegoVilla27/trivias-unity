namespace Trivias.Shared.Rooms
{
    public enum RoomAddressKind : byte
    {
        /// <summary>Red virtual (ZeroTier, Radmin, Tailscale): para amigos por internet.</summary>
        VirtualNetwork = 0,

        /// <summary>Red local (misma wifi o cable).</summary>
        LocalNetwork = 1,

        /// <summary>Solo este ordenador (pruebas con varias ventanas).</summary>
        ThisComputer = 2,
    }

    /// <summary>Dirección del host que se puede compartir para que otros se unan.</summary>
    public readonly struct RoomAddress
    {
        public readonly string Address;
        public readonly RoomAddressKind Kind;

        /// <summary>Nombre de la red si se reconoce (p. ej. «ZeroTier»), o vacío.</summary>
        public readonly string NetworkName;

        public RoomAddress(string address, RoomAddressKind kind, string networkName)
        {
            Address = address;
            Kind = kind;
            NetworkName = networkName ?? string.Empty;
        }
    }
}
