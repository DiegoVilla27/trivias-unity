namespace Trivias.Money.Logic
{
    /// <summary>Dónde está un objeto de dinero (GDD 8: mesa, transportado, bandeja y perdido).</summary>
    public enum MoneyPlace : byte
    {
        Table = 0,
        Carried = 1,
        Tray = 2,
        Lost = 3,
    }
}
