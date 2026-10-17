namespace Trivias.Spectator.Logic
{
    /// <summary>Textos de la cinta del espectador (boceto 07 del HUD; C# puro, cubierto por tests).</summary>
    public static class SpectatorText
    {
        public const string TITLE = "ELIMINADO";
        public const string SUBTITLE = "Eres espectador hasta la siguiente partida";
        public const string OVERVIEW = "Vista general del plató";
        private const string GOLD = "#FFD15C";

        /// <param name="followedName">A quién mira, o vacío en la vista general.</param>
        public static string Watching(string followedName) =>
            string.IsNullOrEmpty(followedName) ? OVERVIEW : $"Viendo a <color={GOLD}><b>{followedName}</b></color>";

        /// <summary>Qué hacen A/D: cambiar de concursante o, desde la vista general, seguir a uno.</summary>
        public static string FollowAction(bool following) => following ? "Cambiar jugador" : "Seguir a un concursante";

        /// <summary>Toda la cinta tal como se lee (para pruebas y registros).</summary>
        /// <param name="anyoneToFollow">Queda algún concursante activo al que seguir.</param>
        public static string Banner(string followedName, bool anyoneToFollow)
        {
            var following = !string.IsNullOrEmpty(followedName);
            var text = $"{TITLE} · {SUBTITLE}\n{Watching(followedName)}";
            if (anyoneToFollow)
                text += $"\nA/D {FollowAction(following)}";
            if (following)
                text += " · Espacio Vista general";
            return text + " · Esc Menú";
        }
    }
}
