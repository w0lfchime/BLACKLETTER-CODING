using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Grid : MonoBehaviour
{
    public static Grid instance;
    public List<GameObject>[,] gridArray;
    public int size;
    public float positionMultiplier = 1;
    public GameObject dronePrefab;
    
    void OnEnable()
    {
        instance = this;
    }
    
    void Start()
    {
        if (gridArray != null) return; // Already initialized, skip
        
        gridArray = new List<GameObject>[size, size];
        
        // Initialize all lists in the grid
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                gridArray[x, z] = new List<GameObject>();
            }
        }

        Spawn(dronePrefab , new Vector3Int(size / 2, 0, size / 2));
    }

    public Vector3Int WorldToGrid(Vector3 position)
    {
        Vector3Int gridPosition = Vector3Int.RoundToInt(position);
        gridPosition.y = 0;
        return gridPosition;
    }

    public bool Spawn(GameObject obj, Vector3Int position)
    {
        GameObject objIns = Instantiate(obj, (Vector3)position * positionMultiplier, Quaternion.identity);
        AddObject(objIns, position);
        return true;
    }

    public bool RemoveObject(GameObject obj, Vector3Int position)
    {
        gridArray[position.x, position.z].Remove(obj);
        return true;
    }

    public bool AddObject(GameObject obj, Vector3Int position)
    {
        gridArray[position.x, position.z].Add(obj);
        obj.GetComponent<GridObject>().currentTilePosition = position;
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
