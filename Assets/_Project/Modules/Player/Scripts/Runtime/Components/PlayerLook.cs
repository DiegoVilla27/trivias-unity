using Trivias.Player.Data;
using Trivias.Player.Logic;
using Trivias.Shared.Settings;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Orienta el objetivo de la cámara con el ratón. La cámara (módulo Camera) sigue a <see cref="CameraRoot"/>.
    /// Solo lo activa el dueño. Sensibilidad e inversión del eje vertical salen de las opciones del jugador.
    /// </summary>
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Transform _cameraRoot;
        [SerializeField] private GameSettings _settings;

        private float _yaw;
        private float _pitch;

        public Transform CameraRoot => _cameraRoot;

        private void OnEnable()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = 0f;
        }

        private void LateUpdate()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                var look = _input.Look;
                var sensitivity = _config.LookSensitivity * _settings.MouseSensitivity;
                _yaw += look.x * sensitivity;
                _pitch -= SettingsRules.LookVertical(look.y, _settings.InvertY) * sensitivity;
            }

            _yaw = LookAngles.Wrap(_yaw);
            _pitch = LookAngles.ClampPitch(_pitch, _config.MinPitch, _config.MaxPitch);
            _cameraRoot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}
