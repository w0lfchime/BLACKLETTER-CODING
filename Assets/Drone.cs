using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drone : GridObject
{
    public static List<Drone> allDrones = new List<Drone>();
    public float speed = 5f;
    public float height = 5f;
    public float swayAmount = 0.3f;
    public float swaySpeed = 1f;
    
    public Transform swayTransform;
    private Vector3 swayBasePosition;
    private float noiseOffset;
    private bool initialized = false;

    void OnEnable()
    {
        if (!allDrones.Contains(this))
            allDrones.Add(this);
    }
    
    void OnDisable()
    {
        allDrones.Remove(this);
    }

    void Start()
    {
        if (initialized) return;
        initialized = true;
        
        if (swayTransform == null)
            swayTransform = transform;
            
        swayBasePosition = swayTransform.localPosition;
        noiseOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        // Apply sway to child only
        float swayX = Mathf.PerlinNoise(Time.time * swaySpeed + noiseOffset, 0f) - 0.5f;
        float swayZ = Mathf.PerlinNoise(Time.time * swaySpeed + noiseOffset + 100f, 0f) - 0.5f;
        
        Vector3 swayOffset = new Vector3(swayX, 0f, swayZ) * swayAmount;
        swayTransform.localPosition = swayBasePosition + swayOffset;
    }

    public void GoToPosition(Vector3Int targetPosition)
    {
        Vector3 worldTargetPosition = new Vector3(targetPosition.x * Grid.instance.positionMultiplier, height, targetPosition.z * Grid.instance.positionMultiplier);
        float time = (transform.position - worldTargetPosition).magnitude / speed;

        Grid.instance.RemoveObject(gameObject, currentTilePosition);
        
        transform.DOMove(worldTargetPosition, time)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                Grid.instance.AddObject(gameObject, targetPosition);
            });
    }
}

