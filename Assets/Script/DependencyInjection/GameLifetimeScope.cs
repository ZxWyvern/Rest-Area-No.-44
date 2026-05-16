using Game.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.DependencyInjection
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private PlayerMovementConfig _movementConfig;
        [SerializeField] private PlayerCameraConfig _cameraConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerController);

            builder.RegisterInstance<IInputReader>(_inputReader);
            builder.RegisterInstance(_movementConfig);
            builder.RegisterInstance(_cameraConfig);

            builder.Register<PlayerMovementSystem>(Lifetime.Singleton)
                .WithParameter(_playerController.CharacterController)
                .WithParameter(_playerController.transform);

            builder.Register<PlayerCameraSystem>(Lifetime.Singleton)
                .WithParameter(_playerController.transform)
                .WithParameter(_playerController.CameraRoot);
        }
    }
}
