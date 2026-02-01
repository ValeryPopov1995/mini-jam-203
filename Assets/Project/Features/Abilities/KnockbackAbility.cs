using Project.Features.Abilities;
using System.Collections;
using UnityEngine;

public class KnockbackAbility : Ability
{
    [Header("Cone Settings")]
    [SerializeField] private float coneRadius = 5f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Push Force")]
    [SerializeField] private float pushStrength = 15f;  // 10=слабый, 25=ракета!
    [SerializeField] private float damage = 0;

    [Header("Targets")]
    [SerializeField] private bool pushIDamageable = true;     
    [SerializeField] private bool pushPhysicsObjects = true;  

    [Header("Recoil")]
    [SerializeField] private bool recoil = true;
    [SerializeField] private float recoilDistance = 1.5f;

    [Header("Effects")]
    [SerializeField] private bool enableEffects = true;
    [SerializeField] private GameObject waveEffectPrefab;      
    [SerializeField] private ParticleSystem shotgunParticles; 

    public override void Activate()
    {
        Vector3 center = transform.root.position;
        Vector3 forward = Camera.main.transform.forward;

        SpawnEffects(center, forward);

        Collider[] nearby = Physics.OverlapSphere(center, coneRadius);
        int damageableCount = 0, physicsCount = 0;

        foreach (var col in nearby)
        {
            // Skip player
            if (col.transform.IsChildOf(transform.root)) continue;

            Vector3 toTarget = (col.transform.position - center).normalized;
            float distance = Vector3.Distance(center, col.transform.position);

            float angle = Vector3.Angle(forward, toTarget);
            if (angle > coneAngle * 0.5f || distance < 0.5f) continue;

            Rigidbody rb = col.attachedRigidbody ?? col.GetComponentInParent<Rigidbody>();
            Transform targetRoot = rb ? rb.transform.root : col.transform.root;

            float distMultiplier = falloffCurve.Evaluate(distance / coneRadius);

            // IDamageable (враги)
            if (pushIDamageable && col.TryGetComponent(out IDamageable enemy))
            {
                KnockbackTarget(targetRoot, toTarget, pushStrength * distMultiplier);
                damageableCount++;
                enemy.TakeDamage(damage);
            }
            // Простые RB (мячики/ящики)
            else if (pushPhysicsObjects && rb != null && !rb.isKinematic)
            {
                ApplyPhysicsPush(rb, toTarget, pushStrength * distMultiplier);
                physicsCount++;
            }
        }

        if (recoil)
        {
            StartCoroutine(PlayerRecoil(transform.root, -forward, recoilDistance));
        }

        Debug.Log($"<color=yellow>SneezeAbility</color>: IDamageable:{damageableCount} Physics:{physicsCount} Strength:{pushStrength}");
    }

    private void SpawnEffects(Vector3 center, Vector3 forward)
    {
        if (!enableEffects) return;

        if (waveEffectPrefab != null)
        {
            Vector3 spawnPos = center + forward * coneRadius * 0.5f;
            GameObject effect = Instantiate(waveEffectPrefab, spawnPos, Quaternion.LookRotation(forward));
            Destroy(effect, 3f);
        }

        if (shotgunParticles != null)
        {
            shotgunParticles.transform.position = center + forward * 0.5f;
            shotgunParticles.transform.rotation = Quaternion.LookRotation(forward);
            shotgunParticles.Play();
        }
    }

    private void KnockbackTarget(Transform targetRoot, Vector3 direction, float strength)
    {
        StartCoroutine(PlayerRecoil(targetRoot, direction, strength * 0.1f));
    }

    private void ApplyPhysicsPush(Rigidbody rb, Vector3 direction, float strength)
    {
        Vector3 force = direction * strength;
        rb.AddForce(force, ForceMode.Impulse);
        Vector3 torque = Vector3.Cross(direction, Vector3.up) * strength * 0.5f;
        rb.AddTorque(torque, ForceMode.Impulse);
    }

    private IEnumerator PlayerRecoil(Transform targetRoot, Vector3 direction, float distance)
    {
        Vector3 startPos = targetRoot.position;
        Vector3 targetPos = startPos + direction.normalized * distance;

        float time = 0f;
        float duration = knockbackDuration;

        while (time < duration)
        {
            float t = time / duration;
            targetRoot.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 forward = Camera.main ? Camera.main.transform.forward : transform.forward;
        Vector3 pos = transform.root.position;

        Gizmos.DrawWireSphere(pos, coneRadius);

        Vector3 left = Quaternion.Euler(0, -coneAngle * 0.5f, 0) * forward * coneRadius;
        Vector3 right = Quaternion.Euler(0, coneAngle * 0.5f, 0) * forward * coneRadius;

        Gizmos.DrawLine(pos, pos + left);
        Gizmos.DrawLine(pos, pos + right);

        if (recoil)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos - forward * recoilDistance);
        }
    }
}
