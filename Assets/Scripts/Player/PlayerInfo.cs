using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfo : MonoBehaviour
{
    public int PlayerID => GetComponent<PlayerInput>().playerIndex;
    public bool ready;
    public GameObject ChosenCar; // The prefab picked in car select
    [HideInInspector] public GameObject RaceCar; // The spawned instance, a child of this player so its wheels find our PlayerInputHandler

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Spawned inactive; the race start sequence places and activates it
    public void SpawnRaceCar()
    {
        if (RaceCar != null || ChosenCar == null) return;

        RaceCar = Instantiate(ChosenCar, transform);
        RaceCar.SetActive(false);
    }

    public void DestroyRaceCar()
    {
        if (RaceCar != null) Destroy(RaceCar);
        RaceCar = null;
    }
}