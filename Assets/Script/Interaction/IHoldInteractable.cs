namespace Game.Interaction
{
    public interface IHoldInteractable : IInteractable
    {
        bool SupportsHoldInteraction { get; }
        float HoldDurationSeconds { get; }
        void BeginHoldInteraction();
        void UpdateHoldInteraction(float normalizedProgress);
        /// <summary>
        /// Called when hold duration completes. This is the terminal interaction event.
        /// Do NOT expect a subsequent call to <see cref="IInteractable.Interact"/>.
        /// </summary>
        void CompleteHoldInteraction();
        void CancelHoldInteraction();
    }
}
