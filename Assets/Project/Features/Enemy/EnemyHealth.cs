using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float hp;
    public bool IsDead { get; private set; }
    EnemyConfig config;

    public event Action OnDeath;

    public void Init(EnemyConfig cfg)
    {
        config = cfg;
        hp = cfg != null ? cfg.maxHP : 100f;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (IsDead) return;

        hp -= amount;
        if (hp <= 0f)
        {
            Die();
        }
        else
        {
            // Здесь можно добавить реакции на урон: вспышка, звук, агро и т.д.
        }
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDeath?.Invoke();

        var animator = GetComponent<EnemyAnimator>();
        animator?.PlayDeath();

        float delay = config != null ? config.deathDestroyDelay : 1.5f;
        Destroy(gameObject, delay);
    }
}

public interface IDamageable
{
    void TakeDamage(float amount, GameObject attacker = null);
}