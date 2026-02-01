using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject
{
    public float maxHP;
    public float moveSpeed;
    public float attackRange;
    public float attackCooldown;
    public float stoppingDistance;
    public float deathDestroyDelay;
    public Projectile projectilePrefab;
    public float damage;
    public float projectileSpeed;
    public EnemyAttackType attackType;

    public enum EnemyAttackType
    {
        Melee,
        Ranged
    }
}