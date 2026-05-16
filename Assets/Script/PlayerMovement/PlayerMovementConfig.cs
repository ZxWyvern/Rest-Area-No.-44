using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player/Movement Config")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Walk")]
        public float walkSpeed = 3.5f;

        [Header("Slope & Step")]
        public float slopeLimit = 45f;
        public float stepOffset = 0.3f;

        [Header("Character Height")]
        public float standHeight = 1.8f;

        [Header("Gravity")]
        public float gravityMagnitude = 9.81f;
        public float maxFallSpeed = 20f;
    }
}
