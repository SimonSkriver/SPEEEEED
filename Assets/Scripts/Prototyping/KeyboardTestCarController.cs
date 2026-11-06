using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardTestCarController : MonoBehaviour
{

    [Header("Wheels (set automatically, visible for debugging purposes)")]
    [SerializeField] private WheelCollider[] wheels;
    [SerializeField] private WheelCollider[] frontWheels;

    [SerializeField] TestCarData carData;
    [SerializeField] Rigidbody rb;

    [SerializeField] float currentSpeed;
    [SerializeField] AnimationCurve steeringCurve;

    float acceleration;
    float steering;
    bool keyboardMissing;

    void Awake()
    {
        wheels = GetComponentsInChildren<WheelCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            acceleration = 0f;
            steering = 0f;
            if (!keyboardMissing)
            {
                Debug.LogWarning("No keyboard is available for the test car.", this);
                keyboardMissing = true;
            }
            return;
        }

        keyboardMissing = false;
        acceleration = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
        steering = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
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
            wheel.motorTorque = carData.EnginePower * acceleration;
        }
    }

    void ApplySteering()
    {
        float steeringAngle = steering * steeringCurve.Evaluate(currentSpeed);
        foreach (var wheel in frontWheels)
        {
            wheel.steerAngle = steeringAngle;
        }
    }
}
