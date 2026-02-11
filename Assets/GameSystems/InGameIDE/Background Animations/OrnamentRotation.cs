using UnityEngine;

[DisallowMultipleComponent]
public class OrnamentRotation : MonoBehaviour
{
    [Header("Base Rotation Speeds (degrees/sec)")]
    public Vector3 baseSpeed = new Vector3(30f, 45f, 60f);

    [Header("Oscillation")]
    public Vector3 oscillationAmplitude = new Vector3(20f, 25f, 30f);
    public Vector3 oscillationFrequency = new Vector3(0.6f, 0.8f, 1.1f);

    [Header("Noise")]
    [Tooltip("How fast the noise evolves over time")]
    public float noiseTimeScale = 0.25f;

    [Tooltip("How strongly noise affects rotation")]
    public float noiseStrength = 1.0f;

    [Header("Randomization")]
    [Tooltip("Random offset so multiple objects never sync")]
    public bool randomizeSeed = true;

    private Vector3 noiseSeed;

    void Awake()
    {
        if (randomizeSeed)
        {
            noiseSeed = new Vector3(
                Random.value * 1000f,
                Random.value * 1000f,
                Random.value * 1000f
            );
        }
    }

    void Update()
    {
        float t = Time.time;

        Vector3 rotationDelta = Vector3.zero;

        // X axis
        rotationDelta.x =
            baseSpeed.x +
            Mathf.Sin(t * oscillationFrequency.x) * oscillationAmplitude.x +
            Mathf.PerlinNoise(noiseSeed.x, t * noiseTimeScale) * oscillationAmplitude.x * noiseStrength;

        // Y axis
        rotationDelta.y =
            baseSpeed.y +
            Mathf.Sin(t * oscillationFrequency.y + 1.37f) * oscillationAmplitude.y +
            Mathf.PerlinNoise(noiseSeed.y, t * noiseTimeScale + 10f) * oscillationAmplitude.y * noiseStrength;

        // Z axis
        rotationDelta.z =
            baseSpeed.z +
            Mathf.Sin(t * oscillationFrequency.z + 2.91f) * oscillationAmplitude.z +
            Mathf.PerlinNoise(noiseSeed.z, t * noiseTimeScale + 20f) * oscillationAmplitude.z * noiseStrength;

        transform.Rotate(rotationDelta * Time.deltaTime, Space.Self);
    }
}

