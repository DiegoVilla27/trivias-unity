using System;

namespace Trivias.Player.Logic
{
    /// <summary>Cálculo de ángulos de cámara. C# puro, cubierto por tests EditMode.</summary>
    public static class LookAngles
    {
        /// <summary>Normaliza un ángulo al rango (-180, 180].</summary>
        public static float Wrap(float degrees)
        {
            degrees %= 360f;
            if (degrees > 180f) degrees -= 360f;
            else if (degrees <= -180f) degrees += 360f;
            return degrees;
        }

        public static float ClampPitch(float pitch, float min, float max)
        {
            pitch = Wrap(pitch);
            return pitch < min ? min : pitch > max ? max : pitch;
        }

        /// <summary>Ángulo de giro en el que mirar según la entrada de movimiento y la orientación de la cámara.</summary>
        public static float MoveYaw(float inputX, float inputY, float cameraYaw) =>
            (float)(Math.Atan2(inputX, inputY) * RAD_TO_DEG) + cameraYaw;

        private const double RAD_TO_DEG = 180.0 / Math.PI;
    }
}
