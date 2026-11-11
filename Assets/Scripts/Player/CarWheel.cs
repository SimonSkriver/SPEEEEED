using UnityEngine;

public class CarWheel : MonoBehaviour
{
    [Header("Car Data (set manually, critical for functionality)")]
    [Tooltip("The specific ScriptableObject from which the data is read to calculate car behaviour")]
    public TestCarData CarData;

    [Header("Suspension settings (can be tweaked)")]
    [SerializeField] private float springRestLength = 1f;
    [SerializeField] private float springStrength = 8175f;
    [SerializeField] private float damperStrength = 3000;

    [Header("Wheel settings")]
    [Tooltip("Is this wheel steering the car (is it a front wheel?)")]
    [SerializeField] bool isSteering;
    [Tooltip("Does this wheel recieve engine torque")]
    [SerializeField] bool isMotorized;

    [Header("Components (set automatically")]
    private Rigidbody rb;
    private PlayerInputHandler playerInput;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerInputHandler>();

        if (CarData == null) Debug.LogWarning("No CarData set, no data is available");
        if (rb == null) Debug.LogWarning("No Rigidbody found, check parent");
        if (playerInput == null) Debug.LogWarning("No PlayerInputHandler found, check parent");
    }

    void FixedUpdate()
    {
        HandleSuspension();
        HandleTurningWheel();
        HandleAcceleration();
        HandleBrakingAndReversing();
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
            float wheelAngle = playerInput.SteerInputValue * CarData.SteeringRange; // Calculate wheel angle based on user input
            Quaternion targetRotation = Quaternion.Euler(0, wheelAngle, 0); // Calcalate rotation based on wheelAngle
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * 10f); // Lerp between current
            Debug.DrawRay(transform.position, transform.forward); // Debug
        }
    }

    void HandleAcceleration()
    {
        if (!isMotorized) return;
        if (Vector3.Dot(rb.transform.forward, rb.linearVelocity) >= CarData.TopSpeed) return; // If car speed is higher than car's top speed

        if (playerInput.AccelerationInputValue > 0)
        {
            float accelerationAmount = playerInput.AccelerationInputValue * CarData.EnginePower;
            rb.AddForceAtPosition(transform.forward * accelerationAmount, transform.position);
        }
    }

    void HandleBrakingAndReversing()
    {
        if (playerInput.BrakeInputValue < 0.1f) return; // If no user input

        if (Vector3.Dot(rb.transform.forward, rb.linearVelocity) > 0.1f) // If the car has forward velocity apply brakes, if 0 velocity apply reversing
        {
            float brakeAmount = playerInput.BrakeInputValue * CarData.BrakeForce;
            rb.AddForceAtPosition(-transform.forward * brakeAmount, transform.position);
        }
        else
        {
            if (Vector3.Dot(rb.transform.forward, rb.linearVelocity) >= CarData.ReversingMaxSpeed) return; // Only reverse when slower than max reverse speed
            if (isMotorized) // Only apply the reversing force on motorized wheels
            {
                float reverseAmount = CarData.EnginePower * playerInput.BrakeInputValue;
                rb.AddForceAtPosition(-transform.forward * reverseAmount, transform.position);
            }
        }
    }

    void HandleTracktion()
    {
        float lateralVelocity = Vector3.Dot(transform.right, rb.GetPointVelocity(transform.position));
        rb.AddForceAtPosition(-transform.right * lateralVelocity * CarData.TireGrip, transform.position);
    }
}