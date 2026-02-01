using Project.Features.Abilities;
using UnityEngine;

public class GrenadeAbility : Ability
{
    [Header("Grenade Throw")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float upForce = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionDamage = 80f;
    [SerializeField] private float pushStrength = 15f;  

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;

    public override void Activate()
    {
        Camera cam = Camera.main;
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 1f;

        GameObject grenade = Instantiate(grenadePrefab, spawnPos, cam.transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 throwDir = cam.transform.forward * throwForce + Vector3.up * upForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
        }

        Grenade explode = grenade.GetComponent<Grenade>() ?? grenade.AddComponent<Grenade>();
        explode.Setup(explosionRadius, explosionDamage, pushStrength, knockbackDuration, explosionEffect);
    }
}
