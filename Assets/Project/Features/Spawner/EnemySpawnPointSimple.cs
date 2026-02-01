using System.Collections;
using UnityEngine;

public class EnemySpawnPointSimple : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] GameObject enemyPrefab;


    [Header("Spawn")]
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] float respawnDelay = -1f; // -1 = не респавнить
    [SerializeField] GameObject spawnFX;

    GameObject currentEnemy;

    void Start()
    {
        if (spawnOnStart)
            Spawn();
    }

    void Spawn()
    {
        if (currentEnemy != null)
            return;

        if (spawnFX != null)
            Instantiate(spawnFX, transform.position, Quaternion.identity);

        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);


        // подписка на смерть
        var health = currentEnemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath += OnEnemyDeath;
    }

    void OnEnemyDeath()
    {
        if (currentEnemy == null) return;

        var health = currentEnemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDeath -= OnEnemyDeath;

        //Destroy(currentEnemy);
        currentEnemy = null;

        if (respawnDelay > 0)
            StartCoroutine(CoSpawnDelay());
    }

    IEnumerator CoSpawnDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}