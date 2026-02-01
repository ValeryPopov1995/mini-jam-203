using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySpawnPointSimple))]
public class EnemySpawnPointSimpleEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemySpawnPointSimple spawner = (EnemySpawnPointSimple)target;

        GUILayout.Space(10);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Spawn Enemy"))
        {
            spawner.Spawn();
        }

        if (GUILayout.Button("Enable Spawner"))
        {
            spawner.EnableSpawner();
        }

        if (GUILayout.Button("Disable Spawner"))
        {
            spawner.DisableSpawner();
        }

        if (GUILayout.Button("Clear All Spawned Enemies"))
        {
            spawner.ClearAllSpawnedEnemies();
        }
    }
}
