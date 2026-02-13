using UnityEngine;

namespace DroneSpace
{
    public sealed class Tile
    {
        public readonly int x;
        public readonly int y;

        // fields (expand as needed)
        public byte type;   
        public byte flags; 

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
            type = 0;
            flags = 0;
        }

        public override string ToString() => $"Tile({x},{y}) type={type}";
    }


    public sealed class Grid : MonoBehaviour
    {
        public static Grid I { get; private set; }

        [Header("Grid Size (Backend)")]
        [SerializeField, Min(1)] private int width = 21;
        [SerializeField, Min(1)] private int height = 21;

        [Header("Init Behavior")]
        [Tooltip("If true, initializes (or rebuilds) the grid in Awake.")]
        [SerializeField] private bool initOnAwake = true;

        [Tooltip("If true, rebuilds the grid whenever width/height change in the inspector (Editor only).")]
        [SerializeField] private bool rebuildOnValidate = true;

        public int Width => width;
        public int Height => height;

        [SerializeField, HideInInspector] private Tile[] tiles;

        public bool IsReady => tiles != null && tiles.Length == width * height;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;

            if (initOnAwake)
                Rebuild(width, height);
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            if (rebuildOnValidate)
            {
                // In edit mode, keep it synced for debugging.
                Rebuild(width, height);
                GridView.I?.Rebuild(force: false);
            }
        }
#endif

        /// <summary>
        /// Rebuilds the grid and recreates all tiles.
        /// </summary>
        public void Rebuild(int newWidth, int newHeight)
        {
            width = Mathf.Max(1, newWidth);
            height = Mathf.Max(1, newHeight);

            tiles = new Tile[width * height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    tiles[ToIndex(x, y)] = new Tile(x, y);
        }

        // Optional: keep a static-style API for convenience
        public static void Init(int newWidth, int newHeight, bool overwrite = true)
        {
            if (I == null)
            {
                Debug.LogError("No Grid instance in scene. Add a Grid component to a GameObject first.");
                return;
            }

            if (!overwrite && I.IsReady)
            {
                Debug.LogWarning("Grid.Init called but grid already exists (overwrite=false).");
                return;
            }

            I.Rebuild(newWidth, newHeight);
        }

        public int ToIndex(int x, int y) => x + y * width;

        public bool InBounds(int x, int y)
            => (uint)x < (uint)width && (uint)y < (uint)height;

        public Tile Get(int x, int y)
        {
            if (!IsReady) throw new System.InvalidOperationException("Grid not initialized. Call Rebuild/Init or enable initOnAwake.");
            if (!InBounds(x, y)) return null;
            return tiles[ToIndex(x, y)];
        }

        public void Set(int x, int y, Tile tile)
        {
            if (!IsReady) throw new System.InvalidOperationException("Grid not initialized. Call Rebuild/Init or enable initOnAwake.");
            if (!InBounds(x, y)) return;
            tiles[ToIndex(x, y)] = tile;
        }

        public Tile[] RawTiles => tiles;
    }
}
