using UnityEngine;

namespace MiniJam203.Player
{
    public interface IMoveInput
    {
        Vector3 MoveDirection { get; }
        Vector2 LookDelta { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool SprintPressed { get; }
        bool SprintHeld { get; }
        bool CrouchPressed { get; }
        bool CrouchHeld { get; }

        event System.Action OnJumpPerformed;
        event System.Action OnSprintPerformed;
        event System.Action OnCrouchPerformed;
    }
}