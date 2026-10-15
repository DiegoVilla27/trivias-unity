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
            // La estación de cambio debe conservar el valor exacto: cada denominación es múltiplo de la anterior.
            if (bag % bundle != 0 || goldBar % bag != 0)
                throw new ArgumentException("Cada denominación debe ser múltiplo exacto de la anterior.");
            _values = new[] { bundle, bag, goldBar };
        }

        public long Of(Denomination denomination) => _values[(int)denomination];
    }
}
