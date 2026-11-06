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
    }

    [Tooltip("Place these in driving order, with the finish line last.")]
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

        StartRace();
    }

    public void RegisterRacer(GameObject car)
    {
        if (car == null)
        {
            Debug.LogError("Cannot register a missing car.", this);
            return;
        }

        if (RaceStarted || Winner != null)
        {
            Debug.LogWarning("Register racers before starting the race.", this);
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
            Debug.LogWarning("The race has already started or finished.", this);
            return;
        }

        if (racers.Count == 0 || checkpoints == null || checkpoints.Length < 2)
        {
            Debug.LogError("Add a racer, at least one checkpoint, and a finish line before starting.", this);
            return;
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null || !checkpoints[i].enabled ||
                !checkpoints[i].gameObject.activeInHierarchy || !checkpoints[i].isTrigger)
            {
                Debug.LogError("Each checkpoint must be an active trigger collider.", this);
                return;
            }

            for (int j = 0; j < i; j++)
            {
                if (checkpoints[i] == checkpoints[j])
                {
                    Debug.LogError("Each checkpoint must be assigned only once.", this);
                    return;
                }
            }
        }

        foreach (Racer racer in racers)
        {
            if (racer.car == null)
            {
                Debug.LogError("Every racer needs a car.", this);
                return;
            }

            racer.laps = 0;
            racer.nextCheckpoint = 0;
        }

        RaceStarted = true;
    }

    private void Update()
    {
        if (!RaceStarted) return;

        foreach (Racer racer in racers)
        {
            if (racer.car == null)
            {
                Debug.LogError("A racer car is missing; the race has stopped.", this);
                RaceStarted = false;
                return;
            }

            Collider next = checkpoints[racer.nextCheckpoint];
            Vector3 carPosition = racer.car.transform.position;
            if (next.ClosestPoint(carPosition) != carPosition) continue;

            racer.nextCheckpoint++;
            if (racer.nextCheckpoint < checkpoints.Length) continue;

            racer.nextCheckpoint = 0;
            racer.laps++;
            if (racer.laps >= 3)
            {
                Winner = racer.car;
                RaceStarted = false;
                break;
            }
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
