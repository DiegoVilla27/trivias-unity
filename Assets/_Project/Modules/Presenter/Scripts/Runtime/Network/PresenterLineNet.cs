using System;
using Unity.Netcode;

namespace Trivias.Presenter.Network
{
    /// <summary>Frase elegida por el host (índice en el catálogo + variables). Cada cliente la formatea localmente.</summary>
    public struct PresenterLineNet : INetworkSerializable, IEquatable<PresenterLineNet>
    {
        public const ushort SILENT = ushort.MaxValue;

        public ushort Phrase;
        public ushort Sequence;
        public long Amount;
        public long Prize;
        public byte Survivors;
        public byte Round;
        public byte Multiplier;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phrase);
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Prize);
            serializer.SerializeValue(ref Survivors);
            serializer.SerializeValue(ref Round);
            serializer.SerializeValue(ref Multiplier);
        }

        public bool Equals(PresenterLineNet other) =>
            Phrase == other.Phrase && Sequence == other.Sequence && Amount == other.Amount && Prize == other.Prize &&
            Survivors == other.Survivors && Round == other.Round && Multiplier == other.Multiplier;

        public override bool Equals(object obj) => obj is PresenterLineNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Phrase, Sequence, Amount, Prize, Survivors, Round, Multiplier);
    }
}
