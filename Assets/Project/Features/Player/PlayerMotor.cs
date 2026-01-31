using MiniJam203.Environment;

namespace MiniJam203.Player
{
    using UnityEngine;

    [RequireComponent(typeof(CharacterController), typeof(PlayerState))]
    public class PlayerMotor : MonoBehaviour, IPlayerMotor
    {
        [Header("References")]
        private CharacterController controller;
        private PlayerState state;
        private IMoveInput input;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 12f;
        [SerializeField] private float crouchSpeed = 3.5f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float airControl = 0.3f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float terminalVelocity = 50f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Crouch")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Ladder")]
        [SerializeField] private float ladderClimbSpeed = 5f;
        [SerializeField] private float ladderSnapForce = 5f;

        [Header("Fall Damage")]
        [SerializeField] private float minFallDamageHeight = 5f;
        [SerializeField] private float maxFallDamageHeight = 15f;
        [SerializeField] private float maxFallDamage = 50f;

        // IPlayerMotor implementation
        public bool IsGrounded => state.IsGrounded;
        public bool IsOnLadder => state.IsOnLadder;
        public bool IsCrouching => state.IsCrouching;
        public Vector3 Velocity => velocity;

        // Private
        private Vector3 velocity;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private Ladder currentLadder;
        private Vector3 externalForce;
        private float originalStepOffset;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            state = GetComponent<PlayerState>();
            input = GetComponent<IMoveInput>();

            originalStepOffset = controller.stepOffset;

            if (input == null)
                Debug.LogError("IMoveInput component not found!");

            SetupInputEvents();
        }

        private void SetupInputEvents()
        {
            if (input != null)
            {
                input.OnJumpPerformed += OnJumpInput;
                input.OnSprintPerformed += OnSprintInput;
                input.OnCrouchPerformed += OnCrouchInput;
            }
        }

        private void OnDestroy()
        {
            if (input != null)
            {
                input.OnJumpPerformed -= OnJumpInput;
                input.OnSprintPerformed -= OnSprintInput;
                input.OnCrouchPerformed -= OnCrouchInput;
            }
        }

        private void Update()
        {
            UpdateTimers();

            if (state.IsOnLadder)
            {
                HandleLadderMovement();
            }
            else
            {
                HandleGroundMovement();
                HandleJump();
                HandleCrouch();
                ApplyGravity();
                HandleExternalForces();
            }

            ApplyMovement();
            UpdateState();
        }

        private void HandleGroundMovement()
        {
            if (input == null) return;

            Vector3 worldMoveDir = transform.TransformDirection(input.MoveDirection);

            float targetSpeed = state.IsCrouching ? crouchSpeed :
                               (input.SprintHeld ? sprintSpeed : walkSpeed);

            state.CurrentSpeed = Mathf.Lerp(state.CurrentSpeed, targetSpeed,
                acceleration * Time.deltaTime);

            Vector3 targetVelocity = worldMoveDir * state.CurrentSpeed;

            float controlFactor = state.IsGrounded ? 1f : airControl;
            velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x,
                acceleration * controlFactor * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z,
                acceleration * controlFactor * Time.deltaTime);
        }

        private void HandleJump()
        {
            bool canJump = state.IsGrounded || coyoteTimer > 0;

            if (jumpBufferTimer > 0 && canJump && !state.IsCrouching)
            {
                Jump(jumpForce);
            }
        }

        private void HandleCrouch()
        {
            if (input == null) return;

            bool wantsToCrouch = input.CrouchHeld;

            if (wantsToCrouch && !state.IsCrouching)
            {
                StartCrouch();
            }
            else if (!wantsToCrouch && state.IsCrouching)
            {
                TryUncrouch();
            }
        }

        private void StartCrouch()
        {
            state.IsCrouching = true;
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
        }

        private void TryUncrouch()
        {
            float checkDistance = standingHeight - crouchHeight;
            Vector3 start = transform.position + Vector3.up * (crouchHeight + 0.1f);

            if (!Physics.Raycast(start, Vector3.up, checkDistance, obstacleLayers))
            {
                state.IsCrouching = false;
                controller.height = standingHeight;
                controller.center = new Vector3(0, standingHeight / 2f, 0);
            }
        }

        private void HandleLadderMovement()
        {
            if (currentLadder == null || input == null) return;

            Vector3 ladderUp = currentLadder.transform.up;
            Vector3 ladderForward = currentLadder.transform.forward;

            float verticalInput = input.MoveDirection.z;
            float horizontalInput = input.MoveDirection.x;

            velocity = (ladderUp * verticalInput + ladderForward * horizontalInput) * ladderClimbSpeed;

            if (input.JumpPressed)
            {
                ExitLadder();
                velocity = transform.forward * ladderClimbSpeed + Vector3.up * jumpForce * 0.5f;
            }
        }

        private void ApplyGravity()
        {
            if (state.IsGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
                velocity.y = Mathf.Max(velocity.y, -terminalVelocity);
            }
        }

        private void HandleExternalForces()
        {
            if (externalForce.magnitude > 0.1f)
            {
                velocity += externalForce * Time.deltaTime;
                externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.deltaTime * 5f);
            }
        }

        private void ApplyMovement()
        {
            controller.Move(velocity * Time.deltaTime);
            state.Velocity = velocity;
        }

        private void UpdateState()
        {
            state.IsGrounded = controller.isGrounded;
            state.IsSprinting = input != null && input.SprintHeld;

            if (state.IsGrounded)
            {
                coyoteTimer = coyoteTime;
            }
            else if (coyoteTimer > 0)
            {
                coyoteTimer -= Time.deltaTime;
            }

            controller.stepOffset = state.IsGrounded ? originalStepOffset : 0f;
        }

        private void UpdateTimers()
        {
            if (input != null && input.JumpPressed)
            {
                jumpBufferTimer = jumpBufferTime;
            }

            if (jumpBufferTimer > 0)
            {
                jumpBufferTimer -= Time.deltaTime;
            }
        }

        // Event Handlers
        private void OnJumpInput()
        {
            jumpBufferTimer = jumpBufferTime;
        }

        private void OnSprintInput()
        {
            // Can add sprint toggle logic here
        }

        private void OnCrouchInput()
        {
            // Can add crouch toggle logic here
        }

        // IPlayerMotor implementation
        public void AddForce(Vector3 force)
        {
            externalForce += force;
        }

        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }

        public void Jump(float force)
        {
            velocity.y = force;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            state.SetJumping();
        }

        public void EnterLadder(Ladder ladder)
        {
            currentLadder = ladder;
            state.IsOnLadder = true;
            velocity = Vector3.zero;
            externalForce = Vector3.zero;

            Vector3 closestPoint = ladder.GetClosestPoint(transform.position);
            Vector3 snapDirection = (closestPoint - transform.position).normalized;
            AddForce(snapDirection * ladderSnapForce);
        }

        public void ExitLadder()
        {
            if (currentLadder != null)
            {
                currentLadder.OnPlayerExit(this);
                currentLadder = null;
            }
            state.IsOnLadder = false;
        }

        // Fall Damage
        private void HandleLanding(float fallDistance)
        {
            if (fallDistance > minFallDamageHeight)
            {
                float damagePercent = Mathf.Clamp01((fallDistance - minFallDamageHeight) /
                    (maxFallDamageHeight - minFallDamageHeight));
                float damage = maxFallDamage * damagePercent;

                // Apply damage to player health system
                Debug.Log($"Fall damage: {damage}");
            }
        }
    }
}