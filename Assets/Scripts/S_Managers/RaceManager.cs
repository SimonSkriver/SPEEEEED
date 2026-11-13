using System;
using System.Collections.Generic;
using UnityEngine;

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
    }

    private const int LapsToFinish = 1;

    [Tooltip("Place these in driving order, with the finish line first.")]
    [SerializeField] private Collider[] checkpoints;
    [SerializeField] private List<Racer> racers = new List<Racer>();

    public IReadOnlyList<Racer> Racers => racers;
    public GameObject Winner { get; private set; }
    public bool RaceStarted { get; private set; }

    private void Start()
    {
        GameObject testCar = GameObject.FindGameObjectWithTag("Player1");
        if (testCar != null)
        {
            RegisterRacer(testCar);
        }

        GameObject testCar2 = GameObject.FindGameObjectWithTag("Player2");
        if (testCar2 != null)
        {
            RegisterRacer(testCar2);
        }

        StartRace();
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
            Debug.LogWarning("RaceManager: race not started - needs at least 1 racer and 2 checkpoints.");
            return;
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null || !checkpoints[i].enabled ||
                !checkpoints[i].gameObject.activeInHierarchy || !checkpoints[i].isTrigger)
            {
                Debug.LogWarning($"RaceManager: race not started - checkpoint at index {i} is missing, disabled, inactive or not a trigger.");
                return;
            }

            for (int j = 0; j < i; j++)
            {
                if (checkpoints[i] == checkpoints[j])
                {
                    Debug.LogWarning($"RaceManager: race not started - checkpoint index {i} is a duplicate of index {j}.");
                    return;
                }
            }
        }

        foreach (Racer racer in racers)
        {
            if (racer.car == null)
            {
                Debug.LogWarning("RaceManager: race not started - a racer has no car assigned.");
                return;
            }

            racer.laps = 0;
            racer.nextCheckpoint = 0;
            racer.lapTimerStarted = false;
            racer.lapTime = FormatTime(0f);
        }

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

            Winner = racer.car;
            RaceStarted = false;
            break;
        }

        racers.Sort((a, b) =>
        {
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
    }
}
