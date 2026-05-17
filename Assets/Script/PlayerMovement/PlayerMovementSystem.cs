using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerMovementSystem
    {
        private readonly CharacterController _characterController;
        private readonly Transform _playerTransform;
        private readonly IInputReader _inputReader;
        private readonly PlayerMovementConfig _config;

        private float _verticalVelocity;
        private Vector3 _smoothedHorizontalVelocity;
        private Vector3 _velocitySmoothRef;

        public PlayerMovementSystem(
            CharacterController characterController,
            Transform playerTransform,
            IInputReader inputReader,
            PlayerMovementConfig config)
        {
            _characterController = characterController;
            _playerTransform = playerTransform;
            _inputReader = inputReader;
            _config = config;

            _characterController.slopeLimit = _config.slopeLimit;
            _characterController.stepOffset = _config.stepOffset;
            _characterController.height = _config.standHeight;
            _characterController.center = new Vector3(0f, _config.standHeight * 0.5f, 0f);
        }

        public void Tick(float deltaTime)
        {
            if (!_characterController.enabled) return;

            Vector2 moveInput = _inputReader.MoveInput;
            Vector3 wishDirection = (_playerTransform.right * moveInput.x) + (_playerTransform.forward * moveInput.y);
            wishDirection.y = 0f;

            if (wishDirection.sqrMagnitude > 1f)
            {
                wishDirection.Normalize();
            }

            float targetSpeed = _config.walkSpeed;
            Vector3 targetHorizontal = wishDirection * targetSpeed;
            float smoothTime = wishDirection.sqrMagnitude > 0.001f
                ? _config.accelerationTime
                : _config.decelerationTime;
            _smoothedHorizontalVelocity = Vector3.SmoothDamp(
                _smoothedHorizontalVelocity,
                targetHorizontal,
                ref _velocitySmoothRef,
                smoothTime,
                float.MaxValue,
                deltaTime);

            bool grounded = _characterController.isGrounded;
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity -= _config.gravityMagnitude * deltaTime;
                if (_verticalVelocity < -_config.maxFallSpeed)
                {
                    _verticalVelocity = -_config.maxFallSpeed;
                }
            }

            _smoothedHorizontalVelocity.y = _verticalVelocity;
            _characterController.Move(_smoothedHorizontalVelocity * deltaTime);
        }
    }
}
