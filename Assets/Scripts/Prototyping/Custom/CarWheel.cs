using UnityEngine;

public class CarWheel : MonoBehaviour
{
    [Header("Car Data (IMPORTANT!)")]
    public TestCarData CarData;

    [Header("Suspension settings (can be tweaked)")]
    [SerializeField] private float springRestLength = 1f;
    [SerializeField] private float springStrength = 8175f;
    [SerializeField] private float damperStrength = 3000;

    [Header("Steering")]


    [Header("Acceleration, breaking and tracktion")]


    [Tooltip("Is this wheel steering the car (is it a front wheel?)")]
    [SerializeField] bool isSteering;
    [Tooltip("Does this wheel recieve engine torque")]
    [SerializeField] bool isMotorized;

    [Header("Components (set automatically")]
    private Rigidbody rb;
    private PlayerInputHandler playerInput;

    public float currentSpeed;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerInputHandler>();
    }

    void FixedUpdate()
    {
        HandleSuspension();
        HandleTurningWheel();
        HandleAcceleration();
        HandleTracktion();
    }

    void HandleSuspension()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, springRestLength))
        {
            float offset = springRestLength - hit.distance; // Calculate offset
            float springForce = offset * springStrength; // Calculate spring force
            float springVelocity = Vector3.Dot(transform.up, rb.GetPointVelocity(transform.position)); // Calculate spring velocity
            float damperForce = springVelocity * -damperStrength; // Calculate damping amount
            float totalForce = springForce + damperForce; // Calculate total force
            rb.AddForceAtPosition(transform.up * totalForce, transform.position); // Apply force
            Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.yellow); // Debug
        }
    }

    void HandleTurningWheel()
    {
        if (isSteering) // Only turn the wheels that are steering
        {
            float wheelAngle = playerInput.SteerValue.x * CarData.SteeringRange; // Calculate wheel angle based on user input
            Quaternion targetRotation = Quaternion.Euler(transform.localRotation.x, wheelAngle, transform.localRotation.z); // Calcalate rotation based on wheelAngle
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * 10f); // Lerp between curretn
            Debug.DrawRay(transform.position, transform.forward); // Debug
        }
            
    }

    void HandleAcceleration()
    {
        if (!isMotorized) return;
        currentSpeed = Vector3.Dot(rb.transform.forward, rb.linearVelocity);
        if (currentSpeed >= CarData.TopSpeed) return;

        if (playerInput.AccelerationValue > 0f)
        {
            float accelerationAmount = playerInput.AccelerationValue * CarData.EnginePower;
            rb.AddForceAtPosition(transform.forward * accelerationAmount, transform.position);
        }
    }

    void HandleTracktion()
    {
        float lateralVelocity = Vector3.Dot(transform.right, rb.GetPointVelocity(transform.position));
        rb.AddForceAtPosition(-transform.right * lateralVelocity * CarData.TireGrip, transform.position);
    }
}