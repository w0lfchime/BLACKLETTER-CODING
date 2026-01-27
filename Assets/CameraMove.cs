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
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y).normalized * speed;
        
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if(transform.position.y==heightLimit.x && scrollDelta>=0) {scrollDelta=0; currentVelocity = Vector3.zero;}else
        if(transform.position.y==heightLimit.y && scrollDelta<=0) {scrollDelta=0; currentVelocity = Vector3.zero;}else{
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
