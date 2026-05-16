using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.Player;

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
            builder.RegisterInstance<IInputReader>(_inputReader);
            builder.RegisterInstance(_movementConfig);
            builder.RegisterInstance(_cameraConfig);

            builder.Register<PlayerMovementSystem>(Lifetime.Singleton)
                .WithParameter(typeof(CharacterController), _playerController.CharacterController)
                .WithParameter(typeof(Transform), _playerController.transform)
                .AsSelf();

            builder.Register<PlayerCameraSystem>(Lifetime.Singleton)
                .WithParameter(typeof(Transform), _playerController.CameraRoot)
                .WithParameter(typeof(Camera), _playerController.PlayerCamera)
                .AsSelf();

            builder.RegisterComponent(_playerController);
        }
    }
}
