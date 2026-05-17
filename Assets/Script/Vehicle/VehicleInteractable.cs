using Game.Interaction;
using UnityEngine;

namespace Game.Vehicle
{
    [DisallowMultipleComponent]
    public sealed class VehicleInteractable : InteractableBase
    {
        [SerializeField] private VehicleController _vehicleController;

        public override bool CanInteract()
        {
            return _vehicleController != null &&
                   _vehicleController.State != null &&
                   !_vehicleController.State.IsOccupied &&
                   isActiveAndEnabled;
        }

        public override string GetInteractionPrompt()
        {
            if (_vehicleController?.State == null)
                return string.Empty;
            return _vehicleController.State.IsOccupied ? "Occupied" : "Press E to Enter";
        }

        public override void Interact()
        {
            if (_vehicleController == null)
            {
                Debug.LogError("[VehicleInteractable] VehicleController reference missing.", this);
                return;
            }
            _vehicleController.TryEnterVehicle();
        }

        private void Reset()
        {
            _vehicleController = GetComponentInParent<VehicleController>();
        }
    }
}
