using System;

namespace Trivias.Shared.Settings
{
    /// <summary>Opciones del jugador en este ordenador (JSON en la carpeta de datos del juego).</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float effectsVolume = 1f;

        /// <summary>Multiplicador de la sensibilidad del ratón (1 = la de fábrica).</summary>
        public float mouseSensitivity = 1f;

        public bool invertY;
        public bool fullscreen = true;

        /// <summary>Última dirección escrita en «Unirme a una sala».</summary>
        public string lastAddress = string.Empty;

        public void Normalize()
        {
            masterVolume = SettingsRules.ClampVolume(masterVolume);
            musicVolume = SettingsRules.ClampVolume(musicVolume);
            effectsVolume = SettingsRules.ClampVolume(effectsVolume);
            mouseSensitivity = SettingsRules.ClampSensitivity(mouseSensitivity);
            lastAddress ??= string.Empty;
        }
    }
}
