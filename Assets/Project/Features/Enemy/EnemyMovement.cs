using UnityEngine;

[RequireComponent(typeof(EnemyNavAgent))]
public class EnemyMovement : MonoBehaviour
{
    EnemyNavAgent navAgent;

    void Awake()
    {
        navAgent = GetComponent<EnemyNavAgent>();
    }

    public void Chase(Vector3 worldTargetPosition)
    {
        navAgent.MoveTo(worldTargetPosition);
    }

    public void Stop()
    {
        navAgent.Stop();
    }

    public bool HasReachedDestination() => navAgent.HasReachedDestination();
}