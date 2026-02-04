using MiniJam203.Environment;
using UnityEngine;

namespace MiniJam203.Player
{
    public interface IPlayerMotor
    {
        bool IsGrounded { get; }
        bool IsOnLadder { get; }
        bool IsCrouching { get; }
        Vector3 Velocity { get; }

        void AddForce(Vector3 force);
        void SetVelocity(Vector3 velocity);
        void Jump(float force);
        void EnterLadder(Ladder ladder);
        void ExitLadder();
    }
}