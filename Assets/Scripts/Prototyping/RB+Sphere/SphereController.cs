using UnityEngine;

public class SphereController : MonoBehaviour
{
    [Header("References (set automatically, visible for debugging purposes)")]
    [SerializeField] Rigidbody sphereRB;
    [SerializeField] PlayerInputHandler playerInput;

    public TestCarData carData;


    void Awake()
    {
        if (sphereRB == null) sphereRB = GetComponentInChildren<Rigidbody>();
        if (playerInput == null) playerInput = GetComponent<PlayerInputHandler>();
    }

    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {
        // Set position of car to the sphere
        transform.position = sphereRB.transform.position;


        Vector3 steerRotation = new Vector3(0, playerInput.SteerValue.x * carData.SteeringRange, 0);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + steerRotation * playerInput.AccelerationValue * Time.deltaTime);
    }

    void FixedUpdate()
    {
        sphereRB.AddForce(transform.forward * carData.EnginePower * playerInput.AccelerationValue, ForceMode.Acceleration);
    }
}