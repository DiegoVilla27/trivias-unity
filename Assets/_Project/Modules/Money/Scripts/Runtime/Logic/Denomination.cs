namespace Trivias.Money.Logic
{
    /// <summary>Denominaciones del dinero del concurso (GDD 4.1). Su valor vive en MoneyConfig.</summary>
    public enum Denomination : byte
    {
        Bundle = 0,
        Bag = 1,
        GoldBar = 2,
    }
}
