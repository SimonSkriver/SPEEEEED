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

    [Header("Values")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateValue;
    [SerializeField] private float brakeValue;
    [SerializeField] private Vector2 steerValue;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationValue => accelerateValue;
    public float BrakeValue => brakeValue;
    public Vector2 SteerValue => steerValue;


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
    }

    void Update()
    {
        accelerateValue = accelerateAction.ReadValue<float>();
        brakeValue = brakeAction.ReadValue<float>();
        steerValue = steerAction.ReadValue<Vector2>();
    }
}