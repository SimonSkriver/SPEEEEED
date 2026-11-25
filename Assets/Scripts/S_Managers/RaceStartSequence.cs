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
        AddPlayerCars();

        for (int i = 0; i < cars.Count && i < gridSlots.Length; i++)
        {
            cars[i].SetActive(true);
            MoveCar(cars[i], gridSlots[i]);
        }

        foreach (GameObject car in cars)
        {
            raceManager.RegisterRacer(car);
        }
    }

    // The cars the players chose live under their PlayerInfo, in player order
    private void AddPlayerCars()
    {
        if (MultiplayerManager.Instance == null)
        {
            return;
        }

        List<PlayerInfo> players = new List<PlayerInfo>(MultiplayerManager.Instance.AllPlayers);
        players.Sort((a, b) => a.PlayerID.CompareTo(b.PlayerID));

        foreach (PlayerInfo player in players)
        {
            player.SpawnRaceCar();
            if (player.RaceCar != null)
            {
                cars.Add(player.RaceCar);
            }
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
            PlayerInput input = PlayerInputHandler.FromCar(cars[i]).GetComponent<PlayerInput>();
            if (input.camera == null)
            {
                input.camera = cars[i].GetComponentInChildren<Camera>();
            }

            input.camera.rect = GetViewport(i, cars.Count);

            AudioListener listener = cars[i].GetComponentInChildren<AudioListener>();
            if (listener != null)
            {
                listener.enabled = i == 0; // Only one listener may be active
            }
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
            PlayerInputHandler handler = PlayerInputHandler.FromCar(car);
            handler.enabled = drivable;

            if (drivable)
            {
                // Driving inputs live in the "Player" map; the players come from the lobby on the "UI" map
                handler.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
            }
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
