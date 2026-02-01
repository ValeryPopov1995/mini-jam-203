using UnityEngine;

namespace MiniJam203.Player
{
    public class PlayerState : MonoBehaviour
    {
        // Public state properties
        public bool IsGrounded { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFalling { get; set; }
        public bool IsOnLadder { get; set; }
        public float CurrentSpeed { get; set; }
        public Vector3 Velocity { get; set; }
        public float FallDistance { get; set; }

        // Events
        public event System.Action OnGrounded;
        public event System.Action OnJump;
        public event System.Action OnLanded;
        public event System.Action OnStartedFalling;

        private bool wasGrounded;
        private float fallStartY;

        private void Update()
        {
            UpdateFallingState();
            UpdateGroundState();
        }

        private void UpdateFallingState()
        {
            if (!IsGrounded && Velocity.y < -0.1f)
            {
                IsFalling = true;
                IsJumping = false;

                if (!wasGrounded)
                {
                    OnStartedFalling?.Invoke();
                    fallStartY = transform.position.y;
                }

                FallDistance = Mathf.Max(0, fallStartY - transform.position.y);
            }
            else
            {
                IsFalling = false;
            }
        }

        private void UpdateGroundState()
        {
            if (!wasGrounded && IsGrounded)
            {
                OnLanded?.Invoke();
                OnGrounded?.Invoke();
                FallDistance = 0;
            }

            wasGrounded = IsGrounded;
        }

        public void SetJumping()
        {
            IsJumping = true;
            IsFalling = false;
            OnJump?.Invoke();
        }
    }
}