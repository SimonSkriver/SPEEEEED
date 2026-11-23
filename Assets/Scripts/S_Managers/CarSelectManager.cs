using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CarSelectManager : MonoBehaviour
{
    [HideInInspector] public PlayerInputHandler playerInput;
    private PlayerInfo playerInfo;

    [SerializeField] private TMP_Text label;
    [SerializeField] private GameObject[] arrowUIButtons;
    [SerializeField] private GameObject[] inGameCars;

    private int currentCarIndex = 0;
    private List<GameObject> cars = new List<GameObject>();
    private string defaultLabelText;

    void Awake()
    {
        defaultLabelText = label.text;
        for (int i = 0; i < transform.childCount; i++)
        {
            cars.Add(transform.GetChild(i).gameObject);
        }
        foreach (var arrow in arrowUIButtons) arrow.SetActive(false);
    }

    public void Bind(PlayerInputHandler playerInputHandler)
    {
        // Get player info and components
        playerInput = playerInputHandler;
        playerInfo = playerInputHandler.GetComponent<PlayerInfo>();

        //Update visuals
        ShowCar(0);
        foreach (var arrow in arrowUIButtons) arrow.SetActive(true);

        // Hook up input to methods
        playerInput.MenuLeftPressed += PreviousCar;
        playerInput.MenuRightPressed += NextCar;
        playerInput.SubmitPressed += Submit;
        playerInput.BackPressed += Back;
    }

    void OnDisable()
    {
        if (playerInput == null) return;

        playerInput.MenuLeftPressed -= PreviousCar;
        playerInput.MenuRightPressed -= NextCar;
        playerInput.SubmitPressed -= Submit;
        playerInput.BackPressed -= Back;
    }

    void ShowCar(int index)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].SetActive(i == index);
            label.text = cars[index].name;
        }
    }

    public void NextCar()
    {
        if (playerInfo.ready == true) return;

        currentCarIndex += 1;
        if (currentCarIndex >= transform.childCount)
        {
            currentCarIndex = 0;
        }
        ShowCar(currentCarIndex);
    }

    public void PreviousCar()
    {
        if (playerInfo.ready == true) return;

        currentCarIndex -= 1;

        if (currentCarIndex < 0)
        {
            currentCarIndex = transform.childCount - 1;
        }
        ShowCar(currentCarIndex);
    }

    void Submit()
    {
        if (playerInfo.ready == false)
        {
            if (playerInfo.ChosenCar == null) playerInfo.ChosenCar = inGameCars[currentCarIndex];
            playerInfo.ready = true;
            LobbyManager.Instance.UpdateReadyPlayerList(playerInfo.PlayerID);
            foreach (var arrow in arrowUIButtons) arrow.SetActive(false);
        }
    }

    void Back()
    {
        if (playerInfo.ready == true)
        {
            playerInfo.ready = false;
            playerInfo.ChosenCar = null;
            LobbyManager.Instance.UpdateReadyPlayerList(playerInfo.PlayerID);
            foreach (var arrow in arrowUIButtons) arrow.SetActive(true);
        }
        else // If the player presses back while not ready, the player quits
        {
            foreach (var car in cars) car.SetActive(false);
            foreach (var arrow in arrowUIButtons) arrow.SetActive(false);
            label.text = defaultLabelText;
            playerInfo.ready = false;
            MultiplayerManager.Instance.OnPlayerLeft(playerInput.GetComponent<PlayerInput>());
            Destroy(playerInfo.gameObject);
        }
    }
}