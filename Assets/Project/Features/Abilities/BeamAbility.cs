using MiniJam203.Player;
using Project.Features.Abilities;
using System.Collections;
using UnityEngine;

public class BeamAbility : Ability
{
    [SerializeField] private PlayerHealth _player;
    [Header("Laser Settings")]
    [SerializeField] private float laserDuration = 1.5f;      
    [SerializeField] private float damagePerSecond = 50f;     
    [SerializeField] private float maxRange = 100f;          
    [SerializeField] private float recoilDamage = 1f;           

    [Header("Visual")]
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Material laserMaterial;          
    [SerializeField] private LayerMask hitLayers = -1;        

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticles;     
    [SerializeField] private AudioSource laserSound;

    private Coroutine laserCoroutine;

    public override void Activate()
    {
        if (laserCoroutine != null) return; 

        laserCoroutine = StartCoroutine(LaserBeam());
    }

    private IEnumerator LaserBeam()
    {
        // Визуал setup
        SetupLaserVisual();
        PlayLaserSound();

        float damagePerFrame = damagePerSecond * Time.deltaTime;
        float elapsed = 0f;

        while (elapsed < laserDuration)
        {
            
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers))
            {
                UpdateLaserLine(cam.transform.position, hit.point);

                
                DamageInBeam(hit.point, 0.3f, damagePerFrame);

                
                if (hitParticles != null)
                {
                    hitParticles.transform.position = hit.point;
                    hitParticles.Play();
                }
            }
            else
            {
                
                Vector3 endPos = cam.transform.position + cam.transform.forward * maxRange;
                UpdateLaserLine(cam.transform.position, endPos);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _player.TakeDamage(recoilDamage,gameObject);
        // Cleanup
        CleanupLaser();
        laserCoroutine = null;

        Debug.Log($"<color=cyan>LaserAbility</color>: Луч длился {laserDuration}s, DPS: {damagePerSecond}");
    }

    private void SetupLaserVisual()
    {
        if (laserLine == null)
        {
            GameObject lineObj = new GameObject("LaserLine");
            lineObj.transform.SetParent(transform);
            laserLine = lineObj.AddComponent<LineRenderer>();
        }

        laserLine.positionCount = 2;
        laserLine.startWidth = 0.1f;
        laserLine.endWidth = 0.05f;
        laserLine.material = laserMaterial ?? new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = Color.red;
        laserLine.endColor = Color.yellow;
        laserLine.enabled = true;
    }

    private void UpdateLaserLine(Vector3 start, Vector3 end)
    {
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
    }

    private void DamageInBeam(Vector3 hitPoint, float beamRadius, float damage)
    {
        Collider[] hits = Physics.OverlapSphere(hitPoint, beamRadius, hitLayers);
        foreach (var col in hits)
        {
            if (col.attachedRigidbody.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
            }
        }
    }

    private void PlayLaserSound()
    {
        if (laserSound != null)
            laserSound.Play();
    }

    private void CleanupLaser()
    {
        if (laserLine != null)
            laserLine.enabled = false;
        if (laserSound != null)
            laserSound.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = Camera.main ? Camera.main.transform.position : transform.position;
        Vector3 dir = Camera.main ? Camera.main.transform.forward : transform.forward;
        Gizmos.DrawRay(origin, dir * maxRange);
    }
}
