using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    float damage;
    GameObject owner;

    public void Init(float damageAmount, float speed, Vector3 dir, GameObject ownerObj)
    {
        damage = damageAmount;
        owner = ownerObj;

        rb = GetComponent<Rigidbody>();

        // Сначала поворачиваем
        transform.rotation = Quaternion.LookRotation(dir);

        // Затем задаём скорость
        rb.linearVelocity = dir * speed;

        // Игнорируем коллизии с владельцем
        var projCol = GetComponent<Collider>();
        foreach (var col in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(projCol, col);
        }

        Destroy(gameObject, 10f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;

        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage, owner);
        }

        Destroy(gameObject);
    }
}
