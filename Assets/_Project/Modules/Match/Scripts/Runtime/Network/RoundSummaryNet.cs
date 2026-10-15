using System;
using Trivias.Money.Logic;
using Unity.Collections;
using Unity.Netcode;

namespace Trivias.Match.Network
{
    /// <summary>Resultado de la ronda en red: dinero, multiplicador y concursantes expulsados.</summary>
    public struct RoundSummaryNet : INetworkSerializable, IEquatable<RoundSummaryNet>
    {
        public long Lost;
        public long Kept;
        public long Bonus;
        public long Total;
        public byte Multiplier;
        public FixedString128Bytes Eliminated;

        public static RoundSummaryNet From(in RoundOutcome outcome, int multiplier, string eliminated)
        {
            var names = new FixedString128Bytes();
            names.CopyFromTruncated(eliminated); // Nunca falla por un texto largo.
            return new RoundSummaryNet
            {
                Lost = outcome.Lost,
                Kept = outcome.Kept,
                Bonus = outcome.Bonus,
                Total = outcome.Total,
                Multiplier = (byte)multiplier,
                Eliminated = names,
            };
        }

        public RoundOutcome ToOutcome() => new(Lost, Kept, Bonus, Total);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Lost);
            serializer.SerializeValue(ref Kept);
            serializer.SerializeValue(ref Bonus);
            serializer.SerializeValue(ref Total);
            serializer.SerializeValue(ref Multiplier);
            serializer.SerializeValue(ref Eliminated);
        }

        public bool Equals(RoundSummaryNet other) =>
            Lost == other.Lost && Kept == other.Kept && Bonus == other.Bonus && Total == other.Total &&
            Multiplier == other.Multiplier && Eliminated.Equals(other.Eliminated);

        public override bool Equals(object obj) => obj is RoundSummaryNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Lost, Kept, Bonus, Total, Multiplier, Eliminated);
    }
}
