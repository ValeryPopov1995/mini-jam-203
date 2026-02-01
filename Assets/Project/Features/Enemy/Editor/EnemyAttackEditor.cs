using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyAttack))]
public class EnemyAttackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyAttack enemyAttack = (EnemyAttack)target;

        // Получаем слой Enemy
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) // проверяем, что слой существует
        {
            // Проверяем, включён ли слой Enemy в damageMask
            if (((1 << enemyLayer) & enemyAttack.damageMask) != 0)
            {
                EditorGUILayout.HelpBox(
                    "Внимание! Слой Enemy включён в Damage Mask. " +
                    "Это значит, что враги могут атаковать друг друга. " +
                    "Лучше исключить слой Enemy из маски.",
                    MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Слой Enemy не найден. Создайте слой 'Enemy' и присвойте его всем врагам.",
                MessageType.Info);
        }
    }
}
