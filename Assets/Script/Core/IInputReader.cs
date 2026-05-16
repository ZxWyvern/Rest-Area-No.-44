using System;
using UnityEngine;

namespace Game
{
    public interface IInputReader
    {
        event Action<Vector2> MovePerformed;
        event Action<Vector2> LookPerformed;
        event Action          JumpStarted;
        event Action          JumpCanceled;
        event Action          SprintStarted;
        event Action          SprintCanceled;
        event Action          CrouchStarted;
        event Action          CrouchCanceled;
        event Action          InteractPerformed;
        event Action          PausePerformed;

        Vector2 MoveInput   { get; }
        Vector2 LookInput   { get; }
        bool    IsJumping   { get; }
        bool    IsSprinting { get; }
        bool    IsCrouching { get; }

        void Enable();
        void Disable();
    }
}
