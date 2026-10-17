using System;

namespace Trivias.Shared.Profile
{
    /// <summary>Lo que lleva puesto un concursante: un id de cosmético (o vacío) por hueco.</summary>
    public readonly struct CosmeticLoadout : IEquatable<CosmeticLoadout>
    {
        public readonly string Head;
        public readonly string Face;
        public readonly string Neck;
        public readonly string Mouth;
        public readonly string Back;

        public CosmeticLoadout(string head, string face, string neck, string mouth = null, string back = null)
        {
            Head = head ?? string.Empty;
            Face = face ?? string.Empty;
            Neck = neck ?? string.Empty;
            Mouth = mouth ?? string.Empty;
            Back = back ?? string.Empty;
        }

        public string this[CosmeticSlot slot] => slot switch
        {
            CosmeticSlot.Head => Head,
            CosmeticSlot.Face => Face,
            CosmeticSlot.Neck => Neck,
            CosmeticSlot.Mouth => Mouth,
            _ => Back,
        };

        public bool Equals(CosmeticLoadout other) =>
            Head == other.Head && Face == other.Face && Neck == other.Neck && Mouth == other.Mouth && Back == other.Back;

        public override bool Equals(object obj) => obj is CosmeticLoadout other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Head, Face, Neck, Mouth, Back);
    }
}
