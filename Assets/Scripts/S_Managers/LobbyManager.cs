using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [SerializeField] CarSelectManager[] carSelectors;
    [SerializeField] CinemachineCamera carSelectCam;
    [SerializeField] CinemachineCamera mapSelectCam;
    [SerializeField] GameObject joinText;

    public List<int> ReadyPlayers = new List<int>();
    private MapSelectManager mapSelectManager;

    private bool allPlayersReady
    {
        get
        {
            if (MultiplayerManager.Instance == null) return false;
            return ReadyPlayers.Count == MultiplayerManager.Instance.AllPlayers.Count;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            mapSelectManager = GetComponent<MapSelectManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AssignPlayerToCarPanel(PlayerInfo player)
    {
        var panel = carSelectors[player.PlayerID];
        print("Assigning player " + player.PlayerID + " to panel number " + System.Array.IndexOf(carSelectors, panel));
        panel.Bind(player.GetComponent<PlayerInputHandler>());
    }

    public void UpdateUIText(bool state)
    {
        joinText.SetActive(state);
    }

    public void UpdateReadyPlayerList(int playerID)
    {
        if (!ReadyPlayers.Contains(playerID)) ReadyPlayers.Add(playerID);
        else ReadyPlayers.Remove(playerID);

        if (allPlayersReady)
        {
            Continue();
            Debug.Log("Continue");
        }
        else Return();
    }

    public void Continue()
    {
        mapSelectCam.enabled = true;
        carSelectCam.enabled = false;
        mapSelectManager.enabled = true;
        MultiplayerManager.Instance.playerInputManager.DisableJoining();
        UpdateUIText(false);
        foreach (var player in MultiplayerManager.Instance.AllPlayers) // Disable input for everyone but player 1
        {
            if (player.PlayerID != 0)
            {
                player.GetComponent<PlayerInput>().DeactivateInput();
            }
        }
    }

    public void Return()
    {
        carSelectCam.enabled = true;
        mapSelectCam.enabled = false;
        mapSelectManager.enabled = false;
        MultiplayerManager.Instance.playerInputManager.EnableJoining();
        if (MultiplayerManager.Instance.AllPlayers.Count != MultiplayerManager.Instance.playerInputManager.maxPlayerCount) UpdateUIText(true);
        foreach (var player in MultiplayerManager.Instance.AllPlayers) // Disable input for everyone but player 1
        {
            player.GetComponent<PlayerInput>().ActivateInput();
        }
    }
}