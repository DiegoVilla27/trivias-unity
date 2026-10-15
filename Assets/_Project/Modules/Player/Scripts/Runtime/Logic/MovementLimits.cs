namespace Trivias.Player.Logic
{
    /// <summary>Restricción de movimiento a un círculo en el plano horizontal. C# puro, cubierto por tests.</summary>
    public static class MovementLimits
    {
        /// <summary>
        /// Devuelve el desplazamiento permitido (dx, dz) para que el punto final no salga del círculo.
        /// Si ya estaba fuera, solo deja moverse hacia dentro.
        /// </summary>
        public static void ClampMove(float x, float z, ref float dx, ref float dz, float centerX, float centerZ, float radius)
        {
            var targetX = x + dx - centerX;
            var targetZ = z + dz - centerZ;
            var distanceSqr = targetX * targetX + targetZ * targetZ;
            if (distanceSqr <= radius * radius)
                return;

            var distance = (float)System.Math.Sqrt(distanceSqr);
            var scale = radius / distance;
            dx = centerX + targetX * scale - x;
            dz = centerZ + targetZ * scale - z;
        }
    }
}
