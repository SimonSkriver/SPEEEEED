using System.Collections;
using UnityEngine;

public class CarInCaseOfEmergency : MonoBehaviour
{
    private PlayerInputHandler playerInput;
    private Rigidbody rb;
    private bool ready = true;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInputHandler>();
        rb = GetComponentInParent<Rigidbody>();

        if (playerInput == null) Debug.LogWarning("InCaseOfEmergency Script: No PlayerInputHandler found! Check parent");
        if (rb == null) Debug.LogWarning("InCaseOfEmergency Script: No PlayerInputHandler found! Check parent");

        playerInput.InCaseOfEmergencyPressed += StartPanickCoRoutine;
    }

    void OnDisable()
    {
        playerInput.InCaseOfEmergencyPressed -= StartPanickCoRoutine;
    }

    void StartPanickCoRoutine()
    {
        StartCoroutine(PanickButton());
    }

    IEnumerator PanickButton()
    {
        if (ready)
        {
            float randomSide = Random.Range(-1f, 1f);
            Vector3 forcePosition = rb.worldCenterOfMass + rb.transform.right * randomSide;
            float randomStrength = Random.Range(5000f, 6000f);
            rb.AddForceAtPosition(Vector3.up * randomStrength, forcePosition, ForceMode.Impulse);
            ready = false;
        }
        yield return new WaitForSeconds(1f);
        ready = true;
    }
}