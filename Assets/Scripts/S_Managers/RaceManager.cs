using System;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [Serializable]
    public class Racer
    {
        public GameObject car;
        public int laps;
        public int position;
        [HideInInspector] public int nextCheckpoint;
        [HideInInspector] public float lapStartTime;
        [HideInInspector] public bool lapTimerStarted;
        public string lapTime = "00:00:00";
        [HideInInspector] public bool finished;
        [HideInInspector] public int finishPlace;
    }

    private const int LapsToFinish = 1;

    [Tooltip("Place these in driving order, with the finish line first.")]
    [SerializeField] private Collider[] checkpoints;
    [SerializeField] private List<Racer> racers = new List<Racer>();

    [Header("Race over screen")]
    [Tooltip("The overlay canvas shown when everyone has finished. It is hidden at the start of the scene.")]
    [SerializeField] private GameObject raceOverCanvas;
    [Tooltip("The button that is selected when the canvas opens, so a controller can navigate it.")]
    [SerializeField] private Button firstSelectedButton;
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private int carSelectSceneIndex = 1;

    private int finishedCount;
    private InputActionAsset menuActions;

    public IReadOnlyList<Racer> Racers => racers;
    public GameObject Winner { get; private set; }
    public bool RaceStarted { get; private set; }

    private void Start()
    {
        if (raceOverCanvas != null)
        {
            raceOverCanvas.SetActive(false);
        }
    }

    public void RegisterRacer(GameObject car)
    {
        if (car == null)
        {
            return;
        }

        if (RaceStarted || Winner != null)
        {
            return;
        }

        if (!racers.Exists(racer => racer.car == car))
        {
            racers.Add(new Racer { car = car });
        }
    }

    public void StartRace()
    {
        if (RaceStarted || Winner != null)
        {
            return;
        }

        if (racers.Count == 0 || checkpoints == null || checkpoints.Length < 2)
        {
            return;
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null || !checkpoints[i].enabled ||
                !checkpoints[i].gameObject.activeInHierarchy || !checkpoints[i].isTrigger)
            {
                return;
            }

            for (int j = 0; j < i; j++)
            {
                if (checkpoints[i] == checkpoints[j])
                {
                    return;
                }
            }
        }

        foreach (Racer racer in racers)
        {
            if (racer.car == null)
            {
                return;
            }

            racer.laps = 0;
            racer.nextCheckpoint = 0;
            racer.lapTimerStarted = false;
            racer.lapTime = FormatTime(0f);
            racer.finished = false;
            racer.finishPlace = 0;
        }

        finishedCount = 0;
        RaceStarted = true;
    }

    private static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return $"{time.Minutes:D2}:{time.Seconds:D2}:{time.Milliseconds / 10:D2}";
    }

    private void Update()
    {
        if (!RaceStarted) return;

        foreach (Racer racer in racers)
        {
            if (racer.car == null)
            {
                RaceStarted = false;
                return;
            }

            if (racer.finished) continue;

            if (racer.lapTimerStarted)
            {
                racer.lapTime = FormatTime(Time.time - racer.lapStartTime);
            }

            Collider next = checkpoints[racer.nextCheckpoint];
            Vector3 carPosition = racer.car.transform.position;
            if (next.ClosestPoint(carPosition) != carPosition) continue;

            // The finish line is checkpoint 0: it starts the lap, and is also the last one hit to end it
            bool crossedFinish = racer.nextCheckpoint == 0;
            racer.nextCheckpoint = (racer.nextCheckpoint + 1) % checkpoints.Length;
            if (!crossedFinish) continue;

            if (!racer.lapTimerStarted)
            {
                racer.lapStartTime = Time.time;
                racer.lapTimerStarted = true;
                Debug.Log($"<color=yellow>LAP 1 STARTED: {racer.car.name}</color>");
                continue;
            }

            racer.laps++;
            if (racer.laps < LapsToFinish) continue;

            racer.lapTimerStarted = false;
            racer.lapTime = FormatTime(Time.time - racer.lapStartTime);
            Debug.Log($"<color=green>LAP 1 COMPLETED: {racer.car.name} - Final Time: {racer.lapTime}</color>");

            racer.finished = true;
            racer.finishPlace = ++finishedCount;
            if (Winner == null)
            {
                Winner = racer.car;
            }

            LockCar(racer.car);
        }

        racers.Sort((a, b) =>
        {
            if (a.finished != b.finished) return a.finished ? -1 : 1;
            if (a.finished) return a.finishPlace.CompareTo(b.finishPlace);

            int result = b.laps.CompareTo(a.laps);
            if (result != 0) return result;
            result = b.nextCheckpoint.CompareTo(a.nextCheckpoint);
            if (result != 0) return result;
            Vector3 target = checkpoints[a.nextCheckpoint].bounds.center;
            return (a.car.transform.position - target).sqrMagnitude.CompareTo(
                (b.car.transform.position - target).sqrMagnitude);
        });

        for (int i = 0; i < racers.Count; i++)
        {
            racers[i].position = i + 1;
        }

        if (finishedCount == racers.Count)
        {
            RaceStarted = false;
            ShowRaceOverScreen();
        }
    }

    private void LockCar(GameObject car)
    {
        PlayerInputHandler handler = PlayerInputHandler.FromCar(car);
        PlayerInput input = handler != null ? handler.GetComponent<PlayerInput>() : null;
        if (input == null)
        {
            return;
        }

        input.DeactivateInput();
    }

    private void ShowRaceOverScreen()
    {
        if (raceOverCanvas == null)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        raceOverCanvas.SetActive(true);

        // Like the map select, only player one uses the menu: the EventSystem would otherwise react to every player's device
        if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.AllPlayers.Count > 0 &&
            EventSystem.current != null && EventSystem.current.TryGetComponent(out InputSystemUIInputModule uiModule) &&
            uiModule.actionsAsset != null)
        {
            menuActions = uiModule.actionsAsset;
            menuActions.devices = MultiplayerManager.Instance.GetPlayerOne().GetComponent<PlayerInput>().devices;
        }

        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    private void OnDestroy()
    {
        // The actions asset outlives the scene, so give the devices back
        if (menuActions != null) menuActions.devices = null;
    }

    // Hook these up to the buttons' OnClick in the Inspector
    // Keeps the players and gives each a fresh car; the race scene then sets up the grid like a normal start
    public void RestartRace()
    {
        if (MultiplayerManager.Instance != null)
        {
            foreach (PlayerInfo player in MultiplayerManager.Instance.AllPlayers)
            {
                player.DestroyRaceCar();
                player.SpawnRaceCar();
                player.GetComponent<PlayerInput>().ActivateInput(); // LockCar deactivated it when they finished
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadCarSelect() => LoadScene(carSelectSceneIndex);

    public void LoadMainMenu() => LoadScene(mainMenuSceneIndex);

    // Leaving the race for a menu scene: remove the players and the multiplayer manager so the next scene starts fresh
    private static void LoadScene(int buildIndex)
    {
        ReleasePlayers();
        SceneManager.LoadScene(buildIndex);
    }

    private static void ReleasePlayers()
    {
        if (MultiplayerManager.Instance == null) return;

        // Destroying a player calls OnPlayerLeft, which edits AllPlayers, so loop over a copy
        foreach (PlayerInfo player in MultiplayerManager.Instance.AllPlayers.ToArray())
        {
            player.DestroyRaceCar();
            Destroy(player.gameObject);
        }

        MultiplayerManager.Instance.AllPlayers.Clear();
        Destroy(MultiplayerManager.Instance.gameObject);
    }
}
