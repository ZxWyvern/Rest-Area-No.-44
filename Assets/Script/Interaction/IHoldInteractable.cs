namespace Game.Interaction
{
    public interface IHoldInteractable : IInteractable
    {
        bool SupportsHoldInteraction { get; }
        float HoldDurationSeconds { get; }
        void BeginHoldInteraction();
        void UpdateHoldInteraction(float normalizedProgress);
        void CompleteHoldInteraction();
        void CancelHoldInteraction();
    }
}
