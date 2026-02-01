using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPointSimple : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] GameObject enemyPrefab;

    [Header("Spawn")]
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] float respawnDelay = -1f; // -1 = не респавнить
    [SerializeField] GameObject spawnFX;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool spawnerEnabled = true;
    private Coroutine respawnCoroutine;

    void Start()
    {
        if (spawnOnStart && spawnerEnabled)
            Spawn();
    }

    public void Spawn()
    {
        if (!spawnerEnabled) return;

        if (spawnFX != null)
            Instantiate(spawnFX, transform.position, Quaternion.identity);

        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        spawnedEnemies.Add(newEnemy);

        var health = newEnemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath += () => OnEnemyDeath(newEnemy);
    }

    void OnEnemyDeath(GameObject enemy)
    {
        if (enemy != null)
        {
            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
                health.OnDeath -= () => OnEnemyDeath(enemy);

            spawnedEnemies.Remove(enemy);
        }

        if (spawnerEnabled && respawnDelay > 0)
        {
            respawnCoroutine = StartCoroutine(CoSpawnDelay());
        }
    }

    IEnumerator CoSpawnDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn();
    }

    public void SetRespawnDelay(float delay)
    {
        respawnDelay = delay;
    }

    public void EnableSpawner()
    {
        spawnerEnabled = true;
        if (spawnedEnemies.Count == 0 && respawnDelay > 0)
        {
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);
            respawnCoroutine = StartCoroutine(CoSpawnDelay());
        }
    }

    public void DisableSpawner()
    {
        spawnerEnabled = false;
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    public void ClearAllSpawnedEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                var health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                    health.OnDeath -= () => OnEnemyDeath(enemy);

                DestroyImmediate(enemy);
            }
        }
        spawnedEnemies.Clear();

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        Debug.Log("Spawner cleared all spawned enemies (Editor).");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
