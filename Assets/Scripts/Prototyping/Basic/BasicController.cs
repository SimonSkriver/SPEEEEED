using UnityEngine;

public class BasicController : MonoBehaviour
{

    // Settings
    public float MoveSpeed = 50;
    public float MaxSpeed = 15;
    public float Drag = 0.98f;
    public float SteerAngle = 20;
    public float Traction = 1;

    // Variables
    private Vector3 MoveForce;
    private PlayerInputHandler playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler>();
    }
    // Update is called once per frame
    void Update()
    {

        // Moving
        MoveForce += transform.forward * MoveSpeed * playerInput.AccelerationValue * Time.deltaTime;
        transform.position += MoveForce * Time.deltaTime;

        // Steering
        float steerInput = playerInput.SteerValue.x;
        transform.Rotate(Vector3.up * steerInput * MoveForce.magnitude * SteerAngle * Time.deltaTime);

        // Drag and max speed limit
        MoveForce *= Drag;
        MoveForce = Vector3.ClampMagnitude(MoveForce, MaxSpeed);

        // Traction
        Debug.DrawRay(transform.position, MoveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
        MoveForce = Vector3.Lerp(MoveForce.normalized, transform.forward, Traction * Time.deltaTime) * MoveForce.magnitude;
    }
}
