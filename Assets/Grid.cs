using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid instance;
    public List<GameObject>[,] gridArray;
    public int size;
    public float positionMultiplier = 1;
    void Start()
    {
        gridArray = new List<GameObject>[size, size];

        instance = this;
    }

    public Vector3 GridToWorld(Vector3 position)
    {
        Vector3Int gridPosition = Vector3Int.RoundToInt(position);
        gridPosition.y = 0;
        return (Vector3)gridPosition * positionMultiplier;
    }

    public bool Spawn(GameObject obj, Vector2Int position)
    {
        Instantiate(obj, (Vector2)position * positionMultiplier, Quaternion.identity);
        return true;
    }

    void OnDrawGizmos()
    {
        if (gridArray != null)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(new Vector3(x * positionMultiplier, 0, y * positionMultiplier), Vector3.one * positionMultiplier);
                }
            }
        }
    }
}
