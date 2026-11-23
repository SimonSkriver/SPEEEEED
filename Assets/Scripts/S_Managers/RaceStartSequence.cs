using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Before the race: puts the cars on the grid, moves a camera from A to B, counts down, then starts the race.
public class RaceStartSequence : MonoBehaviour
{
    [SerializeField] private RaceManager raceManager;

    [Header("Cars")]
    [SerializeField] private Transform[] gridSlots;
    [Tooltip("The cars the players chose. If empty, the cars tagged Player1 to Player4 in the scene are moved to the slots.")]
    [SerializeField] private GameObject[] carPrefabs;

    [Tooltip("Only for two players: on = Player 1 on top and Player 2 below. Off = Player 1 left and Player 2 right. Three or four players use quadrants.")]
    [SerializeField] private bool splitScreenTopBottom = false;

    [Header("Intro camera")]
    [SerializeField] private CinemachineCamera introCamera;
    [Tooltip("The full-screen Camera (with a CinemachineBrain on the intro's channel) that renders the intro. Turned off when the intro ends so it doesn't cover the split screen.")]
    [SerializeField] private Camera introScreenCamera;
    [SerializeField] private Transform cameraStart;
    [SerializeField] private Transform cameraEnd;
    [SerializeField] private float introDuration = 4f;

    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int countdownFrom = 3;
    [Tooltip("Seconds to wait after the intro ends before the countdown starts.")]
    [SerializeField] private float delayBeforeCountdown = 1.5f;

    private readonly List<GameObject> cars = new List<GameObject>();

    private IEnumerator Start()
    {
        PlaceCars();
        ArrangeSplitScreen();
        SetCarsDrivable(false);

        yield return PlayIntroCamera();
        yield return new WaitForSeconds(delayBeforeCountdown);
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
            AddSceneCar("Player3");
            AddSceneCar("Player4");
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

    private void ArrangeSplitScreen()
    {
        if (cars.Count < 2)
        {
            return;
        }

        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].GetComponentInChildren<PlayerInput>().camera.rect = GetViewport(i, cars.Count);
        }
    }

    // Two players split the screen in half; three or four use quadrants (Player 1 top left, 2 top right, 3 bottom left, 4 bottom right)
    private Rect GetViewport(int index, int playerCount)
    {
        if (playerCount == 2)
        {
            if (splitScreenTopBottom)
            {
                return index == 0 ? new Rect(0f, 0.5f, 1f, 0.5f) : new Rect(0f, 0f, 1f, 0.5f);
            }

            return index == 0 ? new Rect(0f, 0f, 0.5f, 1f) : new Rect(0.5f, 0f, 0.5f, 1f);
        }

        float x = index % 2 == 0 ? 0f : 0.5f;
        float y = index < 2 ? 0.5f : 0f;
        return new Rect(x, y, 0.5f, 0.5f);
    }

    private void AddSceneCar(string carTag)
    {
        GameObject car;
        try
        {
            car = GameObject.FindGameObjectWithTag(carTag);
        }
        catch (UnityException) // The tag doesn't exist in the project
        {
            return;
        }

        if (car == null)
        {
            return;
        }

        cars.Add(car);
    }

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

        introCamera.gameObject.SetActive(false); 
        if (introScreenCamera != null)
        {
            introScreenCamera.gameObject.SetActive(false);
        }
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
