using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Components")]
    PlayerInput playerInput;

    [Header("Actions")]
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction steerAction;
    private InputAction lookAction;

    [Header("Values")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateValue;
    [SerializeField] private float brakeValue;
    [SerializeField] private Vector2 steerValue;
    [SerializeField] private Vector2 lookValue;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationValue => accelerateValue;
    public float BrakeValue => brakeValue;
    public Vector2 SteerValue => steerValue;
    public Vector2 LookValue => lookValue;


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
        lookAction = playerInput.actions.FindAction("Look");
    }

    void Update()
    {
        accelerateValue = accelerateAction.ReadValue<float>();
        brakeValue = brakeAction.ReadValue<float>();
        steerValue = steerAction.ReadValue<Vector2>();
        lookValue = lookAction.ReadValue<Vector2>();
    }
}