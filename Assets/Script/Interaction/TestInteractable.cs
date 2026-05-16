using Game.Interaction;
using UnityEngine;

public sealed class TestInteractable : InteractableBase
{
    public override void Interact()
    {
        Debug.Log($"[Interact] {gameObject.name}");
    }
}
