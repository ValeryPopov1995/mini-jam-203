using MiniJam203.Player;
using UnityEngine;

namespace MiniJam203.Environment
{
    public class Ladder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float climbSpeed = 5f;
        [SerializeField] private float snapRadius = 1f;
        [SerializeField] private float topExitOffset = 1f;
        [SerializeField] private float bottomExitOffset = 0.5f;

        [Header("Visual")]
        [SerializeField] private Color gizmoColor = Color.green;

        private BoxCollider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector3(2, 10, 0.5f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                IPlayerMotor motor = other.GetComponent<IPlayerMotor>();
                if (motor != null && !motor.IsOnLadder)
                {
                    motor.EnterLadder(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                IPlayerMotor motor = other.GetComponent<IPlayerMotor>();
                if (motor != null && motor.IsOnLadder)
                {
                    motor.ExitLadder();
                }
            }
        }

        public Vector3 GetClosestPoint(Vector3 playerPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(playerPosition);
            localPos.x = 0;
            localPos.z = 0;
            localPos.y = Mathf.Clamp(localPos.y, -triggerCollider.size.y / 2, triggerCollider.size.y / 2);
            return transform.TransformPoint(localPos);
        }

        public void OnPlayerExit(IPlayerMotor motor)
        {
            // Additional exit logic if needed
        }

        private void OnDrawGizmos()
        {
            if (triggerCollider == null) return;

            Gizmos.color = gizmoColor;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, triggerCollider.size);
            Gizmos.matrix = oldMatrix;

            // Draw entry/exit points
            Gizmos.color = Color.yellow;
            Vector3 topPoint = transform.position + transform.up * (triggerCollider.size.y / 2 + topExitOffset);
            Vector3 bottomPoint = transform.position - transform.up * (triggerCollider.size.y / 2 + bottomExitOffset);

            Gizmos.DrawSphere(topPoint, 0.2f);
            Gizmos.DrawSphere(bottomPoint, 0.2f);
            Gizmos.DrawLine(topPoint, bottomPoint);
        }
    }
}