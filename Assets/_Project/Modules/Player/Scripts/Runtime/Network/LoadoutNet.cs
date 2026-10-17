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
        public FixedString32Bytes Mouth;
        public FixedString32Bytes Back;

        public static LoadoutNet From(in CosmeticLoadout loadout)
        {
            var net = new LoadoutNet();
            net.Head.CopyFromTruncated(loadout.Head);
            net.Face.CopyFromTruncated(loadout.Face);
            net.Neck.CopyFromTruncated(loadout.Neck);
            net.Mouth.CopyFromTruncated(loadout.Mouth);
            net.Back.CopyFromTruncated(loadout.Back);
            return net;
        }

        public CosmeticLoadout ToLoadout() => new(Head.ToString(), Face.ToString(), Neck.ToString(), Mouth.ToString(), Back.ToString());

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Head);
            serializer.SerializeValue(ref Face);
            serializer.SerializeValue(ref Neck);
            serializer.SerializeValue(ref Mouth);
            serializer.SerializeValue(ref Back);
        }

        public bool Equals(LoadoutNet other) =>
            Head.Equals(other.Head) && Face.Equals(other.Face) && Neck.Equals(other.Neck) && Mouth.Equals(other.Mouth) && Back.Equals(other.Back);

        public override bool Equals(object obj) => obj is LoadoutNet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Head, Face, Neck, Mouth, Back);
    }
}
