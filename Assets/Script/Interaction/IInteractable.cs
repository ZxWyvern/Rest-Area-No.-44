namespace Game.Interaction
{
    public interface IInteractable
    {
        bool CanInteract();
        string GetInteractionPrompt();
        void Interact();
    }
}
