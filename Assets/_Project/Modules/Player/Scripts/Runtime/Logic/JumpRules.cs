using System;

namespace Trivias.Player.Logic
{
    /// <summary>Salto del concursante: cuándo se puede y con qué velocidad despega. C# puro, probado en EditMode.</summary>
    public static class JumpRules
    {
        /// <summary>Solo desde el suelo, con el control abierto (no en la sala de espera ni en pausa) y tras la espera.</summary>
        public static bool CanJump(bool grounded, bool canMove, float cooldownLeft) =>
            grounded && canMove && cooldownLeft <= 0f;

        /// <summary>Velocidad vertical para alcanzar <paramref name="height"/> con la gravedad dada (negativa).</summary>
        public static float TakeOffSpeed(float height, float gravity) =>
            height <= 0f || gravity >= 0f ? 0f : (float)Math.Sqrt(height * -2f * gravity);
    }
}
