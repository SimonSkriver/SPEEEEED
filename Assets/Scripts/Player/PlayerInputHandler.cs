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

    [Header("Values (visible for debugging purposes)")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateInputValue;
    [SerializeField] private float brakeInputValue;
    [SerializeField] private float steerInputValue;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationInputValue => accelerateInputValue;
    public float BrakeInputValue => brakeInputValue;
    public float SteerInputValue => steerInputValue;


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
    }

    void Update()
    {
        accelerateInputValue = accelerateAction.ReadValue<float>();
        brakeInputValue = brakeAction.ReadValue<float>();
        steerInputValue = steerAction.ReadValue<float>();
    }
}