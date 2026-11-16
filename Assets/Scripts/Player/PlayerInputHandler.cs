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
<<<<<<< HEAD

    [Header("Values (visible for debugging purposes)")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateValue;
    [SerializeField] private float brakeValue;
    [SerializeField] private Vector2 steerValue;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationValue => accelerateValue;
    public float BrakeValue => brakeValue;
    public Vector2 SteerValue => steerValue;
=======
    private InputAction lookAction;

    [Header("Values (visible for debugging purposes)")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateInputValue;
    [SerializeField] private float brakeInputValue;
    [SerializeField] private float steerInputValue;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationInputValue => accelerateInputValue;
    public float BrakeInputValue => brakeInputValue;
    public float SteerInputValue => steerInputValue;
>>>>>>> merge-before-main


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
<<<<<<< HEAD
=======
        lookAction = playerInput.actions.FindAction("Look");
>>>>>>> merge-before-main
    }

    void Update()
    {
<<<<<<< HEAD
        accelerateValue = accelerateAction.ReadValue<float>();
        brakeValue = brakeAction.ReadValue<float>();
        steerValue = steerAction.ReadValue<Vector2>();
=======
        accelerateInputValue = accelerateAction.ReadValue<float>();
        brakeInputValue = brakeAction.ReadValue<float>();
        steerInputValue = steerAction.ReadValue<float>();
>>>>>>> merge-before-main
    }
}