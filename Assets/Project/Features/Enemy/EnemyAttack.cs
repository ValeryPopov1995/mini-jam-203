using System.ComponentModel;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Component = UnityEngine.Component;

[RequireComponent(typeof(EnemyConfigHolder))]
public class EnemyAttack : MonoBehaviour
{
    EnemyConfig config;
    float lastAttackTime;
    [SerializeField] LayerMask lineOfSightMask = 6;
    [SerializeField] float eyeHeight = 1f;


    // For melee overlap check
    const int PLAYER_LAYER = 0; // если у вас игрок в отдельном слое Ч замените или используйте LayerMask
    LayerMask damageMask = ~0; // по умолчанию Ч все

    void Awake()
    {
        // config через holder (чтобы компонент можно было добавл€ть без последовательности)
        config = GetComponent<EnemyConfigHolder>().Config;
    }

    public float AttackRange => config != null ? config.attackRange : 1f;

    public void Init(EnemyConfig cfg)
    {
        config = cfg;
    }
    public bool CanAttack(float distanceToTarget)
    {
        return distanceToTarget <= AttackRange;
    }

    public void Tick(Transform target)
    {
        if (config == null || target == null) return;

        FaceTarget(target);

        // дл€ дальников Ч провер€ем линию видимости
        if (config.attackType == EnemyConfig.EnemyAttackType.Ranged)
        {
            if (!HasLineOfSight(target))
                return; // Ќ≈ атакуем, если есть стена
        }

        if (Time.time >= lastAttackTime + config.attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
    }

    public void Stop()
    {
        // можно сбросить триггеры анимаций
    }

    void PerformAttack(Transform target)
    {
        if (config.attackType == EnemyConfig.EnemyAttackType.Melee)
        {
            DoMeleeAttack();
        }
        else if (config.attackType == EnemyConfig.EnemyAttackType.Ranged)
        {
            DoRangedAttack(target);
        }
    }

    //если нет LOS Ч враг должен продолжать движение
    public bool HasLineOfSightTo(Transform target)
    {
        return HasLineOfSight(target);
    }

    public bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);

        Vector3 dir = aimPoint - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, lineOfSightMask))
        {
            // если первым попали не в цель Ч значит есть преп€тствие
            return false;
        }

        return true;
    }

    void DoMeleeAttack()
    {
        // ѕроста€ реализаци€: overlap sphere и наносим урон первому IDamageable
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (AttackRange * 0.5f), AttackRange, damageMask);
        foreach (var col in hits)
        {
            if (col.transform == transform) continue;
            var dmg = col.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(config.damage, gameObject);
                break;
            }
        }

        // ¬ызов анимации/эвентов
        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    Vector3 GetAimPoint(Transform target)
    {
        if (target.TryGetComponent<CapsuleCollider>(out var capsule))
            return capsule.bounds.center;

        if (target.TryGetComponent<Collider>(out var col))
            return col.bounds.center;

        return target.position;
    }

    void DoRangedAttack(Transform target)
    {

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(target);


        if (config.projectilePrefab == null)
        {
            Vector3 dir = (aimPoint - origin).normalized;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, AttackRange))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(config.damage, gameObject);
                }
            }
        }
        else
        {
            Vector3 spawnPos = transform.position + Vector3.up * eyeHeight;
            var proj = Instantiate(config.projectilePrefab, spawnPos, Quaternion.identity);

            if (proj != null)
            {

                proj.transform.LookAt(aimPoint);
                proj.Init(config.damage, config.projectileSpeed, gameObject);
            }
        }

        GetComponent<EnemyAnimator>()?.PlayAttack();
    }

    void FaceTarget(Transform target)
    {
        // ѕоворачиваем визуальную часть через EnemyAnimator/Visual
        var brain = GetComponent<EnemyBrain>();
        if (brain != null)
            brain.FaceVisualTowards(target);
    }

    void OnDrawGizmosSelected()
    {
        if (config == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (AttackRange * 0.5f), AttackRange);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        var enemyTarget = GetComponent<EnemyTarget>();
        if (enemyTarget == null || enemyTarget.Target == null)
            return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 aimPoint = GetAimPoint(enemyTarget.Target);

        Vector3 dir = aimPoint - origin;
        float dist = dir.magnitude;

        bool blocked = Physics.Raycast(
            origin,
            dir.normalized,
            dist,
            lineOfSightMask
        );

        Gizmos.color = blocked ? Color.red : Color.green;
        Gizmos.DrawLine(origin, aimPoint);
    }
}