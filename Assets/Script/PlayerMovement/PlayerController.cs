using UnityEngine;
using VContainer;

namespace Game.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private PlayerInputReader _inputReader;

        [Header("Config")]
        [SerializeField] private PlayerMovementConfig _movementConfig;
        [SerializeField] private PlayerCameraConfig   _cameraConfig;

        [Header("References")]
        [SerializeField] private Transform _cameraRoot;
        [SerializeField] private Camera _playerCamera;

        public Transform CameraRoot => _cameraRoot;
        public Camera PlayerCamera => _playerCamera;

        private CharacterController   _characterController;
        private PlayerMovementSystem  _movementSystem;
        private PlayerCameraSystem    _cameraSystem;

        public CharacterController CharacterController
        {
            get
            {
                if (_characterController == null)
                    _characterController = GetComponent<CharacterController>();
                return _characterController;
            }
        }

        private void Awake()
        {
            ValidateReferences();
            _inputReader.Enable();
            LockCursor();
        }

        [Inject]
        public void Init(PlayerMovementSystem movementSystem, PlayerCameraSystem cameraSystem)
        {
            _movementSystem = movementSystem;
            _cameraSystem = cameraSystem;
        }

        private void Update()
        {
            _movementSystem?.Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            _cameraSystem?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _movementSystem?.Dispose();
            _inputReader.Disable();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _movementSystem?.OnControllerColliderHit(hit);
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void ValidateReferences()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_inputReader    == null) Debug.LogError("[PlayerController] InputReader tidak di-assign!", this);
            if (_movementConfig == null) Debug.LogError("[PlayerController] MovementConfig tidak di-assign!", this);
            if (_cameraConfig   == null) Debug.LogError("[PlayerController] CameraConfig tidak di-assign!", this);
            if (_cameraRoot     == null) Debug.LogError("[PlayerController] CameraRoot tidak di-assign!", this);
            if (_playerCamera   == null) Debug.LogError("[PlayerController] PlayerCamera tidak di-assign!", this);
#endif
        }
    }
}
