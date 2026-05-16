using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerCameraSystem
    {
        private readonly Transform _playerTransform;
        private readonly Transform _cameraRoot;
        private readonly IInputReader _inputReader;
        private readonly PlayerCameraConfig _config;

        private float _pitch;

        public PlayerCameraSystem(
            Transform playerTransform,
            Transform cameraRoot,
            IInputReader inputReader,
            PlayerCameraConfig config)
        {
            _playerTransform = playerTransform;
            _cameraRoot = cameraRoot;
            _inputReader = inputReader;
            _config = config;
        }

        public void Tick()
        {
            Vector2 look = _inputReader.LookInput;
            float yawDelta = look.x * _config.mouseSensitivity;
            float pitchDelta = look.y * _config.mouseSensitivity;

            _playerTransform.Rotate(0f, yawDelta, 0f, Space.Self);

            _pitch -= pitchDelta;
            _pitch = Mathf.Clamp(_pitch, -_config.verticalClampDown, _config.verticalClampUp);
            _cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
