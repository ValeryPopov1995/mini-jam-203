using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavAgent : MonoBehaviour
{
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // ћы управл€ем поворотом вручную через Visual
        agent.updateRotation = false;
        agent.updateUpAxis = true;
    }

    public void ApplyConfig(EnemyConfig config)
    {
        agent.speed = config.moveSpeed;
        agent.stoppingDistance = config.stoppingDistance;
        // другие параметры (acceleration, angularSpeed) можно настроить тут при необходимости
    }

    public void MoveTo(Vector3 position)
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public void Stop()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (agent == null) return true;
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
    }

    // expose remaining distance if needed
    public float RemainingDistance => agent != null ? agent.remainingDistance : 0f;
}