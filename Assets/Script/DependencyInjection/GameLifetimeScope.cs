using Game.Player;
using Game.Vehicle;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.DependencyInjection
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Player")]
        [SerializeField] private PlayerController    _playerController;
        [SerializeField] private PlayerInputReader   _inputReader;
        [SerializeField] private PlayerMovementConfig _movementConfig;
        [SerializeField] private PlayerCameraConfig   _cameraConfig;

        [Header("Vehicles")]
        [SerializeField] private VehicleController[] _vehicleControllers;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_playerController == null || _inputReader == null ||
                _movementConfig == null || _cameraConfig == null)
            {
                Debug.LogError("[GameLifetimeScope] Missing required player references.", this);
                return;
            }

            if (_playerController.CharacterController == null || _playerController.CameraRoot == null)
            {
                Debug.LogError("[GameLifetimeScope] PlayerController sub-references missing.", _playerController);
                return;
            }

            builder.RegisterComponent(_playerController);
            builder.RegisterInstance<IInputReader>(_inputReader);
            builder.RegisterInstance(_movementConfig);
            builder.RegisterInstance(_cameraConfig);

            builder.Register<PlayerMovementSystem>(Lifetime.Singleton)
                .WithParameter("characterController", _playerController.CharacterController)
                .WithParameter("playerTransform", _playerController.transform);

            builder.Register<PlayerCameraSystem>(Lifetime.Singleton)
                .WithParameter("playerTransform", _playerController.transform)
                .WithParameter("cameraRoot", _playerController.CameraRoot);

            if (_vehicleControllers != null)
            {
                foreach (var vc in _vehicleControllers)
                {
                    if (vc == null) continue;
                    builder.RegisterComponent(vc);
                }
            }
        }
    }
}
