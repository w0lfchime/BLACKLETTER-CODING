using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace DroneSpace
{
    public class Grid : MonoBehaviour
    {
        public static Grid instance;
        public List<GameObject>[,] gridArray;
        public int size;
        public float positionMultiplier = 1;
        public GameObject tilePrefab;
        public GameObject dronePrefab;
        public GameObject hubPrefab;
    
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
                    Instantiate(tilePrefab, GridToWorld(new Vector3Int(x, 0, z)), Quaternion.identity);
                }
            }
            int middle = (int)(size / 2);
            Spawn(dronePrefab , new Vector3Int(middle, 0, middle), 1);
            Spawn(hubPrefab , new Vector3Int(middle, 0, middle));
        }

        public Vector3Int WorldToGrid(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / positionMultiplier) + size / 2;
            int z = Mathf.RoundToInt(position.z / positionMultiplier) + size / 2;
        
            x = ((x % size) + size) % size;
            z = ((z % size) + size) % size;

            return new Vector3Int(x, 0, z);
        }

        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            float x = (gridPos.x - size / 2) * positionMultiplier;
            float z = (gridPos.z - size / 2) * positionMultiplier;
            return new Vector3(x, 0, z);
        }

        public bool Spawn(GameObject obj, Vector3Int position, float heightOffset = 0)
        {
            heightOffset+=.5f;
            GameObject objIns = Instantiate(obj, GridToWorld(position)+Vector3.up * heightOffset, Quaternion.identity);
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
                    for (int z = 0; z < size; z++)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(GridToWorld(new Vector3Int(x, 0, z)), Vector3.one * positionMultiplier);
                    }
                }
            }
        }
    }

}
