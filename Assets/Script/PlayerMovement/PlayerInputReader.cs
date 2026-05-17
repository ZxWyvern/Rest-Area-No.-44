using System;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player/Input Reader")]
    public sealed class PlayerInputReader : ScriptableObject,
        PlayerControls.IPlayerActions,
        PlayerControls.IVehicleActions,
        IInputReader
    {
        public event Action<Vector2> MovePerformed;
        public event Action<Vector2> LookPerformed;
        public event Action          JumpStarted;
        public event Action          JumpCanceled;
        public event Action          SprintStarted;
        public event Action          SprintCanceled;
        public event Action          CrouchStarted;
        public event Action          CrouchCanceled;
        public event Action          InteractPerformed;
        public event Action          PausePerformed;

        public event Action ExitVehiclePerformed;

        public Vector2 MoveInput   { get; private set; }
        public Vector2 LookInput   { get; private set; }
        public bool    IsJumping   { get; private set; }
        public bool    IsSprinting { get; private set; }
        public bool    IsCrouching { get; private set; }

        public float ThrottleInput { get; private set; }
        public float SteerInput    { get; private set; }

        private PlayerControls _controls;

        public void Enable()
        {
            if (_controls == null)
            {
                _controls = new PlayerControls();
                _controls.Player.SetCallbacks(this);
                _controls.Vehicle.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        public void Disable()
        {
            _controls?.Player.Disable();
            _controls?.Vehicle.Disable();
        }

        public void EnableVehicleMap()
        {
            if (_controls == null) return;
            _controls.Player.Disable();
            _controls.Vehicle.Enable();
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
        }

        public void EnablePlayerMap()
        {
            if (_controls == null) return;
            _controls.Vehicle.Disable();
            _controls.Player.Enable();
            ThrottleInput = 0f;
            SteerInput = 0f;
        }

        private void OnDestroy()
        {
            _controls?.Dispose();
            _controls = null;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
            MovePerformed?.Invoke(MoveInput);
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            LookInput = ctx.ReadValue<Vector2>();
            LookPerformed?.Invoke(LookInput);
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started)  { IsJumping = true;  JumpStarted?.Invoke(); }
            else if (ctx.canceled) { IsJumping = false; JumpCanceled?.Invoke(); }
        }

        public void OnSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.started)       { IsSprinting = true;  SprintStarted?.Invoke(); }
            else if (ctx.canceled) { IsSprinting = false; SprintCanceled?.Invoke(); }
        }

        public void OnCrouch(InputAction.CallbackContext ctx)
        {
            if (ctx.started)       { IsCrouching = true;  CrouchStarted?.Invoke(); }
            else if (ctx.canceled) { IsCrouching = false; CrouchCanceled?.Invoke(); }
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) InteractPerformed?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) PausePerformed?.Invoke();
        }

        public void OnThrottle(InputAction.CallbackContext ctx)
        {
            ThrottleInput = ctx.ReadValue<float>();
        }

        public void OnSteer(InputAction.CallbackContext ctx)
        {
            SteerInput = ctx.ReadValue<float>();
        }

        public void OnExitVehicle(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) ExitVehiclePerformed?.Invoke();
        }
    }
}
