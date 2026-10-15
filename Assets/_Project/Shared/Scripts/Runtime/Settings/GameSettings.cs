using System;
using System.IO;
using UnityEngine;

namespace Trivias.Shared.Settings
{
    /// <summary>
    /// Opciones del jugador (volúmenes, ratón, pantalla). Las leen el sonido, el concursante y el menú sin conocerse.
    /// Se cargan la primera vez que se piden; los cambios avisan con <see cref="Changed"/> y se guardan con <see cref="Save"/>
    /// (el panel de configuración guarda al cerrarse: mover un deslizador no escribe en disco cada frame).
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Settings/Game Settings", fileName = "GameSettings")]
    public sealed class GameSettings : ScriptableObject
    {
        private const string FILE_NAME = "trivias-settings.json";

        public event Action Changed;

        private SettingsData _data;
        private string _path;
        private bool _dirty;

        private SettingsData Data
        {
            get
            {
                if (_data == null)
                {
                    _path ??= Path.Combine(Application.persistentDataPath, FILE_NAME);
                    _data = SettingsStore.Load(_path);
                }
                return _data;
            }
        }

        public float MasterVolume => Data.masterVolume;
        public float MusicVolume => Data.musicVolume;
        public float EffectsVolume => Data.effectsVolume;
        public float MouseSensitivity => Data.mouseSensitivity;
        public bool InvertY => Data.invertY;
        public bool Fullscreen => Data.fullscreen;
        public string LastAddress => Data.lastAddress;

        public void SetMasterVolume(float value) => Apply(() => Data.masterVolume = SettingsRules.ClampVolume(value));
        public void SetMusicVolume(float value) => Apply(() => Data.musicVolume = SettingsRules.ClampVolume(value));
        public void SetEffectsVolume(float value) => Apply(() => Data.effectsVolume = SettingsRules.ClampVolume(value));
        public void SetMouseSensitivity(float value) => Apply(() => Data.mouseSensitivity = SettingsRules.ClampSensitivity(value));
        public void SetInvertY(bool value) => Apply(() => Data.invertY = value);
        public void SetFullscreen(bool value) => Apply(() => Data.fullscreen = value);

        public void SetLastAddress(string address)
        {
            Apply(() => Data.lastAddress = address ?? string.Empty);
            Save();
        }

        /// <summary>Vuelve a las opciones de fábrica (conserva la última dirección).</summary>
        public void ResetToDefaults()
        {
            var address = Data.lastAddress;
            _data = new SettingsData { lastAddress = address };
            _dirty = true;
            Changed?.Invoke();
        }

        public void Save()
        {
            if (!_dirty || _data == null)
                return;
            _dirty = false;
            SettingsStore.Save(_path, _data);
        }

        /// <summary>Solo pruebas: usa otro archivo y descarta lo cargado.</summary>
        public void UseStoragePath(string path)
        {
            _path = path;
            _data = null;
            _dirty = false;
            Changed?.Invoke();
        }

        private void Apply(Action change)
        {
            change();
            _dirty = true;
            Changed?.Invoke();
        }

        private void OnEnable()
        {
            _data = null;
            _path = null;
            _dirty = false;
        }
    }
}
