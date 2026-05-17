using System;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerCameraSystem
    {
        private readonly Transform _playerTransform;
        private readonly Transform _cameraRoot;
        private readonly IInputReader _inputReader;
        private readonly PlayerCameraConfig _config;
        private readonly Func<bool> _isGrounded;
        private readonly Func<bool> _isMoving;

        private float _pitch;
        private float _bobTimer;
        private float _targetCameraY;
        private Vector3 _bobOffset;
        private Vector3 _bobSmoothVelocity;

        public PlayerCameraSystem(
            Transform playerTransform,
            Transform cameraRoot,
            IInputReader inputReader,
            PlayerCameraConfig config,
            Func<bool> isGrounded,
            Func<bool> isMoving)
        {
            _playerTransform = playerTransform;
            _cameraRoot = cameraRoot;
            _inputReader = inputReader;
            _config = config;
            _isGrounded = isGrounded;
            _isMoving = isMoving;
        }

        public void Tick()
        {
            float deltaTime = Time.deltaTime;
            Vector2 look = _inputReader.LookInput;
            float yawDelta = look.x * _config.mouseSensitivity;
            float pitchDelta = look.y * _config.mouseSensitivity;

            _playerTransform.Rotate(0f, yawDelta, 0f, Space.Self);

            _pitch -= pitchDelta;
            _pitch = Mathf.Clamp(_pitch, -_config.verticalClampDown, _config.verticalClampUp);
            _cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            bool crouching = _inputReader.IsCrouching;
            _targetCameraY = crouching ? _config.crouchCameraOffset : 0f;

            Vector3 bobTarget = Vector3.zero;
            if (_config.enableHeadBob && _isGrounded() && _isMoving())
            {
                _bobTimer += deltaTime * _config.bobFrequency * Mathf.PI * 2f;
                bobTarget.y = Mathf.Sin(_bobTimer) * _config.bobAmplitudeY;
                bobTarget.x = Mathf.Sin(_bobTimer * 0.5f) * _config.bobAmplitudeX;
            }
            else
            {
                _bobTimer = 0f;
            }

            float smoothTime = 1f / Mathf.Max(_config.bobSmoothReturn, 0.01f);
            _bobOffset = Vector3.SmoothDamp(
                _bobOffset, bobTarget,
                ref _bobSmoothVelocity, smoothTime,
                float.MaxValue, deltaTime);

            float currentY = Mathf.Lerp(
                _cameraRoot.localPosition.y,
                _targetCameraY,
                _config.crouchCameraLerpSpeed * deltaTime);

            _cameraRoot.localPosition = new Vector3(
                _bobOffset.x,
                currentY + _bobOffset.y,
                0f);
        }
    }
}
