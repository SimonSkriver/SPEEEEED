using Unity.Cinemachine;
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

        // The wheels look for our PlayerInputHandler in Awake, so the car is created under an inactive holder.
        // Its Awake then only runs once it is parented to us and activated.
        GameObject holder = new GameObject("RaceCarHolder");
        holder.SetActive(false);
        RaceCar = Instantiate(ChosenCar, holder.transform);
        RaceCar.SetActive(false);
        RaceCar.transform.SetParent(transform, false);
        Destroy(holder);
        AssignCameraChannel();
    }


    void AssignCameraChannel()
    {
        OutputChannels channel = (OutputChannels)((int)OutputChannels.Channel01 << PlayerID); // Player 1 = Channel01, Player 2 = Channel02, ...

        foreach (CinemachineBrain brain in RaceCar.GetComponentsInChildren<CinemachineBrain>(true))
        {
            brain.ChannelMask = channel;
        }

        foreach (CinemachineVirtualCameraBase cam in RaceCar.GetComponentsInChildren<CinemachineVirtualCameraBase>(true))
        {
            cam.OutputChannel = channel;
        }

        foreach (CinemachineInputAxisController axis in RaceCar.GetComponentsInChildren<CinemachineInputAxisController>(true))
        {
            axis.PlayerIndex = GetComponent<PlayerInput>().user.index;
        }
    }

    public void DestroyRaceCar()
    {
        if (RaceCar != null) Destroy(RaceCar);
        RaceCar = null;
    }
}