using UnityEngine;

public class CarBrakeLights : MonoBehaviour
{
    [SerializeField] GameObject brakeLights;
    private PlayerInputHandler playerInput;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInputHandler>();
    }

    void Update()
    {
        brakeLights.SetActive(playerInput.BrakeInputValue > 0.1f);
    }
}