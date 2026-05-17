namespace Game.Vehicle
{
    public enum OccupiedState
    {
        Empty,
        Entering,
        Occupied,
        Exiting
    }

    public sealed class VehicleState
    {
        public OccupiedState Occupied { get; set; } = OccupiedState.Empty;
        public float ForwardSpeed { get; set; }

        public bool IsOccupied => Occupied == OccupiedState.Occupied;
    }
}
