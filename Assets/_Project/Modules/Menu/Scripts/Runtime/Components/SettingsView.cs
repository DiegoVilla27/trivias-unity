using TMPro;
using Trivias.Shared.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Configuración: volúmenes, sensibilidad e inversión del ratón y pantalla completa. Los cambios se aplican al momento
    /// (el sonido y la cámara escuchan <see cref="GameSettings"/>) y se guardan al cerrar el panel.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameSettings _settings;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _musicVolume;
        [SerializeField] private Slider _effectsVolume;
        [SerializeField] private Slider _mouseSensitivity;
        [SerializeField] private TextMeshProUGUI _masterValue;
        [SerializeField] private TextMeshProUGUI _musicValue;
        [SerializeField] private TextMeshProUGUI _effectsValue;
        [SerializeField] private TextMeshProUGUI _sensitivityValue;
        [SerializeField] private Toggle _invertY;
        [SerializeField] private Toggle _fullscreen;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _closeButton;

        public bool IsOpen => _panel.activeSelf;

        private void Start() => ApplyScreen(); // La pantalla completa guardada se aplica al arrancar.

        private void OnEnable()
        {
            _masterVolume.onValueChanged.AddListener(_settings.SetMasterVolume);
            _musicVolume.onValueChanged.AddListener(_settings.SetMusicVolume);
            _effectsVolume.onValueChanged.AddListener(_settings.SetEffectsVolume);
            _mouseSensitivity.onValueChanged.AddListener(_settings.SetMouseSensitivity);
            _invertY.onValueChanged.AddListener(_settings.SetInvertY);
            _fullscreen.onValueChanged.AddListener(OnFullscreen);
            _resetButton.onClick.AddListener(OnReset);
            _closeButton.onClick.AddListener(Close);
            _settings.Changed += RefreshLabels;
        }

        private void OnDisable()
        {
            _masterVolume.onValueChanged.RemoveListener(_settings.SetMasterVolume);
            _musicVolume.onValueChanged.RemoveListener(_settings.SetMusicVolume);
            _effectsVolume.onValueChanged.RemoveListener(_settings.SetEffectsVolume);
            _mouseSensitivity.onValueChanged.RemoveListener(_settings.SetMouseSensitivity);
            _invertY.onValueChanged.RemoveListener(_settings.SetInvertY);
            _fullscreen.onValueChanged.RemoveListener(OnFullscreen);
            _resetButton.onClick.RemoveListener(OnReset);
            _closeButton.onClick.RemoveListener(Close);
            _settings.Changed -= RefreshLabels;
        }

        public void Open()
        {
            _panel.SetActive(true);
            SyncControls();
        }

        public void Close()
        {
            _panel.SetActive(false);
            _settings.Save();
        }

        private void OnFullscreen(bool fullscreen)
        {
            _settings.SetFullscreen(fullscreen);
            ApplyScreen();
        }

        private void OnReset()
        {
            _settings.ResetToDefaults();
            SyncControls();
            ApplyScreen();
        }

        private void ApplyScreen() =>
            Screen.fullScreenMode = _settings.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        /// <summary>Pone los controles en los valores guardados sin disparar sus avisos.</summary>
        private void SyncControls()
        {
            _masterVolume.SetValueWithoutNotify(_settings.MasterVolume);
            _musicVolume.SetValueWithoutNotify(_settings.MusicVolume);
            _effectsVolume.SetValueWithoutNotify(_settings.EffectsVolume);
            _mouseSensitivity.SetValueWithoutNotify(_settings.MouseSensitivity);
            _invertY.SetIsOnWithoutNotify(_settings.InvertY);
            _fullscreen.SetIsOnWithoutNotify(_settings.Fullscreen);
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (!IsOpen)
                return;
            _masterValue.text = Percent(_settings.MasterVolume);
            _musicValue.text = Percent(_settings.MusicVolume);
            _effectsValue.text = Percent(_settings.EffectsVolume);
            _sensitivityValue.text = $"x{_settings.MouseSensitivity:0.0}";
        }

        private static string Percent(float value) => $"{Mathf.RoundToInt(value * 100f)}%";
    }
}
