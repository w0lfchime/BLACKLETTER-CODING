using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyMovement))]
public class EnemyMovementEditor : Editor
{
    private int tempSpawnCount = 1;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EnemyMovement enemyMovement = (EnemyMovement)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Spawn Settings", EditorStyles.boldLabel);
        tempSpawnCount = EditorGUILayout.IntField("Number to Spawn", tempSpawnCount);
        
        if (GUILayout.Button("Spawn Enemies", GUILayout.Height(40)))
        {
            for (int i = 0; i < tempSpawnCount; i++)
            {
                enemyMovement.SpawnEnemyPublic();
            }
        }
    }
}
