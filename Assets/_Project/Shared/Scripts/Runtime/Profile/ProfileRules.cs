namespace Trivias.Shared.Profile
{
    /// <summary>Reglas de fichas y compras (GDD 11). C# puro sobre <see cref="ProfileData"/>, cubierto por tests.</summary>
    public static class ProfileRules
    {
        /// <summary>Máximo de partidas recordadas para evitar entregas dobles.</summary>
        public const int AWARD_HISTORY = 50;

        /// <summary>Entrega las fichas de una partida una sola vez. Devuelve false si ya se habían entregado.</summary>
        public static bool AwardMatch(ProfileData profile, string matchId, int tokens)
        {
            if (string.IsNullOrEmpty(matchId) || profile.awardedMatches.Contains(matchId))
                return false;

            profile.awardedMatches.Add(matchId);
            if (profile.awardedMatches.Count > AWARD_HISTORY)
                profile.awardedMatches.RemoveAt(0);
            if (tokens > 0)
                profile.tokens += tokens;
            return true;
        }

        public static bool Owns(ProfileData profile, string itemId) => profile.owned.Contains(itemId);

        public static bool CanBuy(ProfileData profile, string itemId, int price) =>
            !string.IsNullOrEmpty(itemId) && price >= 0 && !Owns(profile, itemId) && profile.tokens >= price;

        /// <summary>Compra permanente: descuenta las fichas y añade el cosmético. No equipa.</summary>
        public static bool TryBuy(ProfileData profile, string itemId, int price)
        {
            if (!CanBuy(profile, itemId, price))
                return false;
            profile.tokens -= price;
            profile.owned.Add(itemId);
            return true;
        }

        /// <summary>Equipa un cosmético comprado en su hueco (sustituye al anterior).</summary>
        public static bool TryEquip(ProfileData profile, CosmeticSlot slot, string itemId)
        {
            if (!Owns(profile, itemId))
                return false;
            profile.equipped[(int)slot] = itemId;
            return true;
        }

        public static void Unequip(ProfileData profile, CosmeticSlot slot) => profile.equipped[(int)slot] = string.Empty;
    }
}
