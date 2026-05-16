using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Base class untuk semua objek interaktif.
    /// Extend class ini dan override OnInteract() untuk behavior spesifik.
    /// 
    /// Contoh:
    ///   public sealed class DoorInteractable : InteractableBase { ... }
    ///   public sealed class ItemPickup : InteractableBase { ... }
    /// </summary>
    [RequireComponent(typeof(InteractableHighlight))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Interaction")]
        [SerializeField] protected string _interactionPrompt = "Interact";
        [SerializeField] protected bool   _canInteract       = true;

        // ── IInteractable ─────────────────────────────────────────────────────────
        public virtual string InteractionPrompt => _interactionPrompt;
        public virtual bool   CanInteract       => _canInteract && isActiveAndEnabled;

        // Override di subclass untuk behavior spesifik
        public abstract void OnInteract();

        public virtual void OnHoverEnter()
        {
            // Override jika butuh behavior tambahan saat hover (misal: play audio)
        }

        public virtual void OnHoverExit()
        {
            // Override jika butuh behavior tambahan saat hover exit
        }

        // ── Protected Helpers ─────────────────────────────────────────────────────
        protected void SetInteractable(bool state)
        {
            _canInteract = state;
        }
    }
}
