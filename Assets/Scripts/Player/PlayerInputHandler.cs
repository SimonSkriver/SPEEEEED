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
    private InputAction menuLeftAction;
    private InputAction menuRightAction;
    private InputAction submitAction;
    private InputAction backAction;

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

    public event System.Action MenuLeftPressed;
    public event System.Action MenuRightPressed;
    public event System.Action SubmitPressed;
    public event System.Action BackPressed;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        accelerateAction = playerInput.actions.FindAction("Accelerate");
        brakeAction = playerInput.actions.FindAction("Brake");
        steerAction = playerInput.actions.FindAction("Steer");
        driftAction = playerInput.actions.FindAction("Drift");

        menuLeftAction = playerInput.actions.FindAction("Menu Left");
        menuRightAction = playerInput.actions.FindAction("Menu Right");
        submitAction = playerInput.actions.FindAction("Submit");
        backAction = playerInput.actions.FindAction("Back");
    }

    void OnEnable()
    {
        menuLeftAction.performed += OnMenuLeft;
        menuRightAction.performed += OnMenuRight;
        submitAction.performed += OnSubmit;
        backAction.performed += OnBack;
    }

    void OnDisable()
    {
        menuLeftAction.performed -= OnMenuLeft;
        menuRightAction.performed -= OnMenuRight;
        submitAction.performed -= OnSubmit;
        backAction.performed -= OnBack;
    }

    void Update()
    {
        accelerateInputValue = accelerateAction.ReadValue<float>();
        brakeInputValue = brakeAction.ReadValue<float>();
        steerInputValue = steerAction.ReadValue<float>();
        driftIsHeld = driftAction.IsPressed();
    }

    public void OnMenuLeft(InputAction.CallbackContext context) => MenuLeftPressed?.Invoke();
    public void OnMenuRight(InputAction.CallbackContext context) => MenuRightPressed?.Invoke();
    public void OnSubmit(InputAction.CallbackContext context) => SubmitPressed?.Invoke();
    public void OnBack(InputAction.CallbackContext context) => BackPressed?.Invoke();
}