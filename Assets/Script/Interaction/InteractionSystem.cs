using Game.Player;
using UnityEngine;
using VContainer;

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionSystem : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField, Min(0.1f)] private float _range = 2.5f;
        [SerializeField] private LayerMask _interactableMask = ~0;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("References")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private InteractionPromptPresenter _promptPresenter;

        private readonly RaycastHit[] _hits = new RaycastHit[1];
        private IInteractable _currentInteractable;
        private IHoldInteractable _currentHoldInteractable;
        private float _holdElapsed;
        private bool _holdActive;

        [Inject]
        public void Construct(PlayerController playerController)
        {
            if (_playerCamera == null)
            {
                _playerCamera = playerController.PlayerCamera;
            }
        }

        private void OnEnable()
        {
            if (_inputReader == null)
            {
                Debug.LogError("[InteractionSystem] InputReader reference is missing.", this);
                enabled = false;
                return;
            }

            _inputReader.InteractPerformed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.InteractPerformed -= OnInteractPerformed;
            }

            CancelHoldInteraction();
            SetCurrentTarget(null);
        }

        private void Update()
        {
            UpdateTarget();
            UpdateHold(Time.deltaTime);
        }

        private void UpdateTarget()
        {
            if (_playerCamera == null)
            {
                return;
            }

            Transform cameraTransform = _playerCamera.transform;
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            int hitCount = Physics.RaycastNonAlloc(ray, _hits, _range, _interactableMask, _queryTriggerInteraction);

            IInteractable next = null;
            if (hitCount > 0)
            {
                Collider hitCollider = _hits[0].collider;
                if (hitCollider != null)
                {
                    next = hitCollider.GetComponentInParent<IInteractable>();
                    if (next != null && !next.CanInteract())
                    {
                        next = null;
                    }
                }
            }

            if (!ReferenceEquals(next, _currentInteractable))
            {
                SetCurrentTarget(next);
            }
        }

        private void SetCurrentTarget(IInteractable target)
        {
            if (ReferenceEquals(_currentInteractable, target))
            {
                return;
            }

            CancelHoldInteraction();
            _currentInteractable = target;
            _currentHoldInteractable = target as IHoldInteractable;

            if (_promptPresenter == null)
            {
                return;
            }

            if (_currentInteractable == null)
            {
                _promptPresenter.Hide();
                return;
            }

            _promptPresenter.Show(_currentInteractable.GetInteractionPrompt());
        }

        private void OnInteractPerformed()
        {
            if (_currentInteractable == null || !_currentInteractable.CanInteract())
            {
                return;
            }

            if (_currentHoldInteractable != null && _currentHoldInteractable.SupportsHoldInteraction)
            {
                StartHoldInteraction();
                return;
            }

            _currentInteractable.Interact();
        }

        private void StartHoldInteraction()
        {
            if (_currentHoldInteractable == null)
            {
                return;
            }

            _holdElapsed = 0f;
            _holdActive = true;
            _currentHoldInteractable.BeginHoldInteraction();
            _currentHoldInteractable.UpdateHoldInteraction(0f);
        }

        private void UpdateHold(float deltaTime)
        {
            if (!_holdActive || _currentHoldInteractable == null)
            {
                return;
            }

            if (!_currentHoldInteractable.CanInteract())
            {
                CancelHoldInteraction();
                return;
            }

            float holdDuration = _currentHoldInteractable.HoldDurationSeconds;
            if (holdDuration <= 0f)
            {
                CompleteHoldInteraction();
                return;
            }

            _holdElapsed += deltaTime;
            float normalized = Mathf.Clamp01(_holdElapsed / holdDuration);
            _currentHoldInteractable.UpdateHoldInteraction(normalized);

            if (normalized >= 1f)
            {
                CompleteHoldInteraction();
            }
        }

        private void CompleteHoldInteraction()
        {
            if (_currentHoldInteractable == null)
            {
                return;
            }

            _holdActive = false;
            _currentHoldInteractable.CompleteHoldInteraction();
            _currentHoldInteractable.Interact();
        }

        private void CancelHoldInteraction()
        {
            if (!_holdActive || _currentHoldInteractable == null)
            {
                _holdActive = false;
                _holdElapsed = 0f;
                return;
            }

            _holdActive = false;
            _holdElapsed = 0f;
            _currentHoldInteractable.CancelHoldInteraction();
        }
    }
}
