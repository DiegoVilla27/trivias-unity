using System;

namespace Trivias.Shared.Settings
{
    /// <summary>Límites de las opciones (C# puro, probado en EditMode).</summary>
    public static class SettingsRules
    {
        public const float MIN_SENSITIVITY = 0.2f;
        public const float MAX_SENSITIVITY = 3f;

        public static float ClampVolume(float volume) => float.IsNaN(volume) ? 1f : Math.Clamp(volume, 0f, 1f);

        public static float ClampSensitivity(float sensitivity) =>
            float.IsNaN(sensitivity) ? 1f : Math.Clamp(sensitivity, MIN_SENSITIVITY, MAX_SENSITIVITY);

        /// <summary>Vertical del ratón según «invertir eje»: positivo mira hacia arriba.</summary>
        public static float LookVertical(float mouseY, bool invertY) => invertY ? -mouseY : mouseY;
    }
}
