using UnityEngine;

namespace Game.Vehicle
{
    public sealed class VehicleDriveSystem
    {
        private readonly VehicleConfig _config;
        private readonly VehicleState _state;
        private readonly IInputReader _input;

        private readonly WheelCollider[] _driveWheels;
        private readonly WheelCollider[] _steeringWheels;

        public VehicleDriveSystem(
            VehicleConfig config,
            VehicleState state,
            IInputReader input,
            WheelCollider[] driveWheels,
            WheelCollider[] steeringWheels)
        {
            _config = config;
            _state = state;
            _input = input;
            _driveWheels = driveWheels;
            _steeringWheels = steeringWheels;
        }

        public void Tick(float deltaTime, Rigidbody rigidbody)
        {
            if (!_state.IsOccupied)
            {
                ApplyBrakes(_config.brakeTorque * 0.5f);
                return;
            }

            float throttle = _input.ThrottleInput;
            float steer = _input.SteerInput;

            Vector3 localVelocity = rigidbody.transform.InverseTransformDirection(rigidbody.linearVelocity);
            _state.ForwardSpeed = localVelocity.z;

            ApplySteering(steer);
            ApplyThrottleAndBrake(throttle);
        }

        private void ApplySteering(float steer)
        {
            float targetAngle = steer * _config.maxSteerAngle;
            float speedFactor = Mathf.InverseLerp(0f, 20f, Mathf.Abs(_state.ForwardSpeed));

            foreach (var wheel in _steeringWheels)
            {
                if (wheel != null)
                    wheel.steerAngle = Mathf.Lerp(targetAngle, targetAngle * 0.4f, speedFactor);
            }
        }

        private void ApplyThrottleAndBrake(float throttle)
        {
            if (throttle > 0.01f)
            {
                if (_state.ForwardSpeed < -0.5f)
                {
                    ApplyBrakes(Mathf.Abs(throttle) * _config.brakeTorque);
                    ReleaseMotor();
                }
                else
                {
                    ReleaseBrakes();
                    ApplyMotor(throttle * _config.motorTorque);
                }
            }
            else if (throttle < -0.01f)
            {
                if (_state.ForwardSpeed > 0.5f)
                {
                    ApplyBrakes(Mathf.Abs(throttle) * _config.brakeTorque);
                    ReleaseMotor();
                }
                else
                {
                    ReleaseBrakes();
                    ApplyMotor(throttle * _config.motorTorque);
                }
            }
            else
            {
                ReleaseMotor();
                ApplyBrakes(_config.brakeTorque * 0.05f);
            }
        }

        private void ApplyMotor(float torque)
        {
            foreach (var wheel in _driveWheels)
            {
                if (wheel != null) wheel.motorTorque = torque;
            }
        }

        private void ReleaseMotor()
        {
            foreach (var wheel in _driveWheels)
            {
                if (wheel != null) wheel.motorTorque = 0f;
            }
        }

        private void ApplyBrakes(float torque)
        {
            foreach (var wheel in _driveWheels)
            {
                if (wheel != null) wheel.brakeTorque = torque;
            }
            foreach (var wheel in _steeringWheels)
            {
                if (wheel != null) wheel.brakeTorque = torque;
            }
        }

        private void ReleaseBrakes()
        {
            foreach (var wheel in _driveWheels)
            {
                if (wheel != null) wheel.brakeTorque = 0f;
            }
            foreach (var wheel in _steeringWheels)
            {
                if (wheel != null) wheel.brakeTorque = 0f;
            }
        }
    }
}
