using MiniJam203.Player;
using Project.Features.Abilities;
using System.Collections;
using UnityEngine;

public class BeamAbility : Ability
{
    private PlayerHealth _player;

    [Header("Laser Settings")]
    [SerializeField] private float laserDuration = 1.5f;
    [SerializeField] private float damagePerSecond = 50f;
    [SerializeField] private float maxRange = 100f;
    [SerializeField] private float recoilDamage = 1f;

    [Header("Visual")]
    [SerializeField] private GameObject laserMuzzlePrefab;
    [SerializeField] private LayerMask hitLayers = -1;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioSource laserSound;

    private GameObject muzzleInstance;
    private Coroutine laserCoroutine;

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerHealth>();
    }

    public override void Activate()
    {
        if (laserCoroutine != null) return;
        laserCoroutine = StartCoroutine(LaserBeam());
    }

    private IEnumerator LaserBeam()
    {
        Camera cam = Camera.main;

        SpawnMuzzleFlash(cam.transform.position);
        PlayLaserSound();

        float damagePerFrame = damagePerSecond * Time.deltaTime;
        float elapsed = 0f;

        while (elapsed < laserDuration)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers))
            {
                UpdateMuzzleDirection(ray.origin, hit.point);

                DamageInBeam(hit.point, 0.3f, damagePerFrame);

                if (hitParticles != null)
                {
                    hitParticles.transform.position = hit.point;
                    hitParticles.Play();
                }
            }
            else
            {
                Vector3 endPos = ray.origin + ray.direction * maxRange;
                UpdateMuzzleDirection(ray.origin, endPos);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _player.TakeDamage(recoilDamage, gameObject);
        CleanupLaser();
        laserCoroutine = null;
    }

    private void SpawnMuzzleFlash(Vector3 muzzlePos)
    {
        if (laserMuzzlePrefab == null) return;

        muzzleInstance = Instantiate(laserMuzzlePrefab, muzzlePos, Quaternion.identity);
        muzzleInstance.transform.SetParent(transform);
    }

    private void UpdateMuzzleDirection(Vector3 start, Vector3 end)
    {
        if (muzzleInstance == null) return;

        muzzleInstance.transform.position = start;
        Vector3 direction = (end - start).normalized;
        muzzleInstance.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void DamageInBeam(Vector3 hitPoint, float beamRadius, float damage)
    {
        Collider[] hits = Physics.OverlapSphere(hitPoint, beamRadius, hitLayers);
        foreach (var col in hits)
        {
            try
            {
                IDamageable target = col.attachedRigidbody.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(damage);
            }
            catch { }
        }
    }

    private void PlayLaserSound()
    {
        if (laserSound != null) laserSound.Play();
    }

    private void CleanupLaser()
    {
        if (muzzleInstance != null)
        {
            Destroy(muzzleInstance.gameObject);
            muzzleInstance = null;
        }
        if (laserSound != null) laserSound.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = Camera.main ? Camera.main.transform.position : transform.position;
        Vector3 dir = Camera.main ? Camera.main.transform.forward : transform.forward;
        Gizmos.DrawRay(origin, dir * maxRange);
    }
}
