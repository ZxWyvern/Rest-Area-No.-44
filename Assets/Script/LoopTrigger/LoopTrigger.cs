using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    public sealed class LoopTrigger : MonoBehaviour
    {
        [Header("Teleport")]
        [SerializeField] private Transform _destination;

        [Header("Fade")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] [Range(0.1f, 2f)] private float _fadeOutDuration = 0.5f;
        [SerializeField] [Range(0.1f, 2f)] private float _fadeInDuration  = 0.5f;
        [SerializeField] [Range(0f, 1f)]   private float _holdDuration    = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = true;

        private int  _loopCount;
        private bool _isTeleporting;

        private CharacterController _playerCC;

        private void Awake()
        {
            ValidateSetup();

            if (_fadeImage != null)
                SetFadeAlpha(0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isTeleporting) return;
            if (!other.CompareTag("Player")) return;

            if (_playerCC == null)
                _playerCC = other.GetComponent<CharacterController>();

            StartCoroutine(LoopRoutine(other.transform));
        }

        private IEnumerator LoopRoutine(Transform player)
        {
            _isTeleporting = true;

            yield return StartCoroutine(Fade(0f, 1f, _fadeOutDuration));

            if (_holdDuration > 0f)
                yield return new WaitForSeconds(_holdDuration);

            DoTeleport(player);

            _loopCount++;
            if (_showDebugLog)
                Debug.Log($"[Loop] = {_loopCount} | Destination: {_destination.position}");

            yield return null;

            yield return StartCoroutine(Fade(1f, 0f, _fadeInDuration));

            _isTeleporting = false;
        }

        private void DoTeleport(Transform player)
        {
            if (_playerCC != null)
                _playerCC.enabled = false;

            player.SetPositionAndRotation(_destination.position, _destination.rotation);

            Physics.SyncTransforms();

            if (_playerCC != null)
                _playerCC.enabled = true;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (_fadeImage == null) yield break;

            float elapsed = 0f;
            SetFadeAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                SetFadeAlpha(Mathf.SmoothStep(from, to, t));
                yield return null;
            }

            SetFadeAlpha(to);
        }

        private void SetFadeAlpha(float alpha)
        {
            Color c = _fadeImage.color;
            c.a     = alpha;
            _fadeImage.color = c;
        }

        private void ValidateSetup()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_destination == null)
                Debug.LogError("[LoopTrigger] Destination belum di-assign!", this);

            if (_fadeImage == null)
                Debug.LogWarning("[LoopTrigger] FadeImage belum di-assign — teleport tanpa fade.", this);

            var col = GetComponent<Collider>();
            if (col == null)
                Debug.LogError("[LoopTrigger] Tidak ada Collider di GameObject ini!", this);
            else if (!col.isTrigger)
                Debug.LogWarning("[LoopTrigger] Collider bukan trigger — set Is Trigger = true.", this);
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_destination == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_destination.position, 0.3f);
            Gizmos.DrawLine(transform.position, _destination.position);

            UnityEditor.Handles.Label(_destination.position + Vector3.up * 0.5f,
                $"Loop Dest\n[Loop] = {_loopCount}");
        }
#endif
    }
}
