using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;

public class DroneView : GridObject
{
    public static List<DroneView> allDrones = new List<DroneView>();
    public float speed = 5f;
    public float height = 5f;
    public float swayAmount = 0.3f;
    public float swaySpeed = 1f;

    public Transform swayTransform;
    private Vector3 swayBasePosition;
    private float noiseOffset;
    private bool initialized = false;
    private bool moving;

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

    public void MoveDirection(Vector2 direction)
    {
        if(moving) return;
        GoToPosition(currentTilePosition + new Vector3Int((int)direction.x, 0, (int)direction.y));
    }

    public void GoToPosition(Vector3Int gridPosition)
    {
        // Calculate distance before wrapping so wrapping counts as intended distance
        float distance = (gridPosition - currentTilePosition).magnitude;
        
        gridPosition = DroneSpace.Grid.instance.loopGridPosition(gridPosition);
        Vector3 worldTargetPosition = DroneSpace.Grid.instance.GridToWorld(gridPosition);
        worldTargetPosition.y = height;

        float time = distance / speed;

        DroneSpace.Grid.instance.RemoveObject(gameObject, currentTilePosition);

        moving = true;

        transform.DOMove(worldTargetPosition, time)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                DroneSpace.Grid.instance.AddObject(gameObject, gridPosition);
                moving = false;
            });
    }
}