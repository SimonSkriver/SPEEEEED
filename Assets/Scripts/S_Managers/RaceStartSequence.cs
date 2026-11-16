using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

// Before the race: puts the cars on the grid, moves a camera from A to B, counts down, then starts the race.
public class RaceStartSequence : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;

    [Header("Cars")]
    [SerializeField] private Transform[] gridSlots;
    [Tooltip("The cars the players chose. If empty, the cars tagged Player1/Player2 in the scene are moved to the slots.")]
    [SerializeField] private GameObject[] carPrefabs;

    [Header("Intro camera")]
    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private Transform cameraStart;
    [SerializeField] private Transform cameraEnd;
    [SerializeField] private float introDuration = 4f;

    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int countdownFrom = 3;

    private readonly List<GameObject> cars = new List<GameObject>();

    private IEnumerator Start()
    {
        PlaceCars();
        SetCarsDrivable(false);

        yield return PlayIntroCamera();
        yield return RunCountdown();

        SetCarsDrivable(true);
        raceManager.StartRace();

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void PlaceCars()
    {
        if (carPrefabs.Length > 0)
        {
            for (int i = 0; i < carPrefabs.Length && i < gridSlots.Length; i++)
            {
                cars.Add(Instantiate(carPrefabs[i], gridSlots[i].position, gridSlots[i].rotation));
            }
        }
        else
        {
            AddSceneCar("Player1");
            AddSceneCar("Player2");
            for (int i = 0; i < cars.Count && i < gridSlots.Length; i++)
            {
                MoveCar(cars[i], gridSlots[i]);
            }
        }

        foreach (GameObject car in cars)
        {
            raceManager.RegisterRacer(car);
        }
    }

    private void AddSceneCar(string carTag)
    {
        GameObject car = GameObject.FindGameObjectWithTag(carTag);
        if (car == null)
        {
            return;
        }

        cars.Add(car);
    }

    // Teleport through the Rigidbody too, otherwise physics and interpolation can put the car back
    private static void MoveCar(GameObject car, Transform slot)
    {
        car.transform.SetPositionAndRotation(slot.position, slot.rotation);

        Rigidbody body = car.GetComponentInChildren<Rigidbody>();
        if (body != null)
        {
            body.position = slot.position;
            body.rotation = slot.rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();
        Debug.Log($"RaceStartSequence: moved {car.name} to {slot.name}", car);
    }

    // Disabling the input handler stops the car from receiving throttle, brake and steering
    private void SetCarsDrivable(bool drivable)
    {
        foreach (GameObject car in cars)
        {
            car.GetComponentInChildren<PlayerInputHandler>().enabled = drivable;
        }
    }

    private IEnumerator PlayIntroCamera()
    {
        for (float time = 0f; time < introDuration; time += Time.deltaTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, time / introDuration);
            introCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(cameraStart.position, cameraEnd.position, t),
                Quaternion.Slerp(cameraStart.rotation, cameraEnd.rotation, t));
            yield return null;
        }

        introCamera.gameObject.SetActive(false); // The players' cameras take over
    }

    private IEnumerator RunCountdown()
    {
        countdownText.gameObject.SetActive(true);
        for (int number = countdownFrom; number > 0; number--)
        {
            countdownText.text = number.ToString();
            yield return new WaitForSeconds(1f);
        }
    }
}
