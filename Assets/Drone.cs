using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drone : MonoBehaviour
{
    public static List<Drone> allDrones = new List<Drone>();
    Vector2Int currentTilePosition;
    public float speed = 5f;
    public float height = 5f;

    void Start()
    {
        allDrones.Add(this);
    }
    public void GoToPosition(Vector3 targetPosition)
    {
        targetPosition.y = height;
        transform.DOMove(targetPosition, (transform.position - targetPosition).magnitude / speed);
    }
}
