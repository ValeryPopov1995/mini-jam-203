using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    public Transform Target { get; private set; }

    void Awake()
    {
        // По умолчанию — первый объект с тегом Player
        var p = GameObject.FindWithTag("Player");
        if (p != null) Target = p.transform;
    }

    public void SetTarget(Transform t) => Target = t;
}