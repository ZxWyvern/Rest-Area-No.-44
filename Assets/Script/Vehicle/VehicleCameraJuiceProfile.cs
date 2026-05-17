using UnityEngine;

namespace Game.Vehicle
{
    [CreateAssetMenu(menuName = "Game/Vehicle/Camera Juice Profile")]
    public sealed class VehicleCameraJuiceProfile : ScriptableObject
    {
        [Header("Acceleration Lean")]
        public float accelPositionIntensity = 0.12f;
        public float accelRotationIntensity = 2f;
        public float accelMaxOffset = 0.3f;
        public float accelMaxAngle = 5f;

        [Header("Steering Lean")]
        public float steerRollIntensity = 3f;
        public float steerYawIntensity = 2f;
        public float steerPositionIntensity = 0.08f;
        public float steerMaxAngle = 10f;
        public float steerMaxOffset = 0.2f;

        [Header("Engine Vibration")]
        public AnimationCurve vibrationSpeedCurve = new(new Keyframe(0f, 0.2f), new Keyframe(30f, 1f));
        public float vibPositionAmplitude = 0.003f;
        public float vibRotationAmplitude = 0.5f;
        public float vibFrequencyScale = 1f;

        [Header("Smoothing")]
        public float positionSmoothTime = 0.12f;
        public float rotationSmoothTime = 0.1f;
    }
}
