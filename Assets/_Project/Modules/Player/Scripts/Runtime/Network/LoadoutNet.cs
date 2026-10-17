using System;
using Trivias.Shared.Profile;
using Unity.Collections;
using Unity.Netcode;

namespace Trivias.Player.Network
{
    /// <summary>Lo que lleva puesto el concursante, en red: un id de cosmético por hueco.</summary>
    public struct LoadoutNet : INetworkSerializable, IEquatable<LoadoutNet>
    {
        public FixedString32Bytes Head;
        public FixedString32Bytes Face;
        public FixedString32Bytes Neck;

        public static LoadoutNet From(in CosmeticLoadout loadout)
        {
            var net = new LoadoutNet();
            net.Head.CopyFromTruncated(loadout.Head);
            net.Face.CopyFromTruncated(loadout.Face);
            net.Neck.CopyFromTruncated(loadout.Neck);
            return net;
        }

        public CosmeticLoadout ToLoadout() => new(Head.ToString(), Face.ToString(), Neck.ToString());

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Head);
            serializer.SerializeValue(ref Face);
            serializer.SerializeValue(ref Neck);
        }

        public bool Equals(LoadoutNet other) =>
            Head.Equals(other.Head) && Face.Equals(other.Face) && Neck.Equals(other.Neck);

        public override bool Equals(object obj) => obj is LoadoutNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Head, Face, Neck);
    }
}
