using Game;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerCameraSystem
    {
        private readonly Transform            _cameraRoot;
        private readonly Camera              _camera;
        private readonly IInputReader         _input;
        private readonly PlayerCameraConfig   _config;
        private readonly PlayerMovementSystem _movement;
        private readonly PlayerMovementConfig _movementConfig;

        private float   _verticalAngle;
        private float   _bobTimer;
        private Vector3 _cameraLocalPos;
        private Vector3 _bobOffset;
        private float   _currentCrouchOffset;
        private Vector3 _bobReturnVelocity;

        public PlayerCameraSystem(
            Transform            cameraRoot,
            Camera               camera,
            IInputReader         input,
            PlayerCameraConfig   config,
            PlayerMovementSystem movement,
            PlayerMovementConfig movementConfig)
        {
            _cameraRoot = cameraRoot;
            _camera     = camera;
            _input      = input;
            _config     = config;
            _movement   = movement;
            _movementConfig = movementConfig;

            _cameraLocalPos = camera.transform.localPosition;
        }

        public void Tick(float dt)
        {
            HandleLook();
            if (_config.enableHeadBob)
                HandleHeadBob(dt);
            HandleCrouchCameraOffset(dt);
            ApplyCameraPosition();
        }

        private void HandleLook()
        {
            Vector2 look = _input.LookInput;
            if (look.sqrMagnitude < 0.001f) return;

            float yaw = look.x * _config.mouseSensitivity;
            _cameraRoot.parent.Rotate(Vector3.up, yaw, Space.World);

            _verticalAngle -= look.y * _config.mouseSensitivity;
            _verticalAngle = Mathf.Clamp(_verticalAngle,
                                         -_config.verticalClampDown,
                                          _config.verticalClampUp);

            _cameraRoot.localRotation = Quaternion.Euler(_verticalAngle, 0f, 0f);
        }

        private void HandleHeadBob(float dt)
        {
            if (_movement.IsMoving && _movement.IsGrounded)
            {
                float maxSpeed = _movement.IsCrouching ? _movementConfig.crouchSpeed : _movementConfig.walkSpeed;
                float speedRatio = _movement.CurrentSpeed / maxSpeed;
                _bobTimer += dt * _config.bobFrequency * speedRatio;

                float bobY = (Mathf.PerlinNoise(_bobTimer, 0f) - 0.5f) * 2f * _config.bobAmplitudeY;
                float bobX = (Mathf.PerlinNoise(0f, _bobTimer) - 0.5f) * 2f * _config.bobAmplitudeX;

                _bobOffset = new Vector3(bobX, bobY, 0f);
            }
            else
            {
                _bobOffset = Vector3.SmoothDamp(
                    _bobOffset,
                    Vector3.zero,
                    ref _bobReturnVelocity,
                    1f / _config.bobSmoothReturn
                );

                if (_bobOffset.sqrMagnitude < 0.0001f)
                    _bobTimer = 0f;
            }
        }

        private void HandleCrouchCameraOffset(float dt)
        {
            float targetOffset = _movement.IsCrouching ? _config.crouchCameraOffset : 0f;
            _currentCrouchOffset = Mathf.Lerp(_currentCrouchOffset, targetOffset,
                                              _config.crouchCameraLerpSpeed * dt);
        }

        private void ApplyCameraPosition()
        {
            Vector3 targetPos = _cameraLocalPos;
            targetPos.y      += _currentCrouchOffset;
            targetPos        += _bobOffset;

            _camera.transform.localPosition = targetPos;
        }
    }
}
