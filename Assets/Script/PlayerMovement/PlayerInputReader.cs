using System;
using Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player/Input Reader")]
    public sealed class PlayerInputReader : ScriptableObject, PlayerControls.IPlayerActions, IInputReader
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

        public Vector2 MoveInput   { get; private set; }
        public Vector2 LookInput   { get; private set; }
        public bool    IsJumping   { get; private set; }
        public bool    IsSprinting { get; private set; }
        public bool    IsCrouching { get; private set; }

        private PlayerControls _controls;

        public void Enable()
        {
            if (_controls == null)
            {
                _controls = new PlayerControls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        public void Disable()
        {
            _controls?.Player.Disable();
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
            if (ctx.started)
            {
                IsJumping = true;
                JumpStarted?.Invoke();
            }
            else if (ctx.canceled)
            {
                IsJumping = false;
                JumpCanceled?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                IsSprinting = true;
                SprintStarted?.Invoke();
            }
            else if (ctx.canceled)
            {
                IsSprinting = false;
                SprintCanceled?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                IsCrouching = true;
                CrouchStarted?.Invoke();
            }
            else if (ctx.canceled)
            {
                IsCrouching = false;
                CrouchCanceled?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                InteractPerformed?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                PausePerformed?.Invoke();
        }
    }
}
