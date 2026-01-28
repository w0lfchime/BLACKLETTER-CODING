using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public static EnemyMovement instance;
    
    [System.Serializable]
    public class Enemy
    {
        public string name;
        public Vector3 position;
        public Vector3 velocity;
        public float speed = 5f;
        public float sphereRadius = 0.5f;
        public float separationRadius = 2f;
    }
    
    public List<Enemy> enemies = new List<Enemy>();
    public Transform target;
    public int spawnCount = 1;
    public float defaultSphereRadius = 0.5f;
    public float defaultSeparationRadius = 2f;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float separationWeight = 2f;
    public float seekWeight = 1f;
    public Color enemyColor = Color.red;
    
    private Mesh sphereMesh;
    private Material sphereMaterial;
    private int enemyCount = 0;
    
    // Spatial grid for fast neighbor lookup
    private Dictionary<int, List<Enemy>> spatialGrid = new Dictionary<int, List<Enemy>>();
    private float cellSize;
    
    void OnEnable()
    {
        instance = this;
        CreateSphereMesh();
    }
    
    int GetCellKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int z = Mathf.FloorToInt(pos.z / cellSize);
        return x * 73856093 ^ z * 19349663; // Hash
    }
    
    void RebuildSpatialGrid()
    {
        cellSize = defaultSeparationRadius;
        spatialGrid.Clear();
        
        foreach (Enemy enemy in enemies)
        {
            int key = GetCellKey(enemy.position);
            if (!spatialGrid.ContainsKey(key))
                spatialGrid[key] = new List<Enemy>();
            spatialGrid[key].Add(enemy);
        }
    }
    
    List<Enemy> GetNearbyEnemies(Enemy enemy)
    {
        List<Enemy> nearby = new List<Enemy>();
        int cx = Mathf.FloorToInt(enemy.position.x / cellSize);
        int cz = Mathf.FloorToInt(enemy.position.z / cellSize);
        
        // Check 3x3 grid of cells
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                int key = (cx + x) * 73856093 ^ (cz + z) * 19349663;
                if (spatialGrid.TryGetValue(key, out List<Enemy> cell))
                {
                    nearby.AddRange(cell);
                }
            }
        }
        return nearby;
    }
    
    void CreateSphereMesh()
    {
        sphereMesh = new Mesh();
        sphereMesh.name = "SphereMesh";
        
        int rings = 16;
        int segments = 32;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        for (int i = 0; i <= rings; i++)
        {
            float lat = Mathf.PI * i / rings;
            float sinLat = Mathf.Sin(lat);
            float cosLat = Mathf.Cos(lat);
            
            for (int j = 0; j <= segments; j++)
            {
                float lon = 2 * Mathf.PI * j / segments;
                float sinLon = Mathf.Sin(lon);
                float cosLon = Mathf.Cos(lon);
                
                vertices.Add(new Vector3(sinLat * cosLon, cosLat, sinLat * sinLon));
            }
        }
        
        for (int i = 0; i < rings; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int a = i * (segments + 1) + j;
                int b = a + segments + 1;
                
                triangles.Add(a);
                triangles.Add(a + 1);
                triangles.Add(b);
                
                triangles.Add(b);
                triangles.Add(a + 1);
                triangles.Add(b + 1);
            }
        }
        
        sphereMesh.vertices = vertices.ToArray();
        sphereMesh.triangles = triangles.ToArray();
        sphereMesh.RecalculateNormals();
        sphereMesh.RecalculateBounds();
        
        sphereMaterial = new Material(Shader.Find("Unlit/Color"));
        sphereMaterial.SetColor("_Color", enemyColor);
        sphereMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
    }
    
    void Update()
    {
        if (target == null)
            return;
        
        RebuildSpatialGrid();
            
        for (int i = 0; i < enemies.Count; i++)
        {
            ApplySteering(enemies[i]);
        }
        
        RenderEnemies();
    }
    
    void ApplySteering(Enemy enemy)
    {
        Vector3 separation = CalculateSeparation(enemy) * separationWeight;
        Vector3 seek = CalculateSeek(enemy, target.position) * seekWeight;
        
        Vector3 acceleration = separation + seek;
        acceleration.y = 0;
        
        // Limit force
        if (acceleration.magnitude > maxForce)
            acceleration = acceleration.normalized * maxForce;
        
        enemy.velocity += acceleration * Time.deltaTime;
        
        // Limit speed
        if (enemy.velocity.magnitude > maxSpeed)
            enemy.velocity = enemy.velocity.normalized * maxSpeed;
        
        enemy.position += enemy.velocity * Time.deltaTime;
    }
    
    Vector3 CalculateSeparation(Enemy enemy)
    {
        Vector3 steering = Vector3.zero;
        int count = 0;
        
        List<Enemy> nearby = GetNearbyEnemies(enemy);
        
        foreach (Enemy other in nearby)
        {
            if (other == enemy)
                continue;
            
            Vector3 diff = new Vector3(enemy.position.x - other.position.x, 0, enemy.position.z - other.position.z);
            float dist = diff.magnitude;
            
            if (dist < defaultSeparationRadius && dist > 0)
            {
                steering += diff.normalized / dist;
                count++;
            }
        }
        
        if (count > 0)
        {
            steering /= count;
            if (steering.magnitude > 0)
            {
                steering = steering.normalized * maxSpeed - enemy.velocity;
                if (steering.magnitude > maxForce)
                    steering = steering.normalized * maxForce;
            }
        }
        
        return steering;
    }
    
    Vector3 CalculateSeek(Enemy enemy, Vector3 targetPos)
    {
        Vector3 desired = new Vector3(targetPos.x - enemy.position.x, 0, targetPos.z - enemy.position.z);
        float dist = desired.magnitude;
        
        if (dist < 0.1f)
            return Vector3.zero;
        
        desired = desired.normalized * maxSpeed;
        Vector3 steering = desired - enemy.velocity;
        
        if (steering.magnitude > maxForce)
            steering = steering.normalized * maxForce;
        
        return steering;
    }
    
    void LateUpdate()
    {
    }
    
    void RenderEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(enemy.position, Quaternion.identity, Vector3.one * defaultSphereRadius * 2f);
            Graphics.DrawMesh(sphereMesh, matrix, sphereMaterial, 0);
        }
    }
    
    public void AddEnemy(string name, Vector3 position, float speed = 5f)
    {
        Enemy enemy = new Enemy();
        enemy.name = name;
        enemy.position = position;
        enemy.velocity = Vector3.zero;
        enemy.speed = speed;
        enemy.sphereRadius = defaultSphereRadius;
        enemy.separationRadius = defaultSeparationRadius;
        enemies.Add(enemy);
    }
    
    public void RemoveEnemy(int index)
    {
        if (index >= 0 && index < enemies.Count)
            enemies.RemoveAt(index);
    }
    
    [ContextMenu("Spawn Enemy")]
    public void SpawnEnemyPublic()
    {
        enemyCount++;
        Vector3 spawnPos = target != null ? target.position + Random.insideUnitSphere * 5f : Random.insideUnitSphere * 5f;
        spawnPos.y = 0;
        AddEnemy($"Enemy_{enemyCount}", spawnPos);
    }
}
