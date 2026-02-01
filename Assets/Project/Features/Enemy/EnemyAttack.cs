using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyConfigHolder))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] LayerMask lineOfSightMask = ~0;  // Для проверки LOS
    [SerializeField] float eyeHeight = 1f;

    [Tooltip("Выберите слои объектов, которым враг может наносить урон. Не включайте слой Enemy, чтобы враги не атаковали друг друга.")]
    [SerializeField] public LayerMask damageMask = ~0;       // Маска урона через инспектор

    EnemyConfig config;
    float lastAttackTime;

    void Awake()
    {
        // Получаем конфиг через holder
        config = GetComponent<EnemyConfigHolder>().Config;
    }

    public float AttackRange => config != null ? config.attackRange : 1f;

    public void Init(EnemyConfig cfg)
    {
        config = cfg;
    }

    /// <summary>
    /// Проверяет, есть ли линия видимости до указанной цели
    /// </summary>
    public bool HasLineOfSightTo(Transform target)
    {
        return HasLineOfSight(target);
    }

    public bool CanAttack(float distanceToTarget)
    {
        return distanceToTarget <= AttackRange;
    }

    public void Tick(Transform target)
    {
        if (config == null || target == null) return;

        FaceTarget(target);

        // Проверка линии видимости для дальников
        if (config.attackType == EnemyConfig.EnemyAttackType.Ranged && !HasLineOfSight(target))
            return;

        if (Time.time >= lastAttackTime + config.attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
    }

    void PerformAttack(Transform target)
    {
        if (config.attackType == EnemyConfig.EnemyAttackType.Melee)
            DoMeleeAttack();
        else if (config.attackType == EnemyConfig.EnemyAttackType.Ranged)
            DoRangedAttack(target);
    }

    void DoMeleeAttack()
    {
        Vector3 attackCenter = transform.position + transform.forward * (AttackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(attackCenter, AttackRange, damageMask);

        foreach (var col in hits)
        {
            var dmg = col.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(config.damage, gameObject);
                break; // урон только одному объекту
            }
        }

        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    void DoRangedAttack(Transform target)
    {
        // Игнорируем цели с EnemyAttack
        if (target.GetComponent<EnemyAttack>() != null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);

        if (config.projectilePrefab == null)
        {
            Vector3 dir = (aimPoint - origin).normalized;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, AttackRange, damageMask))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(config.damage, gameObject);
            }
        }
        else
        {
            var proj = Instantiate(config.projectilePrefab, origin, Quaternion.identity);
            proj.transform.LookAt(aimPoint);
            proj.Init(config.damage, config.projectileSpeed, gameObject);
        }

        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    Vector3 GetAimPoint(Transform target)
    {
        if (target.TryGetComponent<Collider>(out var col))
            return col.bounds.center;
        return target.position;
    }

    void FaceTarget(Transform target)
    {
        var brain = GetComponent<EnemyBrain>();
        if (brain != null)
            brain.FaceVisualTowards(target);
    }

    bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);
        Vector3 dir = aimPoint - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, lineOfSightMask))
        {
            return false; // что-то блокирует LOS
        }

        return true;
    }

    /// <summary>
    /// Прекращает текущую атаку (сброс анимаций и таймера)
    /// </summary>
    public void Stop()
    {
        //GetComponent<EnemyAnimator>()?.StopAttack();
        lastAttackTime = 0;
    }

    // ------------------ Gizmos ------------------

    void OnDrawGizmosSelected()
    {
        if (config == null) return;

        // Красная сфера — зона melee атаки
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * (AttackRange * 0.5f);
        Gizmos.DrawWireSphere(attackCenter, AttackRange);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        var enemyTarget = GetComponent<EnemyTarget>();
        if (enemyTarget == null || enemyTarget.Target == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(enemyTarget.Target);

        Vector3 dir = aimPoint - origin;
        float dist = dir.magnitude;

        bool blocked = Physics.Raycast(origin, dir.normalized, dist, lineOfSightMask);

        // Проверяем, есть ли попадание по damageMask
        bool hitsDamageable = Physics.Raycast(origin, dir.normalized, out RaycastHit hit2, dist, damageMask);

        if (hitsDamageable)
            Gizmos.color = Color.green;   // атака попадёт по цели (игрок)
        else if (blocked)
            Gizmos.color = Color.red;     // линия блокируется
        else
            Gizmos.color = Color.yellow;  // свободная линия, но не цель

        Gizmos.DrawLine(origin, aimPoint);

        // Дополнительно для melee: линии к потенциальным целям
        if (config.attackType == EnemyConfig.EnemyAttackType.Melee)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (AttackRange * 0.5f), AttackRange, damageMask);
            foreach (var col in hits)
            {
                if (col.transform == transform) continue;
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, col.transform.position);
            }
        }
    }
}
