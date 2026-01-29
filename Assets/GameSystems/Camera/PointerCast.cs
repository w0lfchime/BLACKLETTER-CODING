using UnityEngine;

public class PointerCast : MonoBehaviour
{
    public Transform mouseObject;
    void LateUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            mouseObject.position = hit.point;

            if(Input.GetMouseButton(0))
            {
                DroneView.allDrones[0].GoToPosition(DroneSpace.Grid.instance.WorldToGrid(hit.point));
            }
        }
    }
}
