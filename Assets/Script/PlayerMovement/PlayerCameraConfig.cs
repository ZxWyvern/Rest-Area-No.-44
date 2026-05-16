using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player/Camera Config")]
    public sealed class PlayerCameraConfig : ScriptableObject
    {
        [Header("Sensitivity")]
        public float mouseSensitivity    = 0.15f;
        public float controllerSensitivity = 120f;

        [Header("Clamp")]
        [Tooltip("Max look-up angle (degrees)")]
        [Range(0f, 90f)]
        public float verticalClampUp     = 80f;

        [Tooltip("Max look-down angle (degrees)")]
        [Range(0f, 90f)]
        public float verticalClampDown   = 80f;

        [Header("Head Bob")]
        public bool  enableHeadBob       = true;
        public float bobFrequency        = 2f;
        public float bobAmplitudeY       = 0.04f;
        public float bobAmplitudeX       = 0.02f;
        [Tooltip("SmoothDamp speed untuk return ke center saat idle")]
        public float bobSmoothReturn     = 6f;

        [Header("Crouch Camera")]
        [Tooltip("Offset Y kamera saat crouch")]
        public float crouchCameraOffset  = -0.4f;
        [Tooltip("Lerp speed untuk crouch camera offset")]
        public float crouchCameraLerpSpeed = 8f;
    }
}
