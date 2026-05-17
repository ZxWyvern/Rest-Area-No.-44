using UnityEngine;

namespace Game.Vehicle
{
    [DisallowMultipleComponent]
    public sealed class VehicleCameraJuice : MonoBehaviour
    {
        [SerializeField] private VehicleController _vehicle;
        [SerializeField] private VehicleCameraJuiceProfile _profile;

        private static readonly AnimationCurve DefaultVibCurve = new(new Keyframe(0f, 0.2f), new Keyframe(30f, 1f));

        private Transform _cachedTransform;
        private Rigidbody _cachedRigidbody;
        private VehicleState _cachedState;

        private Vector3 _currentPos;
        private Vector3 _currentRot;
        private Vector3 _posVelocity;
        private Vector3 _rotVelocity;
        private Vector3 _prevLocalVelocity;
        private float _noiseTime;
        private float _vibSeed;
        private bool _hasPrevVelocity;

        private void Awake()
        {
            _cachedTransform = transform;
            _vibSeed = Random.Range(0f, 100f);

            if (_vehicle == null)
                _vehicle = GetComponentInParent<VehicleController>();

            if (_vehicle != null)
            {
                _cachedRigidbody = _vehicle.Rigidbody;
                _cachedState = _vehicle.State;
            }
        }

        private void LateUpdate()
        {
            if (_profile == null) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            _noiseTime += dt * _profile.vibFrequencyScale;

            if (_cachedRigidbody == null || _cachedState == null || !_cachedState.IsOccupied)
            {
                RecoverToDefault(dt);
                return;
            }

            Vector3 localVelocity = _cachedTransform.InverseTransformDirection(_cachedRigidbody.linearVelocity);

            if (!_hasPrevVelocity)
            {
                _prevLocalVelocity = localVelocity;
                _hasPrevVelocity = true;
            }

            Vector3 localAccel = (localVelocity - _prevLocalVelocity) / dt;
            localAccel = Vector3.ClampMagnitude(localAccel, 50f);
            _prevLocalVelocity = localVelocity;

            float forwardSpeed = localVelocity.z;
            float lateralSpeed = localVelocity.x;
            float speedAbs = Mathf.Abs(forwardSpeed);
            float yawRate = _cachedRigidbody.angularVelocity.y;
            float forwardAccel = localAccel.z;

            Vector3 targetPos = ComputePositionOffset(forwardSpeed, lateralSpeed, speedAbs);
            Vector3 targetRot = ComputeRotationOffset(yawRate, speedAbs, forwardAccel);

            _currentPos = SmoothDampVector(_currentPos, targetPos, ref _posVelocity, _profile.positionSmoothTime, dt);
            _currentRot = SmoothDampVector(_currentRot, targetRot, ref _rotVelocity, _profile.rotationSmoothTime, dt);

            _cachedTransform.localPosition = _currentPos;
            _cachedTransform.localRotation = Quaternion.Euler(_currentRot);
        }

        private Vector3 ComputePositionOffset(float forwardSpeed, float lateralSpeed, float speedAbs)
        {
            Vector3 offset = Vector3.zero;

            offset.z = Mathf.Clamp(-forwardSpeed * _profile.accelPositionIntensity, -_profile.accelMaxOffset, _profile.accelMaxOffset);

            float steerFactor = Mathf.InverseLerp(0f, 10f, speedAbs);
            offset.x = Mathf.Clamp(-lateralSpeed * _profile.steerPositionIntensity * steerFactor, -_profile.steerMaxOffset, _profile.steerMaxOffset);

            float vibIntensity = VibCurve.Evaluate(speedAbs);
            float vibPos = vibIntensity * _profile.vibPositionAmplitude;
            if (vibPos > 0.0001f)
            {
                offset.x += ComputeNoise(_noiseTime, _vibSeed, 0) * vibPos;
                offset.y += ComputeNoise(_noiseTime, _vibSeed, 1) * vibPos * 0.5f;
                offset.z += ComputeNoise(_noiseTime, _vibSeed, 2) * vibPos * 0.3f;
            }

            return offset;
        }

        private Vector3 ComputeRotationOffset(float yawRate, float speedAbs, float forwardAccel)
        {
            Vector3 offset = Vector3.zero;

            offset.x = Mathf.Clamp(-forwardAccel * _profile.accelRotationIntensity, -_profile.accelMaxAngle, _profile.accelMaxAngle);

            float steerFactor = Mathf.InverseLerp(0f, 10f, speedAbs);
            float yawDeg = yawRate * Mathf.Rad2Deg;

            offset.z = Mathf.Clamp(-yawDeg * _profile.steerRollIntensity * steerFactor, -_profile.steerMaxAngle, _profile.steerMaxAngle);
            offset.y = Mathf.Clamp(yawDeg * _profile.steerYawIntensity * steerFactor, -_profile.steerMaxAngle, _profile.steerMaxAngle);

            float vibIntensity = VibCurve.Evaluate(speedAbs);
            float vibRot = vibIntensity * _profile.vibRotationAmplitude;
            if (vibRot > 0.01f)
            {
                offset.x += ComputeNoise(_noiseTime, _vibSeed, 3) * vibRot;
                offset.y += ComputeNoise(_noiseTime, _vibSeed, 4) * vibRot * 0.5f;
                offset.z += ComputeNoise(_noiseTime, _vibSeed, 5) * vibRot * 0.3f;
            }

            return offset;
        }

        private void RecoverToDefault(float dt)
        {
            _currentPos = SmoothDampVector(_currentPos, Vector3.zero, ref _posVelocity, _profile != null ? _profile.positionSmoothTime : 0.12f, dt);
            _currentRot = SmoothDampVector(_currentRot, Vector3.zero, ref _rotVelocity, _profile != null ? _profile.rotationSmoothTime : 0.1f, dt);

            _cachedTransform.localPosition = _currentPos;
            _cachedTransform.localRotation = Quaternion.Euler(_currentRot);

            _hasPrevVelocity = false;
            _noiseTime = 0f;
        }

        private static Vector3 SmoothDampVector(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float dt)
        {
            Vector3 result;
            result.x = Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime, float.PositiveInfinity, dt);
            result.y = Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime, float.PositiveInfinity, dt);
            result.z = Mathf.SmoothDamp(current.z, target.z, ref velocity.z, smoothTime, float.PositiveInfinity, dt);
            return result;
        }

        private static float ComputeNoise(float time, float seed, int channel)
        {
            float t = time + seed + channel * 7.31f;
            float n = 0f;
            n += Mathf.Sin(t * 2.71f) * 0.5f;
            n += Mathf.Sin(t * 5.13f + 1.3f) * 0.25f;
            n += Mathf.Sin(t * 11.47f + 2.7f) * 0.125f;
            n += Mathf.Sin(t * 23.09f + 4.1f) * 0.0625f;
            return n;
        }

        private AnimationCurve VibCurve
        {
            get { return _profile != null && _profile.vibrationSpeedCurve != null ? _profile.vibrationSpeedCurve : DefaultVibCurve; }
        }
    }
}
