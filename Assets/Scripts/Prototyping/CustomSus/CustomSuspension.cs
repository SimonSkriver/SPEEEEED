using UnityEngine;

public class CustomSpring : MonoBehaviour
{
    public Rigidbody rb;
    public Transform[] wheelPositions;
    public float susRestLength = 1f;
    public float susStrenth = 8.175f;
    public float damperStrenth = 100;

    private float springForce;
    private float springVelocity;
    private float damperForce;

    private float currentOffset;
    private float previousOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var wheel in wheelPositions)
        {
            if (Physics.Raycast(wheel.position, -Vector3.up, out RaycastHit hit, susRestLength))
            {
                previousOffset = currentOffset;

                currentOffset = susRestLength - hit.distance; // Length of suspension right now

                springForce = currentOffset * susStrenth; // Calculate the force
                springVelocity = (currentOffset - previousOffset) / Time.fixedDeltaTime; // Calculate the spring's velocity

                damperForce = springVelocity * damperStrenth;

                rb.AddForceAtPosition(transform.up * (springForce + damperForce), transform.position);
            }

        }

        Debug.DrawRay(transform.position, -Vector3.up, Color.yellow);
    }
}