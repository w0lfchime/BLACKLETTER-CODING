using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


namespace DroneSpace
{
    public class GridView : MonoBehaviour
    {
        public static GridView instance;
        public List<GameObject>[,] gridArray;

        private GameObject[,] tileArray;
        [Range(1f, 20f)]
        public int setSize;
        [HideInInspector]
        public int size;
        public float positionMultiplier = 1;
        public GameObject tilePrefab;
        public GameObject dronePrefab;
        public GameObject hubPrefab;
        public Transform TilesParent;

    
        void OnEnable()
        {
            instance = this;
            size = setSize*2+1;
        }
    
        void Start()
        {
            size = setSize*2+1;
            if (gridArray != null) return; // Already initialized, skip
        
            gridArray = new List<GameObject>[size, size];
            tileArray = new GameObject[size, size];
        
            // Initialize all lists in the grid
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    gridArray[x, z] = new List<GameObject>();
                    tileArray[x, z].transform.parent = TilesParent;
                }
            }
            int middle = (int)(size / 2);
            Spawn(dronePrefab , new Vector3Int(middle, 0, middle), 1);
            Spawn(hubPrefab , new Vector3Int(middle, 0, middle));
        }

        void FixedUpdate()
        {
            size = setSize*2+1;
            if(gridArray.GetLength(0) != size)
            {
                // Destroy all old tiles
                foreach(var tile in tileArray)
                    if(tile != null) tile.transform.DOScale(0, .5f).SetEase(Ease.InBack).OnComplete(() => Destroy(tile));
                
                // Recreate grid from scratch
                gridArray = new List<GameObject>[size, size];
                tileArray = new GameObject[size, size];
                
                for(int x = 0; x < size; x++)
                {
                    for(int z = 0; z < size; z++)
                    {
                        gridArray[x, z] = new List<GameObject>();
                        tileArray[x, z] = Instantiate(tilePrefab, GridToWorld(new Vector3Int(x, 0, z)), Quaternion.identity);
                        tileArray[x, z].transform.parent = TilesParent;
                        tileArray[x, z].transform.DOScale(Vector3.zero, 1).From().SetEase(Ease.OutBounce);
                    }
                }

                RemoveObject(DroneView.allDrones[0].gameObject, DroneView.allDrones[0].GetComponent<GridObject>().currentTilePosition);
                DroneView.allDrones[0].GoToPosition(new Vector3Int(size/2,0,size/2), true);
            }
        }

        public Vector3Int WorldToGrid(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / positionMultiplier) + size / 2;
            int z = Mathf.RoundToInt(position.z / positionMultiplier) + size / 2;
        
            return loopGridPosition(new Vector3Int(x, 0, z));
        }

        public Vector3Int loopGridPosition(Vector3Int gridPos)
        {
            int x = ((gridPos.x % size) + size) % size;
            int z = ((gridPos.z % size) + size) % size;
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
            var gridObject = obj.GetComponent<GridObject>();
            if(gridObject != null)
                gridObject.currentTilePosition = position;
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
