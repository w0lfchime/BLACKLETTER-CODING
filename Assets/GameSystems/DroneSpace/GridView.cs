using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DroneSpace
{
    public sealed class GridView : MonoBehaviour
    {
        public static GridView I { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private float positionMultiplier = 1f;
        [SerializeField] private float tileScale = 1f;
        [SerializeField] private Vector3 tileRotation = Vector3.zero; // Euler angles added to all tiles
        [SerializeField] private float animDuration = 0.5f;
        [SerializeField] private float waveDelay = 0.1f; // delay per unit distance from center
        [SerializeField] private Ease animEase = Ease.OutBack;

        [Header("Main Tiles")]
        [SerializeField] private Mesh tileMesh;
        [SerializeField] private Material[] tileMaterials; // Must have GPU Instancing enabled!

        [Header("Wall Tiles (edges)")]
        [SerializeField] private Mesh wallMesh;
        [SerializeField] private Material[] wallMaterials;
        [SerializeField] private Vector2 wallOffset = Vector2.zero; // x=height, y=distance from grid

        [Header("Corner Tiles")]
        [SerializeField] private Mesh cornerMesh;
        [SerializeField] private Material[] cornerMaterials;
        [SerializeField] private Vector2 cornerOffset = Vector2.zero; // x=height, y=distance from grid

        [Header("Runtime (View-layer occupancy)")]
        public List<GameObject>[,] gridArray;      // objects on each tile (view-side)
        
        // GPU Instancing data
        private Matrix4x4[] tileMatrices, wallMatrices, cornerMatrices;
        private float[] tileAnim, wallAnim, cornerAnim;
        private Vector3[] tilePos;
        private int tileCount, wallCount, cornerCount, prevSx, prevSz;
        
        private MaterialPropertyBlock propertyBlock;

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

            propertyBlock = new MaterialPropertyBlock();
            Rebuild(force: true);
        }

        private void Update()
        {
            float dt = Time.deltaTime, speed = animDuration > 0 ? 1f / animDuration : 100f;
            Vector3 bs = Vector3.one * tileScale * positionMultiplier;
            Quaternion br = Quaternion.Euler(tileRotation);

            // Animate tiles
            if (tileMesh != null && tileMaterials?.Length > 0 && tileMatrices != null)
            {
                for (int i = 0; i < tileCount; i++)
                {
                    tileAnim[i] = Mathf.MoveTowards(tileAnim[i], 1f, dt * speed);
                    tileMatrices[i] = Matrix4x4.TRS(tilePos[i], br, bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(tileAnim[i]), animEase));
                }
                DrawInstanced(tileMesh, tileMaterials, tileMatrices, tileCount);
            }

            // Animate walls
            if (wallMesh != null && wallMaterials?.Length > 0 && wallMatrices != null)
            {
                UpdateWalls(dt, speed, bs, br);
                DrawInstanced(wallMesh, wallMaterials, wallMatrices, wallCount);
            }

            // Animate corners
            if (cornerMesh != null && cornerMaterials?.Length > 0 && cornerMatrices != null)
            {
                UpdateCorners(dt, speed, bs, br);
                DrawInstanced(cornerMesh, cornerMaterials, cornerMatrices, cornerCount);
            }
        }

        private void DrawInstanced(Mesh mesh, Material[] mats, Matrix4x4[] matrices, int count)
        {
            for (int sub = 0; sub < mesh.subMeshCount && sub < mats.Length; sub++)
                if (mats[sub] != null)
                    for (int i = 0; i < count; i += 1023)
                        Graphics.DrawMeshInstanced(mesh, sub, mats[sub], matrices, Mathf.Min(1023, count - i), propertyBlock);
        }

        public void Rebuild(bool force = false)
        {
            if (Grid.I == null || !Grid.I.IsReady) { Debug.LogError("Cannot build GridView: backend Grid is missing or not ready."); return; }

            int sx = Grid.I.Width, sz = Grid.I.Height, n = sx * sz;
            if (!force && tileMatrices != null && tileCount == n && prevSx == sx && prevSz == sz) return;

            bool sizeChanged = prevSx != sx || prevSz != sz;
            Vector3 sv = Vector3.one * tileScale * positionMultiplier;
            Quaternion br = Quaternion.Euler(tileRotation);

            // Main grid tiles - wave from center
            gridArray = new List<GameObject>[sx, sz];
            float[] oldAnim = tileAnim;
            tileMatrices = new Matrix4x4[n];
            tilePos = new Vector3[n];
            tileAnim = new float[n];
            tileCount = n;

            float cx = (sx - 1) / 2f, cz = (sz - 1) / 2f;
            float maxDist = Mathf.Sqrt(cx * cx + cz * cz);
            int idx = 0;
            for (int x = 0; x < sx; x++)
            {
                for (int z = 0; z < sz; z++)
                {
                    gridArray[x, z] = new List<GameObject>();
                    Vector3 p = GridToWorld(new Vector3Int(x, 0, z));
                    tilePos[idx] = p;
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));
                    float delay = dist * waveDelay / (animDuration > 0 ? animDuration : 1f);
                    tileAnim[idx] = (!sizeChanged && oldAnim != null && x < prevSx && z < prevSz) ? oldAnim[x * prevSz + z] : -delay;
                    tileMatrices[idx] = Matrix4x4.TRS(p, br, sv * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(tileAnim[idx]), animEase));
                    idx++;
                }
            }
            prevSx = sx; prevSz = sz;

            // Walls & Corners - continue wave outward
            int wn = 2 * (sx + sz);
            float[] oldWall = wallAnim, oldCorner = cornerAnim;
            wallMatrices = new Matrix4x4[wn]; wallAnim = new float[wn]; wallCount = wn;
            cornerMatrices = new Matrix4x4[4]; cornerAnim = new float[4]; cornerCount = 4;
            float edgeDelay = (maxDist + 1) * waveDelay / (animDuration > 0 ? animDuration : 1f);
            float cornerDelay = (maxDist + 2) * waveDelay / (animDuration > 0 ? animDuration : 1f);
            for (int i = 0; i < wn; i++) wallAnim[i] = (!sizeChanged && oldWall != null && i < oldWall.Length) ? oldWall[i] : -edgeDelay;
            for (int i = 0; i < 4; i++) cornerAnim[i] = (!sizeChanged && oldCorner != null && i < oldCorner.Length) ? oldCorner[i] : -cornerDelay;
        }

        private void UpdateWalls(float dt, float speed, Vector3 bs, Quaternion br)
        {
            int sx = SizeX, sz = SizeZ;
            float hx = sx / 2f, hz = sz / 2f, pm = positionMultiplier, wd = wallOffset.y;
            int w = 0;
            for (int i = 0; i < sx; i++)
            {
                float x = (i - hx + 0.5f) * pm;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3(x, wallOffset.x, (-hz - 0.5f - wd) * pm), br * Quaternion.Euler(0, 0, 180), bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(wallAnim[w - 1]), animEase));
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3(x, wallOffset.x, (hz + 0.5f + wd) * pm), br, bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(wallAnim[w - 1]), animEase));
            }
            for (int i = 0; i < sz; i++)
            {
                float z = (i - hz + 0.5f) * pm;
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3((-hx - 0.5f - wd) * pm, wallOffset.x, z), br * Quaternion.Euler(0, 0, -90), bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(wallAnim[w - 1]), animEase));
                wallAnim[w] = Mathf.MoveTowards(wallAnim[w], 1f, dt * speed);
                wallMatrices[w++] = Matrix4x4.TRS(new Vector3((hx + 0.5f + wd) * pm, wallOffset.x, z), br * Quaternion.Euler(0, 0, 90), bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(wallAnim[w - 1]), animEase));
            }
        }

        private void UpdateCorners(float dt, float speed, Vector3 bs, Quaternion br)
        {
            float hx = SizeX / 2f, hz = SizeZ / 2f, pm = positionMultiplier, cd = cornerOffset.y;
            float cx = (hx + 0.5f + cd) * pm, cz = (hz + 0.5f + cd) * pm;
            (Vector3 p, float r)[] c = { (new(-cx, cornerOffset.x, -cz), 180), (new(cx, cornerOffset.x, -cz), 90), (new(-cx, cornerOffset.x, cz), -90), (new(cx, cornerOffset.x, cz), 0) };
            for (int i = 0; i < 4; i++)
            {
                cornerAnim[i] = Mathf.MoveTowards(cornerAnim[i], 1f, dt * speed);
                cornerMatrices[i] = Matrix4x4.TRS(c[i].p, br * Quaternion.Euler(0, 0, c[i].r), bs * DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(cornerAnim[i]), animEase));
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
