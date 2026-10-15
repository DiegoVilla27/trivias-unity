namespace Trivias.SelfBet.Logic
{
    /// <summary>
    /// Reparto de los seis usos de comodín según el tamaño inicial del grupo (GDD 5.1):
    /// 2 → 3 por jugador; 3 → 2; 4 → 1 + 2 de reserva; 5 → 1 + 1 de reserva; 6 → 1.
    /// Con un solo jugador (modo prueba) recibe los seis.
    /// </summary>
    public static class SelfBetDistribution
    {
        public const int TEAM_USES = 6;

        public static void For(int players, out int personalUses, out int reserve)
        {
            if (players <= 0)
            {
                personalUses = 0;
                reserve = 0;
                return;
            }

            personalUses = TEAM_USES / players;
            if (personalUses < 1)
                personalUses = 1;
            reserve = TEAM_USES - personalUses * players;
            if (reserve < 0)
                reserve = 0;
        }
    }
}
