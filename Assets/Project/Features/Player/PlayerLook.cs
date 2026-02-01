using UnityEngine;

namespace MiniJam203.Player
{
    using UnityEngine;

    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private IMoveInput input;

        [Header("Settings")]
        [SerializeField] private float maxLookAngle = 90f;
        [SerializeField] private bool invertY = false;

        [Header("Smoothing")]
        [SerializeField] private bool enableSmoothing = true;
        [SerializeField] private float smoothingFactor = 0.1f;

        private float xRotation = 0f;
        private Vector2 currentLookDelta;
        private Vector2 smoothVelocity;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            input = GetComponentInParent<IMoveInput>();

            if (playerBody == null)
            {
                playerBody = transform.parent;
            }
        }

        private void Update()
        {
            if (input == null || Time.timeScale == 0) return;

            HandleLookRotation();
        }

        private void HandleLookRotation()
        {
            Vector2 lookDelta = input.LookDelta;

            if (enableSmoothing)
            {
                lookDelta = Vector2.SmoothDamp(currentLookDelta, lookDelta, ref smoothVelocity, smoothingFactor);
                currentLookDelta = lookDelta;
            }

            float yMultiplier = invertY ? 1f : -1f;
            xRotation += lookDelta.y * yMultiplier;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerBody.Rotate(Vector3.up * lookDelta.x);
        }

        public void SetSensitivity(float mouseSens, float gamepadSens)
        {
            // This would be called from an options menu
            // The sensitivity is actually handled in SimpleInputHandler
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }
    }
}