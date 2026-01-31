using UnityEngine;

public class EnemyConfigHolder : MonoBehaviour
{
    [SerializeField] EnemyConfig config;
    public EnemyConfig Config => config;

    public void SetConfig(EnemyConfig cfg)
    {
        config = cfg;
    }
}