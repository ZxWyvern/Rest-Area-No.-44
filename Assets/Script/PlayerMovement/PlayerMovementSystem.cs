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
            Vector2 moveInput = _inputReader.MoveInput;
            Vector3 wishDirection = (_playerTransform.right * moveInput.x) + (_playerTransform.forward * moveInput.y);

            if (wishDirection.sqrMagnitude > 1f)
            {
                wishDirection.Normalize();
            }

            float targetSpeed = _config.walkSpeed;
            Vector3 horizontalVelocity = wishDirection * targetSpeed;

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

            horizontalVelocity.y = _verticalVelocity;
            _characterController.Move(horizontalVelocity * deltaTime);
        }
    }
}
