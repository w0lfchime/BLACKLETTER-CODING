using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyMovementGPU))]
public class EnemyMovementGPUEditor : Editor
{
    private int tempSpawnCount = 1000;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EnemyMovementGPU enemyMovement = (EnemyMovementGPU)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Spawn Settings", EditorStyles.boldLabel);
        tempSpawnCount = EditorGUILayout.IntField("Enemy Count", tempSpawnCount);
        
        if (GUILayout.Button("Spawn Enemies", GUILayout.Height(40)))
        {
            enemyMovement.enemyCount = tempSpawnCount;
            if (Application.isPlaying)
            {
                enemyMovement.RespawnEnemies();
            }
        }
    }
}
