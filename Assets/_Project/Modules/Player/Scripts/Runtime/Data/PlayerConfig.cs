using UnityEngine;

namespace Trivias.Player.Data
{
    /// <summary>Valores de movimiento y cámara del concursante. Se ajustan en el asset, no en el código.</summary>
    [CreateAssetMenu(menuName = "Trivias/Player/Player Config", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movimiento")]
        [SerializeField, Min(0f)] private float _moveSpeed = 3.2f;
        [SerializeField, Min(0f)] private float _speedChangeRate = 10f;
        [SerializeField, Range(0f, 0.3f)] private float _rotationSmoothTime = 0.12f;
        [SerializeField] private float _gravity = -15f;
        [SerializeField, Min(0f)] private float _freeFallDelay = 0.15f;

        [Header("Detección de suelo")]
        [SerializeField] private float _groundedOffset = -0.14f;
        [SerializeField, Min(0f)] private float _groundedRadius = 0.28f;

        [Header("Cámara")]
        [SerializeField, Min(0f)] private float _lookSensitivity = 0.12f;
        [SerializeField] private float _minPitch = -30f;
        [SerializeField] private float _maxPitch = 70f;

        public float MoveSpeed => _moveSpeed;
        public float SpeedChangeRate => _speedChangeRate;
        public float RotationSmoothTime => _rotationSmoothTime;
        public float Gravity => _gravity;
        public float FreeFallDelay => _freeFallDelay;
        public float GroundedOffset => _groundedOffset;
        public float GroundedRadius => _groundedRadius;
        public float LookSensitivity => _lookSensitivity;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
    }
}
