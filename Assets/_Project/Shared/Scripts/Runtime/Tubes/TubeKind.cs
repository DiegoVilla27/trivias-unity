namespace Trivias.Shared.Tubes
{
    /// <summary>Tubos de cada cabina: el del dinero (del mueble al techo) y el del concursante (sobre la plataforma).</summary>
    public enum TubeKind : byte
    {
        Money = 0,
        Player = 1,
    }
}
