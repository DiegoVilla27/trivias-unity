using System;
using Trivias.SelfBet.Logic;
using Unity.Netcode;

namespace Trivias.SelfBet.Network
{
    /// <summary>Estado de comodín de un concursante en red (11 bytes).</summary>
    public struct BettorNet : INetworkSerializable, IEquatable<BettorNet>
    {
        private const byte RESERVE = 1;
        private const byte LOCKED = 2;
        private const byte ELIMINATED = 4;
        private const byte LEFT = 8;

        public ulong ClientId;
        public byte PersonalUses;
        public sbyte Booth;
        public byte Flags;

        public static BettorNet From(in Bettor bettor) => new()
        {
            ClientId = bettor.ClientId,
            PersonalUses = (byte)bettor.PersonalUses,
            Booth = (sbyte)bettor.Booth,
            Flags = (byte)((bettor.UsesReserve ? RESERVE : 0) | (bettor.IsLocked ? LOCKED : 0) |
                           (bettor.IsEliminated ? ELIMINATED : 0) | (bettor.HasLeft ? LEFT : 0)),
        };

        public Bettor ToBettor() => new(ClientId, PersonalUses, Booth, (Flags & RESERVE) != 0,
            (Flags & LOCKED) != 0, (Flags & ELIMINATED) != 0, (Flags & LEFT) != 0);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PersonalUses);
            serializer.SerializeValue(ref Booth);
            serializer.SerializeValue(ref Flags);
        }

        public bool Equals(BettorNet other) =>
            ClientId == other.ClientId && PersonalUses == other.PersonalUses && Booth == other.Booth && Flags == other.Flags;

        public override bool Equals(object obj) => obj is BettorNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ClientId, PersonalUses, Booth, Flags);
    }
}
