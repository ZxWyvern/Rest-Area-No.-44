using UnityEngine;
using VContainer;

namespace Game.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private Transform _cameraRoot;
        [SerializeField] private Camera _playerCamera;

        private CharacterController _characterController;
        private PlayerMovementSystem _movementSystem;
        private PlayerCameraSystem _cameraSystem;

        public CharacterController CharacterController => _characterController;
        public Transform CameraRoot => _cameraRoot;
        public Camera PlayerCamera => _playerCamera;

        [Inject]
        public void Construct(PlayerMovementSystem movementSystem, PlayerCameraSystem cameraSystem)
        {
            _movementSystem = movementSystem;
            _cameraSystem = cameraSystem;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_inputReader == null || _cameraRoot == null || _playerCamera == null)
            {
                Debug.LogError("[PlayerController] Missing serialized references.", this);
                enabled = false;
                return;
            }

            _inputReader.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            _movementSystem?.Tick(Time.deltaTime);
            _cameraSystem?.Tick();
        }

        private void OnDisable()
        {
            _inputReader?.Disable();
        }
    }
}
