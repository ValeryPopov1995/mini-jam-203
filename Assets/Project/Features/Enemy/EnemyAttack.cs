using System.ComponentModel;
using UnityEngine;
using Component = UnityEngine.Component;

[RequireComponent(typeof(EnemyConfigHolder))]
public class EnemyAttack : MonoBehaviour
{
    EnemyConfig config;
    float lastAttackTime;

    // For melee overlap check
    const int PLAYER_LAYER = 0; // если у вас игрок в отдельном слое — замените или используйте LayerMask
    LayerMask damageMask = ~0; // по умолчанию — все

    void Awake()
    {
        // config через holder (чтобы компонент можно было добавлять без последовательности)
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

    void DoMeleeAttack()
    {
        // Простая реализация: overlap sphere и наносим урон первому IDamageable
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
            // пробуем PlayerHealth как запасной вариант (если в проекте нет интерфейса)
           // var ph = col.GetComponent<UnityEngine.Component>() as Component;
        }

        // Вызов анимации/эвентов
        var animator = GetComponent<EnemyAnimator>();
        animator?.PlayAttack();
    }

    void DoRangedAttack(Transform target)
    {
        if (config.projectilePrefab == null)
        {
            // простая рейкаст-имитация
            Vector3 dir = (target.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.0f, dir, out hit, AttackRange))
            {
                var dmgable = hit.collider.GetComponent<IDamageable>();
                if (dmgable != null)
                {
                    dmgable.TakeDamage(config.damage, gameObject);
                }
            }
        }
        else
        {
            // spawn projectile
            var prefab = config.projectilePrefab;
            var spawnPos = transform.position + (transform.forward * 1.0f) + Vector3.up * 1.1f;
            var proj = Instantiate(prefab, spawnPos, transform.rotation);
            if (proj != null)
            {
                Vector3 targetCenter = target.position + Vector3.up * 1.0f;
                proj.transform.LookAt(targetCenter);
                proj.Init(config.damage, config.projectileSpeed, gameObject);

            }
        }

        var animator = GetComponent<EnemyAnimator>();
        animator?.PlayAttack();
    }

    void FaceTarget(Transform target)
    {
        // Поворачиваем визуальную часть через EnemyAnimator/Visual
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
}