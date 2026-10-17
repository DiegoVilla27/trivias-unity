using System;

namespace Trivias.Shared.Profile
{
    /// <summary>Lo que lleva puesto un concursante: un id de cosmético (o vacío) por hueco.</summary>
    public readonly struct CosmeticLoadout : IEquatable<CosmeticLoadout>
    {
        public readonly string Head;
        public readonly string Face;
        public readonly string Neck;

        public CosmeticLoadout(string head, string face, string neck)
        {
            Head = head ?? string.Empty;
            Face = face ?? string.Empty;
            Neck = neck ?? string.Empty;
        }

        public string this[CosmeticSlot slot] => slot switch
        {
            CosmeticSlot.Head => Head,
            CosmeticSlot.Face => Face,
            _ => Neck,
        };

        public bool Equals(CosmeticLoadout other) =>
            Head == other.Head && Face == other.Face && Neck == other.Neck;

        public override bool Equals(object obj) => obj is CosmeticLoadout other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Head, Face, Neck);
    }
}
