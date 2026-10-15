namespace Trivias.Money.Logic
{
    /// <summary>Resultado económico de resolver una pregunta.</summary>
    public readonly struct RoundOutcome
    {
        /// <summary>Dinero perdido: bandejas incorrectas y, con la regla provisional, lo que no estaba en bandejas.</summary>
        public readonly long Lost;

        /// <summary>Dinero de la bandeja correcta, que vuelve a la mesa.</summary>
        public readonly long Kept;

        /// <summary>Dinero extra por los concursantes apostados en la respuesta correcta.</summary>
        public readonly long Bonus;

        /// <summary>Dinero del equipo tras resolver.</summary>
        public readonly long Total;

        public RoundOutcome(long lost, long kept, long bonus, long total)
        {
            Lost = lost;
            Kept = kept;
            Bonus = bonus;
            Total = total;
        }
    }
}
