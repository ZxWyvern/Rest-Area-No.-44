using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Raycast dari camera ke depan untuk detect IInteractable.
    /// Trigger highlight on hover dan OnInteract saat tombol ditekan.
    /// </summary>
    public sealed class InteractionSystem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Ray Settings")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _interactRange = 2.5f;
        [SerializeField] private LayerMask _interactMask = ~0; 

        [Header("Input")]
        [SerializeField] private Game.Player.PlayerInputReader _inputReader;

        // ── State ─────────────────────────────────────────────────────────────────
        private IInteractable _currentTarget;
        private InteractableHighlight _currentHighlight;

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            if (_inputReader != null)
                _inputReader.InteractPerformed += OnInteractInput;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
                _inputReader.InteractPerformed -= OnInteractInput;

            ClearTarget();
        }

        private void Update()
        {
            DetectInteractable();
        }

        // ── Detection ─────────────────────────────────────────────────────────────
        private void DetectInteractable()
        {
            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * _interactRange, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactMask, QueryTriggerInteraction.Ignore))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null && interactable.CanInteract)
                {
                    // KUNCI COCOK: Jika masih memandang objek interaktif yang sama, langsung batalkan eksekusi lanjutan.
                    // Layer objek tidak diganggu, Raycast tidak akan mengalami disorientasi fisis.
                    if (interactable == _currentTarget)
                    {
                        return;
                    }

                    ClearTarget();
                    SetTarget(interactable, hit.collider);
                    return;
                }
            }

            // Lepas kunci target jika pandangan mata keluar dari seluruh collider objek
            if (_currentTarget != null)
            {
                ClearTarget();
            }
        }

        // ── Target Management ─────────────────────────────────────────────────────
        private void SetTarget(IInteractable interactable, Collider col)
        {
            _currentTarget = interactable;
            _currentTarget.OnHoverEnter();

            _currentHighlight = col.GetComponentInParent<InteractableHighlight>();
            _currentHighlight?.SetHighlight(true);
        }

        private void ClearTarget()
        {
            if (_currentTarget == null) return;

            _currentTarget.OnHoverExit();
            _currentHighlight?.SetHighlight(false);

            _currentTarget = null;
            _currentHighlight = null;
        }

        // ── Interact Input ────────────────────────────────────────────────────────
        private void OnInteractInput()
        {
            if (_currentTarget == null || !_currentTarget.CanInteract) return;
            _currentTarget.OnInteract();
        }

        // ── Gizmo ─────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_playerCamera == null) return;

            Gizmos.color = _currentTarget != null ? Color.green : Color.yellow;
            Gizmos.DrawRay(_playerCamera.transform.position, _playerCamera.transform.forward * _interactRange);
        }
#endif
    }
}