using System.Globalization;

namespace Trivias.Money.Logic
{
    /// <summary>Formato de cantidades y nombres en español ("100.000", "fajo").</summary>
    public static class MoneyFormat
    {
        private static readonly NumberFormatInfo Format = new()
        {
            NumberGroupSeparator = ".",
            NumberDecimalDigits = 0,
        };

        public static string Amount(long value) => value.ToString("N0", Format);

        public static string Name(Denomination denomination) => denomination switch
        {
            Denomination.Bundle => "fajo",
            Denomination.Bag => "bolsa",
            Denomination.GoldBar => "lingote",
            _ => "dinero",
        };
    }
}
