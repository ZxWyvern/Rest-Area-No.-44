using UnityEngine;
using Game.Interaction;

public sealed class TestInteractable : InteractableBase
{
    public override void OnInteract()
    {
        Debug.Log($"[Interact] {gameObject.name}");
    }
}