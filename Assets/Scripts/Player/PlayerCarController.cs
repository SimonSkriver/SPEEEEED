using UnityEngine;

public class PlayerCarController : MonoBehaviour
{

    [Header("Wheels (set automatically, visible for debugging purposes)")]
    [SerializeField] private WheelCollider[] wheels;
    [SerializeField] private WheelCollider[] frontWheels;

    [SerializeField] TestCarData carData;
    [SerializeField] PlayerInputHandler playerInput;
    [SerializeField] Rigidbody rb;

    [SerializeField] float currentSpeed;
    [SerializeField] AnimationCurve steeringCurve;

    void Awake()
    {
        wheels = GetComponentsInChildren<WheelCollider>();
        playerInput = GetComponent<PlayerInputHandler>();
        rb = GetComponent<Rigidbody>();
        Debug.Log(wheels);
    }

    void FixedUpdate()
    {
        HandleAcceleration();
        currentSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        ApplySteering();
    }

    void HandleAcceleration()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = carData.EnginePower * playerInput.AccelerationValue;
        }
    }

    void ApplySteering()
    {
        float steeringAngle = playerInput.SteerValue.x * steeringCurve.Evaluate(currentSpeed);
        foreach (var wheel in frontWheels)
        {
            wheel.steerAngle = steeringAngle;
        }
    }
}