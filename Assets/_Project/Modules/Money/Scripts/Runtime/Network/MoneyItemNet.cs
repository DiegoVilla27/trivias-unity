using System;
using Trivias.Money.Logic;
using Unity.Netcode;

namespace Trivias.Money.Network
{
    /// <summary>Representación en red de <see cref="MoneyItem"/>: 13 bytes por objeto.</summary>
    public struct MoneyItemNet : INetworkSerializable, IEquatable<MoneyItemNet>
    {
        public byte Denomination;
        public byte Place;
        public byte PlaceIndex;
        public ushort Slot;
        public ulong Holder;

        public static MoneyItemNet From(in MoneyItem item) => new()
        {
            Denomination = (byte)item.Denomination,
            Place = (byte)item.Place,
            PlaceIndex = item.PlaceIndex,
            Slot = item.Slot,
            Holder = item.Holder,
        };

        public MoneyItem ToItem() =>
            new((Logic.Denomination)Denomination, (MoneyPlace)Place, PlaceIndex, Slot, Holder);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Denomination);
            serializer.SerializeValue(ref Place);
            serializer.SerializeValue(ref PlaceIndex);
            serializer.SerializeValue(ref Slot);
            serializer.SerializeValue(ref Holder);
        }

        public bool Equals(MoneyItemNet other) =>
            Denomination == other.Denomination && Place == other.Place && PlaceIndex == other.PlaceIndex &&
            Slot == other.Slot && Holder == other.Holder;

        public override bool Equals(object obj) => obj is MoneyItemNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Denomination, Place, PlaceIndex, Slot, Holder);
    }
}
