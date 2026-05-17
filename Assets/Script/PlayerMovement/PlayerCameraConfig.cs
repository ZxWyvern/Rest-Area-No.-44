using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player/Camera Config")]
    public sealed class PlayerCameraConfig : ScriptableObject
    {
        [Header("Sensitivity")]
        public float mouseSensitivity = 0.15f;

        [Header("Clamp")]
        [Range(0f, 90f)]
        public float verticalClampUp   = 80f;
        [Range(0f, 90f)]
        public float verticalClampDown = 80f;

        [Header("Head Bob")]
        public bool enableHeadBob = true;
        [Min(0.1f)] public float bobFrequency = 2f;
        [Min(0f)] public float bobAmplitudeY = 0.04f;
        [Min(0f)] public float bobAmplitudeX = 0.02f;
        [Min(0.1f)] public float bobSmoothReturn = 6f;

        [Header("Crouch")]
        public float crouchCameraOffset = -0.4f;
        [Min(0.1f)] public float crouchCameraLerpSpeed = 8f;
    }
}
