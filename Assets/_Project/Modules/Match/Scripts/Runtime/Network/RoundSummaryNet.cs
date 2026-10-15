using System;
using Trivias.Money.Logic;
using Unity.Netcode;

namespace Trivias.Match.Network
{
    /// <summary>Resultado de la ronda en red, para mostrar lo perdido y lo conservado en todos los clientes.</summary>
    public struct RoundSummaryNet : INetworkSerializable, IEquatable<RoundSummaryNet>
    {
        public long Lost;
        public long Kept;
        public long Bonus;
        public long Total;

        public static RoundSummaryNet From(in RoundOutcome outcome) => new()
        {
            Lost = outcome.Lost,
            Kept = outcome.Kept,
            Bonus = outcome.Bonus,
            Total = outcome.Total,
        };

        public RoundOutcome ToOutcome() => new(Lost, Kept, Bonus, Total);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Lost);
            serializer.SerializeValue(ref Kept);
            serializer.SerializeValue(ref Bonus);
            serializer.SerializeValue(ref Total);
        }

        public bool Equals(RoundSummaryNet other) =>
            Lost == other.Lost && Kept == other.Kept && Bonus == other.Bonus && Total == other.Total;

        public override bool Equals(object obj) => obj is RoundSummaryNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Lost, Kept, Bonus, Total);
    }
}
