using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed = 5f;
    public float scrollspeed = 5;
    public float acceleration = 15f;
    public Vector2 heightLimit;
    
    private Vector3 currentVelocity = Vector3.zero;
    
    void OnEnable()
    {
        currentVelocity = Vector3.zero;
    }
    
    void Update()
    {
        // WASD only (not arrow keys)
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y += 1;
        if (Input.GetKey(KeyCode.S)) input.y -= 1;
        if (Input.GetKey(KeyCode.D)) input.x += 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y).normalized * speed;
        
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        bool atLowerLimit = transform.position.y <= heightLimit.x;
        bool atUpperLimit = transform.position.y >= heightLimit.y;
        
        if((atLowerLimit && scrollDelta >= 0) || (atUpperLimit && scrollDelta <= 0))
        {
            scrollDelta = 0;
            // Remove the scroll-induced velocity (along transform.forward), keep perpendicular velocity
            currentVelocity -= Vector3.Project(currentVelocity, transform.forward);
        }
        
        if(scrollDelta != 0)
        {
            targetVelocity += transform.forward * scrollDelta * scrollspeed * 1000;
        }
                
        // Smooth acceleration/deceleration
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        
        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
        
        // Clamp camera height
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, heightLimit.x, heightLimit.y);
        transform.position = position;
    }
}
