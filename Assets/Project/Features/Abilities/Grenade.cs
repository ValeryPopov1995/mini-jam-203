using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private float radius, damage, pushStrength, knockbackDuration;
    private GameObject explosionEffect;

    public void Setup(float expRadius, float expDamage, float pushStr, float kbDuration,
                     GameObject effect)
    {
        radius = expRadius;
        damage = expDamage;
        pushStrength = pushStr;
        knockbackDuration = kbDuration;
        explosionEffect = effect;

        Destroy(gameObject, 9);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        Vector3 center = transform.position;

        // VFX
        if (explosionEffect != null)
            Instantiate(explosionEffect, center, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(center, radius);

        foreach (var col in hits)
        {
            if (!col.attachedRigidbody) continue;
            if (col.attachedRigidbody.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage, gameObject);
            }
        }

        foreach (var col in hits)
        {
            if (!col.attachedRigidbody) continue;
            Rigidbody rb = col.attachedRigidbody ?? col.GetComponentInParent<Rigidbody>();

            Transform targetRoot = rb.transform.root;
            Vector3 toTarget = (targetRoot.position - center).normalized;
            float dist = Vector3.Distance(center, targetRoot.position);
            float falloff = Mathf.Clamp01(1f - (dist / radius));

            if (rb.isKinematic)
            {
                targetRoot.GetComponent<MonoBehaviour>()?.StartCoroutine(Knockback(targetRoot, toTarget, pushStrength * falloff * 0.1f));
            }
            else
            {
                rb.AddExplosionForce(pushStrength * falloff, center, radius, 1f, ForceMode.Impulse);
            }
        }
    }

    private static IEnumerator Knockback(Transform targetRoot, Vector3 direction, float distance)
    {
        Vector3 startPos = targetRoot.position;
        Vector3 targetPos = startPos + direction.normalized * distance;

        float time = 0f;
        float duration = 0.3f;

        while (time < duration)
        {
            float t = time / duration;
            targetRoot.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }
    }
}
