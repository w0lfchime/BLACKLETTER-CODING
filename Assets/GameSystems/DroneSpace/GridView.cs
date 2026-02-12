using System.Collections.Generic;
using UnityEngine;

namespace DroneSpace
{
    public sealed class GridView : MonoBehaviour
    {
        public static GridView I { get; private set; }

        [Header("Tile Visuals")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tilesParent;
        [SerializeField] private float positionMultiplier = 1f;

        [Header("Runtime (View-layer occupancy)")]
        public List<GameObject>[,] gridArray;      // objects on each tile (view-side)
        private GameObject[,] tileArray;           // tile visuals

        public int SizeX => Grid.I != null ? Grid.I.Width : 0;
        public int SizeZ => Grid.I != null ? Grid.I.Height : 0; // backend Height maps to view Z

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

        private void Start()
        {
            if (Grid.I == null || !Grid.I.IsReady)
            {
                Debug.LogError("GridView requires Grid.I to exist and be initialized before GridView.Start().");
                return;
            }

            Rebuild(force: true);
        }

        /// <summary>
        /// Rebuilds tile visuals + occupancy arrays to match the backend Grid.
        /// Call this after Grid.Rebuild/Init.
        /// </summary>
        public void Rebuild(bool force = false)
        {
            if (Grid.I == null || !Grid.I.IsReady)
            {
                Debug.LogError("Cannot build GridView: backend Grid is missing or not ready.");
                return;
            }

            int sx = Grid.I.Width;
            int sz = Grid.I.Height;

            if (!force && tileArray != null && tileArray.GetLength(0) == sx && tileArray.GetLength(1) == sz)
                return;

            // Destroy old visuals
            if (tileArray != null)
            {
                foreach (var go in tileArray)
                    if (go != null) Destroy(go);
            }

            // Allocate new arrays
            gridArray = new List<GameObject>[sx, sz];
            tileArray = new GameObject[sx, sz];

            for (int x = 0; x < sx; x++)
            {
                for (int z = 0; z < sz; z++)
                {
                    gridArray[x, z] = new List<GameObject>();

                    if (tilePrefab != null)
                    {
                        var tileGO = Instantiate(tilePrefab, GridToWorld(new Vector3Int(x, 0, z)), Quaternion.identity);
                        if (tilesParent != null) tileGO.transform.SetParent(tilesParent, worldPositionStays: true);
                        tileArray[x, z] = tileGO;
                    }
                }
            }
        }

        // --- Coordinate conversions (view works in XZ, backend works in XY) ---

        public Vector3Int WorldToGrid(Vector3 position)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3Int.zero;

            int x = Mathf.RoundToInt(position.x / positionMultiplier) + sx / 2;
            int z = Mathf.RoundToInt(position.z / positionMultiplier) + sz / 2;

            return LoopGridPosition(new Vector3Int(x, 0, z));
        }

        public Vector3Int LoopGridPosition(Vector3Int gridPos)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3Int.zero;

            int x = ((gridPos.x % sx) + sx) % sx;
            int z = ((gridPos.z % sz) + sz) % sz;
            return new Vector3Int(x, 0, z);
        }

        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            int sx = SizeX;
            int sz = SizeZ;
            if (sx <= 0 || sz <= 0) return Vector3.zero;

            float x = (gridPos.x - sx / 2) * positionMultiplier;
            float z = (gridPos.z - sz / 2) * positionMultiplier;
            return new Vector3(x, 0f, z);
        }

        // --- Spawning / occupancy ---

        public bool Spawn(GameObject prefab, Vector3Int position, float heightOffset = 0f)
        {
            if (prefab == null) return false;

            position = LoopGridPosition(position);
            var objIns = Instantiate(prefab, GridToWorld(position) + Vector3.up * heightOffset, Quaternion.identity);
            AddObject(objIns, position);
            return true;
        }

        public bool RemoveObject(GameObject obj, Vector3Int position)
        {
            if (obj == null || gridArray == null) return false;

            position = LoopGridPosition(position);
            if (!InBoundsView(position)) return false;

            return gridArray[position.x, position.z].Remove(obj);
        }

        public bool AddObject(GameObject obj, Vector3Int position)
        {
            if (obj == null || gridArray == null) return false;

            position = LoopGridPosition(position);
            if (!InBoundsView(position)) return false;

            gridArray[position.x, position.z].Add(obj);

            var gridObject = obj.GetComponent<GridObject>();
            if (gridObject != null)
                gridObject.currentTilePosition = position;

            return true;
        }

        private bool InBoundsView(Vector3Int p)
        {
            int sx = SizeX;
            int sz = SizeZ;
            return (uint)p.x < (uint)sx && (uint)p.z < (uint)sz;
        }

        // --- Backend tile access using XZ coords (z -> y) ---

        public Tile GetBackendTile(Vector3Int gridPos)
        {
            if (Grid.I == null || !Grid.I.IsReady) return null;
            gridPos = LoopGridPosition(gridPos);
            return Grid.I.Get(gridPos.x, gridPos.z);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (Grid.I == null || !Grid.I.IsReady) return;

            int sx = Grid.I.Width;
            int sz = Grid.I.Height;

            Gizmos.color = Color.white;
            for (int x = 0; x < sx; x++)
                for (int z = 0; z < sz; z++)
                    Gizmos.DrawWireCube(GridToWorld(new Vector3Int(x, 0, z)), Vector3.one * positionMultiplier);
        }
    }
}
