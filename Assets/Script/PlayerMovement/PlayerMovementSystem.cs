using System;
using UnityEngine;

namespace Game.Player
{
    public enum LocomotionState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Airborne
    }

    public sealed class PlayerMovementSystem : IDisposable
    {
        public LocomotionState State { get; private set; }
        public bool IsGrounded => _grounded;
        public bool IsCrouching => _crouchActive;
        public bool IsMoving => _input.MoveInput.sqrMagnitude > 0.01f;
        public float CurrentSpeed { get; private set; }
        public Vector3 HorizontalVelocity { get; private set; }

        private readonly CharacterController _cc;
        private readonly Transform _transform;
        private readonly IInputReader _input;
        private readonly PlayerMovementConfig _config;

        private Vector3 _velocity;
        private float _verticalVelocity;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isJumpingUp;
        private bool _jumpCutApplied;
        private float _currentHeight;
        private bool _grounded;
        private bool _wasGrounded;
        private bool _crouchActive;
        private bool _sprintActive;
        private bool _disposed;

        private const float PushForce = 2f;

        public PlayerMovementSystem(
            CharacterController cc,
            Transform transform,
            IInputReader input,
            PlayerMovementConfig config)
        {
            _cc = cc;
            _transform = transform;
            _input = input;
            _config = config;
            _currentHeight = config.standHeight;

            cc.slopeLimit = config.slopeLimit;
            cc.stepOffset = config.stepOffset;
            cc.height     = config.standHeight;

            _input.JumpStarted    += OnJumpStarted;
            _input.JumpCanceled   += OnJumpCanceled;
            _input.SprintStarted  += OnSprintStarted;
            _input.SprintCanceled += OnSprintCanceled;
            _input.CrouchStarted  += OnCrouchStarted;
            _input.CrouchCanceled += OnCrouchCanceled;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _input.JumpStarted    -= OnJumpStarted;
            _input.JumpCanceled   -= OnJumpCanceled;
            _input.SprintStarted  -= OnSprintStarted;
            _input.SprintCanceled -= OnSprintCanceled;
            _input.CrouchStarted  -= OnCrouchStarted;
            _input.CrouchCanceled -= OnCrouchCanceled;
        }

        public void Tick(float dt)
        {
            _grounded = _cc.isGrounded;
            UpdateGroundedState();
            UpdateTimers(dt);
            UpdateCrouch();
            UpdateState();
            HandleCrouch(dt);
            HandleHorizontalMovement(dt);
            HandleVerticalMovement(dt);
            ApplyMove(dt);
            _wasGrounded = _grounded;
        }

        private void UpdateGroundedState()
        {
            if (_grounded)
            {
                _coyoteTimer = _config.coyoteTime;

                if (!_wasGrounded)
                {
                    _isJumpingUp = false;
                    _jumpCutApplied = false;
                    if (_verticalVelocity < 0f)
                        _verticalVelocity = -2f;
                }
            }
        }

        private void UpdateTimers(float dt)
        {
            if (!_grounded)
                _coyoteTimer = Mathf.Max(0f, _coyoteTimer - dt);

            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - dt);
        }

        private void OnSprintStarted()  => _sprintActive = true;
        private void OnSprintCanceled() => _sprintActive = false;
        private void OnCrouchStarted()  => _crouchActive = true;
        private void OnCrouchCanceled() => _crouchActive = false;

        private void UpdateCrouch()
        {
            bool wantsCrouch = _crouchActive;
            bool canStand = CanStandUp();

            if (wantsCrouch)
            {
                _crouchActive = true;
            }
            else if (_crouchActive && !canStand)
            {
                _crouchActive = true;
            }
            else
            {
                _crouchActive = false;
            }
        }

        private void UpdateState()
        {
            if (!_grounded)
            {
                State = LocomotionState.Airborne;
                return;
            }

            if (_crouchActive)
            {
                State = LocomotionState.Crouching;
            }
            else if (_sprintActive && IsMoving)
            {
                State = LocomotionState.Sprinting;
            }
            else if (IsMoving)
            {
                State = LocomotionState.Walking;
            }
            else
            {
                State = LocomotionState.Idle;
            }
        }

        private void HandleHorizontalMovement(float dt)
        {
            Vector2 rawInput = _input.MoveInput;
            bool moving = rawInput.sqrMagnitude > 0.01f;

            float targetSpeed;
            switch (State)
            {
                case LocomotionState.Crouching: targetSpeed = _config.crouchSpeed; break;
                case LocomotionState.Sprinting: targetSpeed = _config.sprintSpeed; break;
                default:                        targetSpeed = _config.walkSpeed;   break;
            }

            Vector3 inputDir = Vector3.zero;
            if (moving)
            {
                inputDir = _transform.right * rawInput.x
                         + _transform.forward * rawInput.y;
                inputDir.Normalize();
            }

            Vector3 targetVelocity = inputDir * targetSpeed;

            float accel = _grounded
                ? (moving ? _config.groundAcceleration : _config.groundDeceleration)
                : _config.airAcceleration;

            HorizontalVelocity = Vector3.Lerp(HorizontalVelocity, targetVelocity, accel * dt);
            CurrentSpeed = HorizontalVelocity.magnitude;
        }

        private void HandleVerticalMovement(float dt)
        {
            if (_grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
                return;
            }

            float gravityMultiplier = _isJumpingUp && _verticalVelocity > 0f
                ? 1f
                : _config.fallGravityMultiplier;

            _verticalVelocity -= _config.gravityMagnitude * gravityMultiplier * dt;
            _verticalVelocity = Mathf.Max(_verticalVelocity, -_config.maxFallSpeed);

            if (_verticalVelocity <= 0f)
                _isJumpingUp = false;
        }

        private void OnJumpStarted()
        {
            _jumpBufferTimer = _config.jumpBufferTime;
            TryJump();
        }

        private void OnJumpCanceled()
        {
            if (_isJumpingUp && !_jumpCutApplied && _verticalVelocity > 0f)
            {
                _verticalVelocity *= 1f / _config.jumpCutGravityMultiplier;
                _jumpCutApplied = true;
            }
        }

        private void TryJump()
        {
            bool canJump = _grounded || _coyoteTimer > 0f;
            bool hasBuffer = _jumpBufferTimer > 0f;

            if (!canJump || !hasBuffer) return;

            _verticalVelocity = _config.JumpVelocity;
            _isJumpingUp = true;
            _jumpCutApplied = false;
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
        }

        private void HandleCrouch(float dt)
        {
            float targetHeight = _crouchActive ? _config.crouchHeight : _config.standHeight;
            _currentHeight = Mathf.Lerp(_currentHeight, targetHeight,
                                        _config.crouchTransitionSpeed * dt);

            _cc.height = _currentHeight;
            _cc.center = new Vector3(0f, _currentHeight * 0.5f, 0f);
        }

        private bool CanStandUp()
        {
            Vector3 origin = _transform.position + Vector3.up * _config.crouchHeight;
            float checkRadius = _cc.radius * 0.9f;
            float checkDist = _config.standHeight - _config.crouchHeight - checkRadius;

            return !Physics.SphereCast(origin, checkRadius, Vector3.up, out _,
                                       checkDist, ~LayerMask.GetMask("Player"),
                                       QueryTriggerInteraction.Ignore);
        }

        private void ApplyMove(float dt)
        {
            Vector3 motion = HorizontalVelocity + Vector3.up * _verticalVelocity;
            _cc.Move(motion * dt);
        }

        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic) return;

            if (hit.moveDirection.y < -0.3f) return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            rb.AddForce(pushDir * PushForce, ForceMode.Impulse);
        }
    }
}
