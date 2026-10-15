namespace Trivias.Shared.Profile
{
    /// <summary>Hueco donde se lleva un cosmético. En cada hueco solo puede ir uno.</summary>
    public enum CosmeticSlot
    {
        Head = 0,
        Face = 1,
        Neck = 2,
        Color = 3,
    }

    public static class CosmeticSlots
    {
        public const int COUNT = 4;
    }
}
