using System.Collections;
using UnityEngine;

namespace Project.Features.Abilities
{
    [System.Serializable]
    public class SneezeAbility : Ability
    {
        [Header("Wave Settings")]
        [SerializeField] private float waveRadius = 5f;
        [SerializeField] private float coneAngle = 60f;  // Угол конуса вперёд
        [SerializeField] private float pushForce = 15f;
        [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Effects")]
        [SerializeField] private GameObject waveEffectPrefab;
        [SerializeField] private ParticleSystem pushParticles;
        
        [Header("Targets")]
        [SerializeField] private bool pushEnemies = true;
        [SerializeField] private bool pushPhysicsObjects = true;
        
        [Header("Settings for non-rb enemies")]
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private float knockbackDistance = 2f;
        public override void Activate()
        {
            Vector3 center = transform.position;
            Vector3 forward = Camera.main.transform.forward;  // Направление взгляда
            
            if (waveEffectPrefab != null)
            {
                GameObject effect = Instantiate(waveEffectPrefab, center + forward * waveRadius * 0.5f, 
                    Quaternion.LookRotation(forward));
                Destroy(effect, 3f);
            }
            
            if (pushParticles != null)
            {
                pushParticles.transform.position = center + forward * 1f;
                pushParticles.transform.rotation = Quaternion.LookRotation(forward);
                pushParticles.Play();
            }
            
            RepulseForward(center, forward);
            Debug.Log($"<color=cyan>{this}</color>: Волна вперёд!");
        }
        
        private void RepulseForward(Vector3 center, Vector3 forward)
        {
            Collider[] nearby = Physics.OverlapSphere(center, waveRadius);
    
            foreach (var col in nearby)
            {
                Vector3 toTarget = (col.transform.position - center).normalized;
                float distance = Vector3.Distance(center, col.transform.position);
        
                // Только вперёд
                float angle = Vector3.Angle(forward, toTarget);
                if (angle > coneAngle * 0.5f || distance < 0.5f) continue;
        
                Rigidbody rb = col.attachedRigidbody;
        
                if (rb != null)
                {
                    ApplyRepulse(rb, toTarget, distance);
                }
                else if (pushPhysicsObjects)
                {
                    ApplyTransformPush(col.transform, toTarget, distance);
                }
            }
        }

        private void ApplyRepulse(Rigidbody rb, Vector3 direction, float distance)
        {
            float forceMultiplier = falloffCurve.Evaluate(distance / waveRadius);
    
            if (rb.isKinematic)
            {
                // Kinematic — напрямую velocity
                StartCoroutine(KnockbackRoutine(rb.transform,direction,pushForce));
                rb.linearVelocity = direction * pushForce * forceMultiplier;
            }
            else
            {
                // Dynamic — импульс
                Vector3 force = direction * pushForce * forceMultiplier;
                rb.AddForce(force, ForceMode.Impulse);
        
                Vector3 torque = Vector3.Cross(direction, Vector3.up) * pushForce * 0.5f;
                rb.AddTorque(torque, ForceMode.Impulse);
            }
        }

        private void ApplyTransformPush(Transform target, Vector3 direction, float distance)
        {
            float forceMultiplier = falloffCurve.Evaluate(distance / waveRadius);
            Vector3 pushVelocity = direction * pushForce * forceMultiplier * 0.5f;
    
            // Плавное отталкивание
            target.position += pushVelocity/* * Time.deltaTime*/;
            target.Rotate(Vector3.up, Random.Range(-45, 45) * forceMultiplier);
        }
        
        private IEnumerator KnockbackRoutine(Transform enemy, Vector3 dir, float force)
        {
            float time = 0f;
            Vector3 start = enemy.position;
            Vector3 target = start + dir.normalized * knockbackDistance * (force / 10f);

            while (time < knockbackDuration)
            {
                float t = time / knockbackDuration;
                enemy.position = Vector3.Lerp(start, target, t);
                time += Time.deltaTime;
                yield return null;
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 forward = Camera.main ? Camera.main.transform.forward : transform.forward;
            
            // Конус вперёд
            Gizmos.DrawWireSphere(transform.position, waveRadius);
            
            // Линии конуса
            Vector3 leftEdge = Quaternion.Euler(0, -coneAngle * 0.5f, 0) * forward * waveRadius;
            Vector3 rightEdge = Quaternion.Euler(0, coneAngle * 0.5f, 0) * forward * waveRadius;
            
            Gizmos.DrawLine(transform.position, transform.position + leftEdge);
            Gizmos.DrawLine(transform.position, transform.position + rightEdge);
        }
    }
}
