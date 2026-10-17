namespace Trivias.Shared.Profile
{
    /// <summary>Dónde se lleva un cosmético: uno por hueco (cabeza, cara, cuello, boca y espalda).</summary>
    public enum CosmeticSlot
    {
        Head = 0,
        Face = 1,
        Neck = 2,
        Mouth = 3,
        Back = 4,
    }

    public static class CosmeticSlots
    {
        public const int COUNT = 5;
    }
}
