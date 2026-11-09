using UnityEngine;

public class Suspension : MonoBehaviour
{

    [Header("Settings (can be tweaked)")]
    [SerializeField] private float springRestLength = 1f;
    [SerializeField] private float springStrenth = 8.175f;
    [SerializeField] private float damperStrenth = 3;

    public Transform[] springPositions;
    private Rigidbody rb;

    [Header("Values to be calculated")]
    private float offset;
    private float springForce;
    private float springVelocity;
    private float damperForce;
    private float totalForce;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var spring in springPositions)
        {
            if (Physics.Raycast(spring.position, -spring.up, out RaycastHit hit, springRestLength))
            {
                offset = springRestLength - hit.distance; // Calculate offset
                springForce = offset * springStrenth; // Calculate spring force

                Vector3 springWorldVel = rb.GetPointVelocity(spring.position); // Spring velocity
                springVelocity = Vector3.Dot(spring.up, springWorldVel); // Spring velocity

                damperForce = springVelocity * -damperStrenth; // Calculate damping amount

                totalForce = springForce + damperForce; // Calculate total force
                rb.AddForceAtPosition(spring.up * totalForce, spring.position); // Apply force

                Debug.DrawRay(spring.position, -spring.up * hit.distance, Color.yellow); // Debug
            }
        }
    }
}