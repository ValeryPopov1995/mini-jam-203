using UnityEngine;

namespace MiniJam203.Player
{
    public interface IPlayerState
    {
        bool IsGrounded { get; }
        bool IsCrouching { get; }
        bool IsSprinting { get; }
        bool IsJumping { get; }
        bool IsFalling { get; }
        bool IsOnLadder { get; }
        float CurrentSpeed { get; }
        Vector3 Velocity { get; }
    }
}