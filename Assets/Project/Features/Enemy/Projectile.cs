using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    float damage;
    float speed;
    GameObject owner;

    public void Init(float damageAmount, float velocity, GameObject ownerObj)
    {
        damage = damageAmount;
        speed = velocity;
        owner = ownerObj;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        Destroy(gameObject, 6f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        var dmg = collider.GetComponent<IDamageable>();
        if (dmg != null && collider.gameObject != owner)
        {
            dmg.TakeDamage(damage, owner);
        }

        // Можно тут воспроизводить VFX / звук
        Destroy(gameObject);
    }


    void OnCollisionEnter(Collision collision)
    {
        var dmg = collision.collider.GetComponent<IDamageable>();
        if (dmg != null && collision.gameObject != owner)
        {
            dmg.TakeDamage(damage, owner);
        }

        // Можно тут воспроизводить VFX / звук
        Destroy(gameObject);
    }
}
