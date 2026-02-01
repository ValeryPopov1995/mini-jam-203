using UnityEngine;

[RequireComponent(typeof(EnemyConfigHolder))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyTarget))]
public class EnemyBrain : MonoBehaviour
{
    [SerializeField] EnemyConfig config; // назначьте конфиг в инспекторе
    [SerializeField] float hysteresis = 0.5f; // отступ для входа/выхода из атаки



    EnemyMovement movement;
    EnemyAttack attack;
    EnemyHealth health;
    EnemyTarget target;
    EnemyAnimator animator;
    EnemyConfigHolder configHolder;

    // Последняя видимая позиция игрока
    Vector3 lastSeenPosition;
    bool isAttacking;

    void Start()
    {
        movement = GetComponent<EnemyMovement>();
        attack = GetComponent<EnemyAttack>();
        health = GetComponent<EnemyHealth>();
        target = GetComponent<EnemyTarget>();
        animator = GetComponent<EnemyAnimator>();
        configHolder = GetComponent<EnemyConfigHolder>();

        // если конфиг задан тут — передаём в holder чтобы все компоненты могли получить его
        if (config != null)
        {
            configHolder.SetConfig(config);
        }

        // инициализация компонентов
        var cfg = configHolder.Config;
        if (cfg != null)
        {
            var nav = GetComponent<EnemyNavAgent>();
            nav?.ApplyConfig(cfg);

            var atk = GetComponent<EnemyAttack>();
            atk?.Init(cfg);

            health?.Init(cfg);
        }
    }
    void Update()
    {
        if (health == null || health.IsDead) return;
        if (target == null || target.Target == null) return;

        Vector3 playerPos = target.Target.position;

        float distance = Vector3.Distance(transform.position, playerPos);

        // гистерезис — предотвращаем фликеринг
        float enterRange = attack.AttackRange;
        float exitRange = attack.AttackRange + hysteresis;

        bool hasLOS = true;

        // Проверка видимости ТОЛЬКО для дальников
        if (configHolder.Config.attackType == EnemyConfig.EnemyAttackType.Ranged)
        {
            hasLOS = attack.HasLineOfSight(target.Target);
        }

        // ===== Обновляем последнюю видимую позицию =====
        if (hasLOS)
        {
            lastSeenPosition = playerPos;
        }

        // ===== ВХОД В АТАКУ =====
        if (!isAttacking && distance <= enterRange && hasLOS)
            isAttacking = true;
        else if (isAttacking && (distance >= exitRange || !hasLOS))
            isAttacking = false;

        // ===== ПОВЕДЕНИЕ =====
        if (isAttacking)
        {
            movement.Stop();
            attack.Tick(target.Target);
            animator?.SetMoving(false);
        }
        else
        {
            Vector3 destination = hasLOS ? playerPos : lastSeenPosition;
            movement.Chase(destination);
            attack.Stop();
            animator?.SetMoving(true);
            Debug.DrawLine(transform.position + Vector3.up, destination + Vector3.up, Color.cyan);
        }
    }



    // вспомогательный для attack (поворачивает визуал)
    public void FaceVisualTowards(Transform t)
    {
        animator?.FaceVisualTowards(t);
    }
}