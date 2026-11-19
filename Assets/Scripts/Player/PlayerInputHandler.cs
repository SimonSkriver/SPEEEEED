using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Components")]
    PlayerInput playerInput;

    [Header("In Game Actions")]
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction steerAction;
    private InputAction driftAction;

    [Header("UI Action")]
    private InputAction menuLeft;
    private InputAction menuRight;

    [Header("Values (visible for debugging purposes)")]
    // Private fields so as not to be read or written from elsewhere (serialized for debugging purposes)
    [SerializeField] private float accelerateInputValue;
    [SerializeField] private float brakeInputValue;
    [SerializeField] private float steerInputValue;
    [SerializeField] private bool driftIsHeld;
    // Public properties to get the value of the private fields from outside this class, without overriding them
    public float AccelerationInputValue => accelerateInputValue;
    public float BrakeInputValue => brakeInputValue;
    public float SteerInputValue => steerInputValue;
    public bool DriftIsHeld => driftIsHeld;


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
        driftAction = playerInput.actions.FindAction("Drift");

        menuLeft = playerInput.actions.FindAction("MenuLeft");
        menuRight = playerInput.actions.FindAction("MenuRight");
        menuLeft.performed += ctx => OnLeftClick();
        menuRight.performed += ctx => OnRightClick();
    }

    void Update()
    {
        accelerateInputValue = accelerateAction.ReadValue<float>();
        brakeInputValue = brakeAction.ReadValue<float>();
        steerInputValue = steerAction.ReadValue<float>();
        driftIsHeld = driftAction.IsPressed();
    }

    public void OnLeftClick()
    {

    }

    public void OnRightClick()
    {

    }
}