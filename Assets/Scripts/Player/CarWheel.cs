using UnityEngine;

public class CarWheel : MonoBehaviour
{
    [Header("Car Data (set manually, critical for functionality)")]
    [Tooltip("The specific ScriptableObject from which the data is read to calculate car behaviour")]
    public CarData CarData;

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

    [Header("Traction settings")]
    [SerializeField] float tractionCorrectionTime = 2f;

    [Header("Components (set automatically")]
    private Rigidbody rb;
    private PlayerInputHandler playerInput;

    [Header("Internal values (to be used within script)")]
    private LayerMask playerLayer;
    private RaycastHit hit;
    private bool grounded;
    private float forwardSpeed;
    private Vector3 wheelVelocity;
    private float wheelMass;
    private float speedFactor; // Value of current speed against top speed (0-1)
    private float currentTireGrip;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerInputHandler>();
        playerLayer = LayerMask.GetMask("Player");

        if (CarData == null) Debug.LogWarning("No CarData set, no data is available");
        if (rb == null) Debug.LogWarning("No Rigidbody found, check parent");
        if (playerInput == null) Debug.LogWarning("No PlayerInputHandler found, check parent");
        wheelMass = rb.mass / 4;
    }

    void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, out hit, springRestLength + wheelRadius, ~playerLayer);
        forwardSpeed = Vector3.Dot(rb.transform.forward, rb.linearVelocity);
        wheelVelocity = rb.GetPointVelocity(transform.position);
        speedFactor = Mathf.InverseLerp(0, CarData.TopSpeed, forwardSpeed);
        currentTireGrip = CarData.TireGrip;

        HandleTurningWheel();
        HandleSuspension();
        HandleAcceleration();
        HandleBrakingAndReversing();
        HandleDrifting();
        HandleTraction();
        DebugRays();
    }

    void HandleSuspension()
    {
        if (!grounded) return;

        float offset = springRestLength - (hit.distance - wheelRadius); // Calculate offset
        float springForce = offset * springStrength; // Calculate spring force
        float springVelocity = Vector3.Dot(transform.up, wheelVelocity); // Calculate spring velocity
        float damperForce = springVelocity * -damperStrength; // Calculate damping amount
        float totalForce = springForce + damperForce; // Calculate total force
        rb.AddForceAtPosition(transform.up * totalForce, transform.position); // Apply force
    }

    void HandleTurningWheel()
    {
        if (isSteering) // Only turn the wheels that are steering
        {
            float currentSteeringRange = Mathf.Lerp(CarData.SteeringRange, CarData.SteeringRangeAtTopSpeed, speedFactor); // Lerp between max-min range based on speed
            float wheelAngle = playerInput.SteerInputValue * currentSteeringRange; // Calculate wheel angle based on user input and speed
            Quaternion targetRotation = Quaternion.Euler(0f, wheelAngle, 0f); // Calcalate rotation based on wheelAngle
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * 10f); // Lerp between current
        }
    }

    void HandleAcceleration()
    {
        if (!isMotorized) return;
        if (!grounded) return;
        //if (speedFactor == 1) return; // If car speed is higher than car's top speed

        if (playerInput.AccelerationInputValue > 0)
        {
            float availablePower = CarData.EnginePower * CarData.PowerCurve.Evaluate(speedFactor);
            float accelerationAmount = availablePower * playerInput.AccelerationInputValue;
            rb.AddForceAtPosition(transform.forward * accelerationAmount, transform.position);
        }
    }

    void HandleBrakingAndReversing()
    {
        if (playerInput.BrakeInputValue < 0.1f) return; // If no user input
        if (!grounded) return;

        if (forwardSpeed > 0.1f) // If the car has forward velocity apply brakes, if 0 velocity apply reversing
        {
            Brake();
        }
        else
        {
            Reverse();
        }
    }

    void Brake()
    {
        float brakeAmount = playerInput.BrakeInputValue * CarData.BrakeForce;
        rb.AddForceAtPosition(-rb.transform.forward * brakeAmount, transform.position);
    }

    void Reverse()
    {
        float reverseSpeed = Mathf.Abs(forwardSpeed);
        if (reverseSpeed >= CarData.ReversingTopSpeed) return; // Only reverse when slower than max reverse speed
        if (isMotorized)
        {
            float reverseAmount = CarData.EnginePower * playerInput.BrakeInputValue;
            rb.AddForceAtPosition(-transform.forward * reverseAmount, transform.position);
        }
    }

    void HandleTraction()
    {
        if (!grounded) return;

        float lateralVelocity = Vector3.Dot(transform.right, wheelVelocity);
        float targetVelocityChange = -lateralVelocity * currentTireGrip;
        float counterAcceleration = targetVelocityChange / Time.fixedDeltaTime;
        rb.AddForceAtPosition(transform.right * counterAcceleration * wheelMass, transform.position);
    }

    void HandleDrifting()
    {
        if (playerInput.DriftIsHeld)
        {
            if (!isSteering) currentTireGrip = CarData.RearWheelDriftingTireGrip; // Rear wheels
            else currentTireGrip = CarData.FrontWheelDriftingTireGrip; // Front wheels
        }
        else
        {
            currentTireGrip = Mathf.Lerp(currentTireGrip, CarData.TireGrip, Time.fixedDeltaTime * tractionCorrectionTime);
        }
    }

    void DebugRays()
    {
        Color suspensionDebugRayColor = grounded ? Color.green : Color.yellow;
        Debug.DrawRay(transform.position, -transform.up * (springRestLength + wheelRadius), suspensionDebugRayColor); // Suspension debug
        Debug.DrawRay(transform.position, transform.right, Color.red); // Suspension debug
    }
}