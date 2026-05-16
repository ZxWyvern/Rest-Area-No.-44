using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Semua tunable parameter movement dikumpulkan di sini.
    /// ScriptableObject = data-driven, bisa di-swap per scene/level tanpa ubah code.
    /// JANGAN mutate field ini saat runtime — ini config, bukan state.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Player/Movement Config")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Walk / Run")]
        [Tooltip("Kecepatan jalan normal (m/s)")]
        public float walkSpeed        = 3.5f;

        [Tooltip("Kecepatan sprint (m/s)")]
        public float sprintSpeed      = 6f;

        [Tooltip("Kecepatan saat crouch (m/s)")]
        public float crouchSpeed      = 1.8f;

        [Tooltip("Seberapa cepat velocity horizontal mencapai target. Lebih tinggi = lebih responsif tapi lebih arcade.")]
        [Range(1f, 30f)]
        public float groundAcceleration  = 12f;

        [Tooltip("Deselerasi saat tidak ada input (ground friction emulation)")]
        [Range(1f, 30f)]
        public float groundDeceleration  = 10f;

        [Tooltip("Kontrol di udara — biasanya lebih rendah dari ground untuk feel yang baik")]
        [Range(0f, 15f)]
        public float airAcceleration     = 4f;

        [Header("Jump")]
        [Tooltip("Tinggi lompatan dalam meter — dikonversi ke velocity via v=sqrt(2gh)")]
        public float jumpHeight          = 1.2f;

        [Tooltip("Coyote time window dalam detik")]
        [Range(0f, 0.3f)]
        public float coyoteTime          = 0.15f;

        [Tooltip("Jump buffer window dalam detik — input jump sebelum landing tetap di-register")]
        [Range(0f, 0.3f)]
        public float jumpBufferTime      = 0.15f;

        [Tooltip("Multiplier gravity saat player melepas jump early (variable jump height)")]
        [Range(1f, 5f)]
        public float jumpCutGravityMultiplier = 2f;

        [Tooltip("Multiplier gravity saat falling — meningkatkan game feel tanpa ubah jump height")]
        [Range(1f, 5f)]
        public float fallGravityMultiplier    = 2.5f;

        [Tooltip("Max fall speed untuk mencegah tunneling")]
        public float maxFallSpeed        = 20f;

        [Header("Crouch")]
        [Tooltip("Tinggi CharacterController saat crouch")]
        public float crouchHeight        = 1f;

        [Tooltip("Tinggi normal CharacterController")]
        public float standHeight         = 1.8f;

        [Tooltip("Kecepatan transisi crouch (lerp speed)")]
        [Range(1f, 20f)]
        public float crouchTransitionSpeed = 10f;

        [Header("Slope & Step")]
        public float slopeLimit          = 45f;
        public float stepOffset          = 0.3f;

        [Header("Gravity")]
        [Tooltip("Custom gravity magnitude. Default Physics.gravity.y jika <= 0")]
        public float gravityMagnitude    = 9.81f;

        // ── Computed Properties ───────────────────────────────────────────────────
        /// <summary>Velocity Y yang dibutuhkan untuk mencapai jumpHeight.</summary>
        public float JumpVelocity => Mathf.Sqrt(2f * gravityMagnitude * jumpHeight);
    }
}
