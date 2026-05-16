using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Attach ke objek interactable.
    /// Mendaftarkan Renderer secara dinamis ke OutlineCustomPass tanpa mengubah layer fisik.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractableHighlight : MonoBehaviour
    {
        [Tooltip("Jika true, child renderers juga ikut di-highlight")]
        [SerializeField] private bool _includeChildren = true;

        private Renderer[] _renderers;
        private bool _isHighlighted;

        private void Awake()
        {
            _renderers = _includeChildren
                ? GetComponentsInChildren<Renderer>(includeInactive: false)
                : new[] { GetComponent<Renderer>() };
        }

        public void SetHighlight(bool active)
        {
            if (_isHighlighted == active) return;
            _isHighlighted = active;

            if (_renderers == null || _renderers.Length == 0) return;

            // Memanggil fungsi static pendaftaran pada OutlineCustomPass
            if (active)
            {
                OutlineCustomPass.RegisterRenderers(_renderers);
            }
            else
            {
                OutlineCustomPass.UnregisterRenderers(_renderers);
            }
        }

        private void OnDisable()
        {
            if (_isHighlighted)
                SetHighlight(false);
        }
    }
}