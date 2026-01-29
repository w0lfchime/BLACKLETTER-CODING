using UnityEngine;

public class TextIDE : MonoBehaviour
{
    public MeshFilter dragHandle;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverMesh())
            {
                isDragging = true;
                offset = transform.position - GetMouseWorldPosition();
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        
        if (isDragging)
        {
            Vector3 newPos = GetMouseWorldPosition() + offset;
            newPos.y = transform.position.y;
            transform.position = newPos;
        }
    }

    bool IsMouseOverMesh()
    {
        if (dragHandle == null) return false;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.GetComponent<MeshFilter>() == dragHandle;
        }
        return false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    void onclick()
    {
        Application.OpenURL("https://blacklettercoding.github.io/BLACKLETTER-CODING/TextIDE/");
    }
}
