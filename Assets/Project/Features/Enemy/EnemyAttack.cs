using UnityEngine;

[RequireComponent(typeof(EnemyConfigHolder))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private float projectileSpawnOffset = 0.6f;

    [Tooltip("Слои, которым можно наносить урон (НЕ Enemy)")]
    [SerializeField] public LayerMask damageMask = ~0;

    private EnemyConfig config;
    private float lastAttackTime;

    public void Init(EnemyConfig cfg)
    {
        config = cfg;
    }

    private void Awake()
    {
        config = GetComponent<EnemyConfigHolder>().Config;
    }

    public float AttackRange => config != null ? config.attackRange : 1f;

    public void Tick(Transform target)
    {
        if (config == null || target == null) return;

        FaceTarget(target);

        if (config.attackType == EnemyConfig.EnemyAttackType.Ranged && !HasLineOfSight(target))
            return;

        if (Time.time >= lastAttackTime + config.attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack(Transform target)
    {
        if (config.attackType == EnemyConfig.EnemyAttackType.Melee)
            DoMeleeAttack();
        else
            DoRangedAttack(target);
    }

    private void DoMeleeAttack()
    {
        Vector3 attackCenter = transform.position + transform.forward * (AttackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, AttackRange, damageMask);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(config.damage, gameObject);
                break;
            }
        }

        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    private void DoRangedAttack(Transform target)
    {
        if (target.GetComponent<EnemyAttack>() != null) return;

        Vector3 baseOrigin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);
        Vector3 dir = (aimPoint - baseOrigin).normalized;

        if (config.projectilePrefab == null)
        {
            if (Physics.Raycast(baseOrigin, dir, out RaycastHit hit, AttackRange, damageMask))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(config.damage, gameObject);
            }
        }
        else
        {
            Vector3 origin = baseOrigin + dir * projectileSpawnOffset;

            var proj = Instantiate(config.projectilePrefab, origin, Quaternion.identity);
            proj.Init(config.damage, config.projectileSpeed, dir, gameObject);
        }

        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    private Vector3 GetAimPoint(Transform target)
    {
        if (target.TryGetComponent<Collider>(out var col))
            return col.bounds.center;
        return target.position;
    }

    private void FaceTarget(Transform target)
    {
        var brain = GetComponent<EnemyBrain>();
        if (brain != null)
            brain.FaceVisualTowards(target);
    }

    public bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);
        Vector3 dir = aimPoint - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, lineOfSightMask))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;

            return false;
        }

        return true;
    }

    // ------------------ Gizmos ------------------
    private void OnDrawGizmosSelected()
    {
        if (config == null) return;
        // Красная сфера — зона melee атаки
        Gizmos.color = Color.red; 
        Vector3 attackCenter = transform.position + transform.forward * (AttackRange * 0.5f);
        Gizmos.DrawWireSphere(attackCenter, AttackRange);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; var enemyTarget = GetComponent<EnemyTarget>();
        if (enemyTarget == null || enemyTarget.Target == null) 
            return; 
        Vector3 origin = transform.position + Vector3.up * eyeHeight; 
        Vector3 aimPoint = GetAimPoint(enemyTarget.Target); 
        Vector3 dir = aimPoint - origin; float dist = dir.magnitude; 
        bool blocked = Physics.Raycast(origin, dir.normalized, dist, lineOfSightMask);
        // Проверяем, есть ли попадание по damageMask
        bool hitsDamageable = Physics.Raycast(origin, dir.normalized, out RaycastHit hit2, dist, damageMask);
        if (hitsDamageable) 
            Gizmos.color = Color.green; // атака попадёт по цели (игрок)
        else if (blocked)
            Gizmos.color = Color.red; // линия блокируется
        else 
            Gizmos.color = Color.yellow;
        // свободная линия, но не цель
        Gizmos.DrawLine(origin, aimPoint); // Дополнительно для melee: линии к потенциальным целям
        if (config.attackType == EnemyConfig.EnemyAttackType.Melee)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (AttackRange * 0.5f), AttackRange, damageMask);
            foreach (var col in hits)
            {
                if (col.transform == transform) continue;
                Gizmos.color = Color.cyan; Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, col.transform.position);
            }
        }
    }

    public void Stop()
    {
        // Можно добавить логику остановки атаки, если нужно
    }
}