using UnityEngine;

namespace Project.Features.Abilities
{
    public class Arrow : MonoBehaviour
    {
        private float speed;
        private float lifeTime;
        private float damage;
        private GameObject hitEffectPrefab;
        
        public void Initialize(ArrowAbility ability, Vector3 direction, float arrowSpeed, float lifetime)
        {
            speed = arrowSpeed;
            lifeTime = lifetime;
            damage = ability.damage;
            hitEffectPrefab = ability.hitEffectPrefab;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.linearVelocity = direction * speed;
            Destroy(gameObject, lifeTime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Проверяем врагов
            if (collision.rigidbody.TryGetComponent<IDamageable>(out var damageable))
                DealDamage(damageable);

            SpawnHitEffect(collision.contacts[0].point);
            Destroy(gameObject);
        }
        
        private void DealDamage(IDamageable enemy)
        {
            enemy.TakeDamage(damage);
        }
        
        private void SpawnHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
            }
        }
    }
}