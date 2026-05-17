using UnityEngine;

namespace Game.Vehicle
{
    [CreateAssetMenu(menuName = "Game/Vehicle/Vehicle Config")]
    public sealed class VehicleConfig : ScriptableObject
    {
        [Header("Engine & Physics")]
        public float mass = 1500f;
        public float motorTorque = 400f;
        public float brakeTorque = 3000f;
        public float maxSteerAngle = 35f;

        [Header("Exit")]
        public float exitClearanceRadius = 1f;
        public LayerMask exitObstacleMask = -1;
    }
}
