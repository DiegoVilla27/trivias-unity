using System;

namespace Trivias.Money.Logic
{
    /// <summary>
    /// Cuántos objetos de cada denominación puede llevar un concursante a la vez.
    /// Solo se lleva un tipo de denominación por viaje (GDD 4.2).
    /// </summary>
    public sealed class CarryLimits
    {
        private readonly int[] _limits;

        public CarryLimits(int bundles, int bags, int goldBars)
        {
            if (bundles < 1 || bags < 1 || goldBars < 1)
                throw new ArgumentException("Cada denominación debe permitir al menos un objeto.");
            _limits = new[] { bundles, bags, goldBars };
        }

        public int For(Denomination denomination) => _limits[(int)denomination];
    }
}
