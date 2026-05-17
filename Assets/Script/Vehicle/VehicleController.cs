using Game.Player;
using UnityEngine;
using VContainer;

namespace Game.Vehicle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private VehicleConfig _config;

        [Header("Seats & Exits")]
        [SerializeField] private Transform _driverSeat;
        [SerializeField] private Transform _exitPoint;

        [Header("Cameras")]
        [SerializeField] private Camera _vehicleCamera;

        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider[] _frontWheels;
        [SerializeField] private WheelCollider[] _rearWheels;

        [Header("Wheel Visuals")]
        [SerializeField] private Transform[] _frontWheelVisuals;
        [SerializeField] private Transform[] _rearWheelVisuals;

        [Header("Exit Velocity Transfer")]
        [SerializeField] [Range(0f, 1f)] private float _exitVelocityRetention = 0.3f;

        private static readonly Collider[] _exitOverlapBuffer = new Collider[16];
        private IInputReader _input;
        private PlayerController _playerController;
        private Rigidbody _rigidbody;

        private VehicleState _state;
        private VehicleDriveSystem _driveSystem;

        public VehicleState State => _state;
        public Rigidbody Rigidbody => _rigidbody;

        [Inject]
        public void Construct(
            IInputReader input,
            PlayerController playerController)
        {
            _input = input;
            _playerController = playerController;
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("[VehicleController] VehicleConfig not assigned.", this);
                enabled = false;
                return;
            }

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.mass = _config.mass;
            _rigidbody.constraints = RigidbodyConstraints.None;
            _rigidbody.linearDamping = 0.05f;
            _rigidbody.angularDamping = 0.05f;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.centerOfMass = new Vector3(0f, -0.5f, 0f);

            if (_vehicleCamera != null)
                _vehicleCamera.gameObject.SetActive(false);
        }

        private void Start()
        {
            Bootstrap();
        }

        private void Bootstrap()
        {
            if (_state != null) return;
            if (!ValidateReferences()) return;

            _state = new VehicleState();
            _driveSystem = new VehicleDriveSystem(_config, _state, _input, _rearWheels, _frontWheels);
            _input.ExitVehiclePerformed += OnExitRequested;
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.ExitVehiclePerformed -= OnExitRequested;
        }

        private void FixedUpdate()
        {
            if (_driveSystem == null) return;

            _driveSystem.Tick(Time.fixedDeltaTime, _rigidbody);
        }

        private void Update()
        {
            UpdateWheelVisuals();
        }

        private void UpdateWheelVisuals()
        {
            for (int i = 0; i < _frontWheels.Length && i < _frontWheelVisuals.Length; i++)
                UpdateSingleWheel(_frontWheels[i], _frontWheelVisuals[i]);
            for (int i = 0; i < _rearWheels.Length && i < _rearWheelVisuals.Length; i++)
                UpdateSingleWheel(_rearWheels[i], _rearWheelVisuals[i]);
        }

        private static void UpdateSingleWheel(WheelCollider collider, Transform visual)
        {
            if (collider == null || visual == null) return;
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            visual.SetPositionAndRotation(pos, rot);
        }

        public bool TryEnterVehicle()
        {
            if (_state == null || _config == null)
            {
                Debug.LogError("[VehicleController] Not initialized.", this);
                return false;
            }

            if (_state.IsOccupied) return false;

            _state.Occupied = OccupiedState.Entering;

            _rigidbody.isKinematic = false;

            _input.EnableVehicleMap();

            _playerController.CharacterController.enabled = false;

            Transform playerTransform = _playerController.transform;
            playerTransform.SetPositionAndRotation(_driverSeat.position, _driverSeat.rotation);
            playerTransform.SetParent(_driverSeat);

            Physics.SyncTransforms();

            _playerController.PlayerCamera.gameObject.SetActive(false);
            if (_vehicleCamera != null)
                _vehicleCamera.gameObject.SetActive(true);

            _state.Occupied = OccupiedState.Occupied;
            return true;
        }

        public void TryExitVehicle()
        {
            if (!_state.IsOccupied) return;

            _state.Occupied = OccupiedState.Exiting;
            Vector3 exitPos = FindSafeExitPosition();
            if (exitPos == Vector3.zero)
            {
                _state.Occupied = OccupiedState.Occupied;
                Debug.LogWarning("[VehicleController] Cannot exit — no safe position found.", this);
                return;
            }

            Vector3 vehicleVelocity = _rigidbody.linearVelocity;

            Transform playerTransform = _playerController.transform;
            playerTransform.SetParent(null);
            playerTransform.SetPositionAndRotation(exitPos, _exitPoint != null ? _exitPoint.rotation : transform.rotation);

            _playerController.CharacterController.enabled = true;

            Physics.SyncTransforms();

            if (vehicleVelocity.magnitude > 0.5f)
            {
                Vector3 transfer = vehicleVelocity * _exitVelocityRetention;
                _playerController.CharacterController.Move(transfer);
            }

            if (_vehicleCamera != null)
                _vehicleCamera.gameObject.SetActive(false);
            _playerController.PlayerCamera.gameObject.SetActive(true);

            _input.EnablePlayerMap();

            _state.Occupied = OccupiedState.Empty;
        }

        private Vector3 FindSafeExitPosition()
        {
            if (_exitPoint == null)
                return transform.position + transform.right * 2f + Vector3.up;

            Vector3 basePos = _exitPoint.position;

            if (IsPositionClear(basePos))
                return basePos;

            float radius = _config.exitClearanceRadius * 2f;
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0.5f, Mathf.Sin(angle)) * radius;
                Vector3 candidate = basePos + offset;
                if (IsPositionClear(candidate))
                    return candidate;
            }

            return Vector3.zero;
        }

        private bool IsPositionClear(Vector3 position)
        {
            int count = Physics.OverlapSphereNonAlloc(position, _config.exitClearanceRadius, _exitOverlapBuffer, _config.exitObstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (!_exitOverlapBuffer[i].transform.IsChildOf(transform))
                    return false;
            }
            return true;
        }

        private void OnExitRequested()
        {
            TryExitVehicle();
        }

        private bool ValidateReferences()
        {
            if (_driverSeat == null)
            {
                Debug.LogError("[VehicleController] DriverSeat not assigned.", this);
                return false;
            }
            if (_exitPoint == null)
                Debug.LogWarning("[VehicleController] ExitPoint not assigned — using fallback.", this);
            if (_vehicleCamera == null)
                Debug.LogWarning("[VehicleController] VehicleCamera not assigned.", this);
            if (_input == null)
            {
                Debug.LogError("[VehicleController] IInputReader not injected.", this);
                return false;
            }
            if (_playerController == null)
            {
                Debug.LogError("[VehicleController] PlayerController not injected.", this);
                return false;
            }
            if (_frontWheels == null || _frontWheels.Length == 0)
            {
                Debug.LogError("[VehicleController] FrontWheels not assigned.", this);
                return false;
            }
            if (_rearWheels == null || _rearWheels.Length == 0)
            {
                Debug.LogError("[VehicleController] RearWheels not assigned.", this);
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_config == null || _exitPoint == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_exitPoint.position, _config.exitClearanceRadius);
        }
#endif
    }
}
