using UnityEngine;

public class PointerCast : MonoBehaviour
{
    public Transform mouseObject;
    
    public static Vector2 GetArrowKeyVector()
    {
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) input.y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) input.y -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) input.x += 1;
        if (Input.GetKey(KeyCode.LeftArrow)) input.x -= 1;
        return input;
    }
    
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

            DroneView.allDrones[0].MoveDirection(GetArrowKeyVector());
        }
    }
}
