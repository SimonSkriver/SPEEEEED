using UnityEngine;

#region Engine Torque Enum
enum EnginePowerMode
{
    Lerp,
    Curve,
}
#endregion

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
    [SerializeField] float wheelRadius;
    [Tooltip("Is this wheel steering the car (is it a front wheel?)")]
    [SerializeField] bool isSteering;
    [Tooltip("Does this wheel recieve engine torque")]
    [SerializeField] bool isMotorized;

    [Header("Engine power mode")]
    [SerializeField] EnginePowerMode enginePowerMode;
    [Tooltip("Curve will only be used if mode 'Curve' is selected")]
    [SerializeField] AnimationCurve powerCurve;

    [Header("Raycast hit?")]
    [SerializeField] bool grounded;

    [Header("Components (set automatically")]
    private Rigidbody rb;
    private PlayerInputHandler playerInput;

    [Header("Internal values (are calculated by script)")]
    private RaycastHit hit;
    private float forwardSpeed;
    private float speedFactor; // Value of current speed against top speed (0-1)

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
        grounded = Physics.Raycast(transform.position, -transform.up, out hit, springRestLength);
        forwardSpeed = Vector3.Dot(rb.transform.forward, rb.linearVelocity);
        speedFactor = Mathf.InverseLerp(0, CarData.TopSpeed, forwardSpeed);

        HandleTurningWheel();
        HandleSuspension();
        HandleAcceleration();
        HandleBrakingAndReversing();
        HandleTraction();
    }

    void HandleSuspension()
    {
        if (!grounded) return;

        float offset = springRestLength - hit.distance; // Calculate offset
        float springForce = offset * springStrength; // Calculate spring force
        float springVelocity = Vector3.Dot(transform.up, rb.GetPointVelocity(transform.position)); // Calculate spring velocity
        float damperForce = springVelocity * -damperStrength; // Calculate damping amount
        float totalForce = springForce + damperForce; // Calculate total force
        rb.AddForceAtPosition(transform.up * totalForce, transform.position); // Apply force
        Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.yellow); // Debug
    }

    void HandleTurningWheel()
    {
        if (isSteering) // Only turn the wheels that are steering
        {
            float currentSteeringRange = Mathf.Lerp(CarData.SteeringRange, CarData.SteeringRangeAtTopSpeed, speedFactor); // Lerp between max-min range based on speed
            float wheelAngle = playerInput.SteerInputValue * currentSteeringRange; // Calculate wheel angle based on user input and speed
            Quaternion targetRotation = Quaternion.Euler(0, wheelAngle, 0); // Calcalate rotation based on wheelAngle
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * 10f); // Lerp between current
            Debug.DrawRay(transform.position, transform.forward); // Debug
        }
    }

    void HandleAcceleration()
    {
        if (!isMotorized) return;
        if (!grounded) return;
        if (speedFactor == 1) return; // If car speed is higher than car's top speed

        if (playerInput.AccelerationInputValue > 0)
        {
            switch (enginePowerMode)
            {
            case EnginePowerMode.Lerp:
                float availablePower = Mathf.Lerp(CarData.EnginePower, 0, speedFactor);
                float accelerationAmount = availablePower * playerInput.AccelerationInputValue;
                rb.AddForceAtPosition(transform.forward * accelerationAmount, transform.position);
                break;
            
            case EnginePowerMode.Curve:
                availablePower = CarData.EnginePower * powerCurve.Evaluate(speedFactor);
                accelerationAmount = availablePower * playerInput.AccelerationInputValue;
                rb.AddForceAtPosition(transform.forward * accelerationAmount, transform.position);
                break;
            }
        }
    }

    void HandleBrakingAndReversing()
    {
        if (!grounded) return;
        if (playerInput.BrakeInputValue < 0.1f) return; // If no user input

        if (forwardSpeed > 0.1f) // If the car has forward velocity apply brakes, if 0 velocity apply reversing
        {
            float brakeAmount = playerInput.BrakeInputValue * CarData.BrakeForce;
            rb.AddForceAtPosition(-transform.forward * brakeAmount, transform.position);
        }
        else
        {
            float reverseSpeed = Mathf.Abs(forwardSpeed);
            if (reverseSpeed >= CarData.ReversingMaxSpeed) return; // Only reverse when slower than max reverse speed
            if (isMotorized) // Only apply the reversing force on motorized wheels
            {
                float reverseAmount = CarData.EnginePower * playerInput.BrakeInputValue;
                rb.AddForceAtPosition(-transform.forward * reverseAmount, transform.position);
            }
        }
    }

    void HandleTraction()
    {
        float lateralVelocity = Vector3.Dot(transform.right, rb.GetPointVelocity(transform.position));
        rb.AddForceAtPosition(-transform.right * lateralVelocity * CarData.TireGrip, transform.position);
    }
}