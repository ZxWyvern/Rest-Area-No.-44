using UnityEngine;
using UnityEngine.Events;

namespace Game.LoopTrigger
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerTriggerVolumeEvent : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private bool _triggerOnce = true;
        [SerializeField] private UnityEvent _onPlayerEntered;

        private bool _hasTriggered;

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered && _triggerOnce)
            {
                return;
            }

            if (!other.CompareTag(_playerTag))
            {
                return;
            }

            _hasTriggered = true;
            _onPlayerEntered?.Invoke();
        }
    }
}
