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
        [HideInInspector] public float finalLapTime;
        [HideInInspector] public bool lapTimerStarted;
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
            racer.finalLapTime = 0f;
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

            // Handle lap timing
            if (!racer.lapTimerStarted)
            {
                // Starting lap 1 - start the timer
                racer.lapStartTime = Time.time;
                racer.lapTimerStarted = true;
                Debug.Log($"{racer.car.name} started lap timer at {racer.lapStartTime}");
            }
            else if (racer.laps == 3)
            {
                // Finished lap 3 - stop timer and print final time
                racer.finalLapTime = Time.time - racer.lapStartTime;
                Debug.Log($"{racer.car.name} finished in {racer.finalLapTime:F2} seconds");
            }

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
