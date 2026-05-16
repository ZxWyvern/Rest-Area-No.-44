namespace Game.Interaction
{
    public interface IHoldInteractable : IInteractable
    {
        void OnInteractStarted();
        void OnInteractHold(float progress);
        void OnInteractCompleted();
        void OnInteractCanceled();
        float HoldDuration { get; }
    }
}
