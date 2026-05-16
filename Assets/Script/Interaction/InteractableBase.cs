using UnityEngine;

namespace Game.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _interactionPrompt = "Interact";
        [SerializeField] protected bool _canInteract = true;

        public virtual bool CanInteract() => _canInteract && isActiveAndEnabled;
        public virtual string GetInteractionPrompt() => _interactionPrompt;
        public abstract void Interact();

        protected void SetInteractable(bool value)
        {
            _canInteract = value;
        }
    }
}
