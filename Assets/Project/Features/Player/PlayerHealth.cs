using System;
using System.Collections;
using UnityEngine;

namespace MiniJam203.Player
{
    // Интерфейс, который должен реализовывать любой объект, получающий урон.
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        public event Action<float> OnHealthChanged;
        public event Action<GameObject> OnDied;

        private float currentHealth;
        private bool isDead;
        private bool isInvulnerable;
        private Coroutine invulCoroutine;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(maxHealth, 0f, float.MaxValue);
            isDead = false;
            isInvulnerable = false;
            OnHealthChanged?.Invoke(currentHealth);
        }

        // Реализация интерфейса
        public void TakeDamage(float damage, GameObject attacker)
        {
            if (isDead) return;
            if (damage <= 0f) return;
            if (isInvulnerable) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f)
            {
                Die(attacker);
            }
            else if (invulnerabilityDuration > 0f)
            {
                if (invulCoroutine != null) StopCoroutine(invulCoroutine);
                invulCoroutine = StartCoroutine(TemporaryInvulnerability(invulnerabilityDuration));
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            if (amount <= 0f) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }

        private void Die(GameObject attacker)
        {
            if (isDead) return;
            isDead = true;

            // Можно добавить отключение управления, проигрывание анимации и т.д.
            Debug.Log($"{gameObject.name} died. Killer: {(attacker != null ? attacker.name : "Unknown")}");

            OnDied?.Invoke(attacker);
        }

        private IEnumerator TemporaryInvulnerability(float duration)
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
            invulCoroutine = null;
        }
    }
}