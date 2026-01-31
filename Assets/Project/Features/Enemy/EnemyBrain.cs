using UnityEngine;

[RequireComponent(typeof(EnemyConfigHolder))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyTarget))]
public class EnemyBrain : MonoBehaviour
{
    [SerializeField] EnemyConfig config; // назначьте конфиг в инспекторе
    [SerializeField] float hysteresis = 0.5f; // отступ дл€ входа/выхода из атаки

    EnemyMovement movement;
    EnemyAttack attack;
    EnemyHealth health;
    EnemyTarget target;
    EnemyAnimator animator;
    EnemyConfigHolder configHolder;

    bool isAttacking;

    void Start()
    {
        movement = GetComponent<EnemyMovement>();
        attack = GetComponent<EnemyAttack>();
        health = GetComponent<EnemyHealth>();
        target = GetComponent<EnemyTarget>();
        animator = GetComponent<EnemyAnimator>();
        configHolder = GetComponent<EnemyConfigHolder>();

        // если конфиг задан тут Ч передаЄм в holder чтобы все компоненты могли получить его
        if (config != null)
        {
            configHolder.SetConfig(config);
        }

        // инициализаци€ компонентов
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

        float distance = Vector3.Distance(transform.position, target.Target.position);

        // гистерезис Ч предотвращаем фликеринг
        float enterRange = attack.AttackRange;
        float exitRange = attack.AttackRange + hysteresis;

        if (!isAttacking && distance <= enterRange)
            isAttacking = true;
        else if (isAttacking && distance >= exitRange)
            isAttacking = false;

        if (isAttacking)
        {
            movement.Stop();
            attack.Tick(target.Target);
            animator?.SetMoving(false);
        }
        else
        {
            movement.Chase(target.Target.position);
            attack.Stop();
            animator?.SetMoving(true);
        }
    }

    // вспомогательный дл€ attack (поворачивает визуал)
    public void FaceVisualTowards(Transform t)
    {
        animator?.FaceVisualTowards(t);
    }
}