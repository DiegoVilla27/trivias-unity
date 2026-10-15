namespace Trivias.Spectator.Logic
{
    /// <summary>Rótulo del espectador (C# puro, cubierto por tests).</summary>
    public static class SpectatorText
    {
        private const string HEADER = "<color=#FF6B6B><b>ELIMINADO</b></color> · Eres espectador hasta la siguiente partida";
        private const string KEYS = "<size=80%><color=#FFFFFFAA>A/D o flechas: cambiar · Espacio: vista general</color></size>";

        /// <param name="followedName">A quién mira, o vacío en la vista general.</param>
        /// <param name="anyoneToFollow">Queda algún concursante activo al que seguir.</param>
        public static string Banner(string followedName, bool anyoneToFollow)
        {
            if (!string.IsNullOrEmpty(followedName))
                return $"{HEADER}\nViendo a <b>{followedName}</b>   {KEYS}";
            return anyoneToFollow
                ? $"{HEADER}\nVista general del plató   <size=80%><color=#FFFFFFAA>A/D o flechas: seguir a un concursante</color></size>"
                : $"{HEADER}\nVista general del plató";
        }
    }
}
