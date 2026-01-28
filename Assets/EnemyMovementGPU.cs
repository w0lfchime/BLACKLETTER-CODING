using UnityEngine;

public class EnemyMovementGPU : MonoBehaviour
{
    public static EnemyMovementGPU instance;
    
    struct EnemyGPU
    {
        public Vector3 position;
        public Vector3 velocity;
    }
    
    public ComputeShader computeShader;
    public Transform target;
    public int enemyCount = 1000;
    public float spawnRadius = 50f;
    public float defaultSphereRadius = 0.5f;
    public float defaultSeparationRadius = 2f;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float separationWeight = 2f;
    public float seekWeight = 1f;
    public Color enemyColor = Color.red;
    
    private ComputeBuffer enemyBuffer;
    private EnemyGPU[] enemies;
    private Material instancedMaterial;
    private Mesh sphereMesh;
    private int kernelID;
    private Bounds renderBounds;
    
    void OnEnable()
    {
        instance = this;
    }
    
    void Start()
    {
        if (computeShader == null)
        {
            Debug.LogError("EnemyMovementGPU: Assign the EnemySteering compute shader in the Inspector!");
            enabled = false;
            return;
        }
        
        InitializeEnemies();
        CreateSphereMesh();
        SetupMaterial();
        
        kernelID = computeShader.FindKernel("CSMain");
        renderBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    }
    
    void InitializeEnemies()
    {
        enemies = new EnemyGPU[enemyCount];
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 rand = Random.insideUnitCircle * spawnRadius;
            enemies[i] = new EnemyGPU
            {
                position = new Vector3(rand.x, 0, rand.y),
                velocity = Vector3.zero
            };
        }
        
        enemyBuffer = new ComputeBuffer(enemyCount, sizeof(float) * 6);
        enemyBuffer.SetData(enemies);
    }
    
    void CreateSphereMesh()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereMesh = sphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(sphere);
    }
    
    void SetupMaterial()
    {
        Shader shader = Shader.Find("Custom/EnemyInstanced");
        if (shader == null)
        {
            Debug.LogError("Could not find Custom/EnemyInstanced shader! Using fallback.");
            shader = Shader.Find("Unlit/Color");
        }
        instancedMaterial = new Material(shader);
        instancedMaterial.SetColor("_Color", enemyColor);
        instancedMaterial.SetBuffer("enemyBuffer", enemyBuffer);
        instancedMaterial.SetFloat("_SphereRadius", defaultSphereRadius);
        instancedMaterial.enableInstancing = true;
    }
    
    void Update()
    {
        if (target == null || computeShader == null)
            return;
        
        // Set compute shader parameters
        computeShader.SetBuffer(kernelID, "enemies", enemyBuffer);
        computeShader.SetVector("targetPosition", target.position);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("maxSpeed", maxSpeed);
        computeShader.SetFloat("maxForce", maxForce);
        computeShader.SetFloat("separationRadius", defaultSeparationRadius);
        computeShader.SetFloat("separationWeight", separationWeight);
        computeShader.SetFloat("seekWeight", seekWeight);
        computeShader.SetInt("enemyCount", enemyCount);
        
        // Dispatch
        int threadGroups = Mathf.CeilToInt(enemyCount / 256f);
        computeShader.Dispatch(kernelID, threadGroups, 1, 1);
        
        // Update material
        instancedMaterial.SetFloat("_SphereRadius", defaultSphereRadius);
        instancedMaterial.SetColor("_Color", enemyColor);
        
        // Render all instances
        Graphics.DrawMeshInstancedProcedural(sphereMesh, 0, instancedMaterial, renderBounds, enemyCount);
    }
    
    void OnDestroy()
    {
        enemyBuffer?.Release();
    }
    
    [ContextMenu("Respawn Enemies")]
    public void RespawnEnemies()
    {
        // Release old buffer
        enemyBuffer?.Release();
        
        // Create new array with current count
        enemies = new EnemyGPU[enemyCount];
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 rand = Random.insideUnitCircle * spawnRadius;
            enemies[i] = new EnemyGPU
            {
                position = new Vector3(rand.x, 0, rand.y),
                velocity = Vector3.zero
            };
        }
        
        enemyBuffer = new ComputeBuffer(enemyCount, sizeof(float) * 6);
        enemyBuffer.SetData(enemies);
        instancedMaterial.SetBuffer("enemyBuffer", enemyBuffer);
    }
}
