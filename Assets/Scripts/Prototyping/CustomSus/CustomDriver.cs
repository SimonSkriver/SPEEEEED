using UnityEngine;

public class CustomDriver : MonoBehaviour
{
    [Header("Acceleration")]
    [SerializeField] private float accelSpeed;

    [Header("Turning")]
    [SerializeField] private float turnSpeed;

    [Header("Traction")]
    [SerializeField] private float traction;

    [Header("Debug")]
    [SerializeField] private float velocityDebugRayLength;
    [SerializeField] private float currentSpeed;

    private Rigidbody rb;
    private PlayerInputHandler playerInput;
    public float forceOffset = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputHandler>();
    }

    private void FixedUpdate()
    {
        // Acceleration
        Vector3 desiredAccel = transform.forward * accelSpeed * playerInput.AccelerationValue;
        rb.AddForceAtPosition(desiredAccel, transform.position, ForceMode.Acceleration);

        // Turn
        if (playerInput.AccelerationValue != 0)
        {
            Vector3 desiredTurn = Vector3.up * turnSpeed * playerInput.SteerValue.x;
            rb.AddTorque(desiredTurn);
        }

        /*// Traction
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            tractionConstant = driftTractionConstant;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            tractionConstant = normalTractionConstant;
        }*/
        Vector3 desiredTraction = transform.InverseTransformDirection(rb.linearVelocity);
        rb.AddForce(transform.right * -desiredTraction.x * traction);
    }
}