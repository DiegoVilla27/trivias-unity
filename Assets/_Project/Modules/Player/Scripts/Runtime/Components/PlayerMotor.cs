using Trivias.Player.Data;
using Trivias.Player.Logic;
using Trivias.Shared.Physics;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Movimiento en tercera persona relativo a la cámara. Sin salto ni sprint (fuera del GDD).
    /// Solo lo activa el dueño; los demás reciben la posición por red.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int MotionSpeedId = Animator.StringToHash("MotionSpeed");
        private static readonly int GroundedId = Animator.StringToHash("Grounded");
        private static readonly int FreeFallId = Animator.StringToHash("FreeFall");

        private const float INPUT_DEADZONE_SQR = 0.0001f;
        private const float SPEED_TOLERANCE = 0.1f;
        private const float GROUNDED_VERTICAL_VELOCITY = -2f;

        [SerializeField] private PlayerConfig _config;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _cameraRoot;

        private float _speed;
        private float _animationBlend;
        private float _targetYaw;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _airTime;
        private bool _grounded = true;

        private void OnEnable() => _targetYaw = transform.eulerAngles.y;

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            UpdateGrounded();
            UpdateVerticalVelocity(deltaTime);
            Move(deltaTime);
        }

        /// <summary>Coloca al personaje sin que el CharacterController lo devuelva a su posición anterior.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _controller.enabled = true;
            _targetYaw = rotation.eulerAngles.y;
        }

        private void UpdateGrounded()
        {
            var position = transform.position;
            var spherePosition = new Vector3(position.x, position.y - _config.GroundedOffset, position.z);
            _grounded = Physics.CheckSphere(spherePosition, _config.GroundedRadius, GameLayers.GROUND_MASK,
                QueryTriggerInteraction.Ignore);
            _animator.SetBool(GroundedId, _grounded);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_grounded)
            {
                _airTime = 0f;
                _animator.SetBool(FreeFallId, false);
                if (_verticalVelocity < 0f)
                    _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
                return;
            }

            _airTime += deltaTime;
            if (_airTime >= _config.FreeFallDelay)
                _animator.SetBool(FreeFallId, true);
            _verticalVelocity += _config.Gravity * deltaTime;
        }

        private void Move(float deltaTime)
        {
            var input = _input.Move;
            var hasInput = input.sqrMagnitude > INPUT_DEADZONE_SQR;
            var targetSpeed = hasInput ? _config.MoveSpeed : 0f;
            var inputMagnitude = hasInput ? Mathf.Clamp01(input.magnitude) : 1f;

            var velocity = _controller.velocity;
            var currentHorizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

            if (currentHorizontalSpeed < targetSpeed - SPEED_TOLERANCE ||
                currentHorizontalSpeed > targetSpeed + SPEED_TOLERANCE)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    deltaTime * _config.SpeedChangeRate);
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, deltaTime * _config.SpeedChangeRate);
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            if (hasInput)
            {
                _targetYaw = LookAngles.MoveYaw(input.x, input.y, _cameraRoot.eulerAngles.y);
                var yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYaw, ref _rotationVelocity,
                    _config.RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            var direction = Quaternion.Euler(0f, _targetYaw, 0f) * Vector3.forward;
            _controller.Move(direction * (_speed * deltaTime) + Vector3.up * (_verticalVelocity * deltaTime));

            _animator.SetFloat(SpeedId, _animationBlend);
            _animator.SetFloat(MotionSpeedId, inputMagnitude);
        }
    }
}
