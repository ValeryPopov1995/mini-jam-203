using Project.Features.Abilities;
using System.Collections;
using UnityEngine;

public class ProtectiveCircle : Ability
{
    [Header("Circle Settings")]
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private float circleDamage = 20f;      
    [SerializeField] private float circleDuration = 8f;     
    [SerializeField] private bool damagePlayerInside = false; 

    [Header("Tick Rate")]
    [SerializeField] private float damageTickRate = 0.5f;   

    [Header("Visual")]
    [SerializeField] private GameObject circleEffectPrefab; 
    [SerializeField] private Material circleMaterial;       

    private GameObject circleInstance;
    private Coroutine damageCoroutine;

    public override void Activate()
    {
        Vector3 circlePos = transform.root.position;
        circlePos.y = GetGroundY(circlePos);

        if (circleEffectPrefab)
        {
            circleInstance = Instantiate(circleEffectPrefab, circlePos, Quaternion.identity);
            SetupCircleVisual(circleInstance);
        }

        // Запускаем DoT
        damageCoroutine = StartCoroutine(CircleDamageTick(circlePos));

        Debug.Log($"<color=green>ProtectiveCircle</color>: Щит активирован! r:{circleRadius} dmg:{circleDamage}");
    }

    private float GetGroundY(Vector3 pos)
    {
        // Raycast вниз для точного Y
        if (Physics.Raycast(pos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 10f))
            return hit.point.y;
        return pos.y;
    }

    private void SetupCircleVisual(GameObject circle)
    {
        if (circleMaterial != null)
        {
            Renderer rend = circle.GetComponent<Renderer>();
            if (rend != null) rend.material = circleMaterial;
        }

        circle.transform.localScale = Vector3.one * (circleRadius * 2f);
    }

    private IEnumerator CircleDamageTick(Vector3 center)
    {
        float elapsed = 0f;
        while (elapsed < circleDuration)
        {
            DealCircleDamage(center);
            elapsed += damageTickRate;
            yield return new WaitForSeconds(damageTickRate);
        }

        // Cleanup
        if (circleInstance != null)
            Destroy(circleInstance);
        damageCoroutine = null;
    }

    private void DealCircleDamage(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, circleRadius);

        foreach (var col in hits)
        {
            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target == null) continue;

            bool isPlayer = col.transform.IsChildOf(transform.root);
            if (isPlayer && !damagePlayerInside) continue;

            target.TakeDamage(circleDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = transform.root.position;
        pos.y = GetGroundY(pos);
        Gizmos.DrawWireSphere(pos, circleRadius);
    }
}
