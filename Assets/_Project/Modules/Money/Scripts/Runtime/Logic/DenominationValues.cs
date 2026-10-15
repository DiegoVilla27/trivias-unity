using System;

namespace Trivias.Money.Logic
{
    /// <summary>Valor de cada denominación. Se construye desde MoneyConfig para que la lógica no dependa de Unity.</summary>
    public sealed class DenominationValues
    {
        private readonly long[] _values;

        public DenominationValues(long bundle, long bag, long goldBar)
        {
            if (bundle <= 0 || bag <= bundle || goldBar <= bag)
                throw new ArgumentException("Las denominaciones deben ser positivas y crecientes.");
            _values = new[] { bundle, bag, goldBar };
        }

        public long Of(Denomination denomination) => _values[(int)denomination];
    }
}
