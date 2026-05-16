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
    }
}
