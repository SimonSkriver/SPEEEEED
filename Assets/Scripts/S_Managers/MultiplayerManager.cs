using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    public PlayerInfo playerOne;
    public PlayerInputManager playerInputManager;

    private List<PlayerInput> pendingJoinedPlayer = new List<PlayerInput>();
    private bool lobbyReady => LobbyManager.Instance != null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    void Start()
    {
        if (lobbyReady)
        {
            foreach (var player in pendingJoinedPlayer)
            {
                ProcessPlayerJoin(player);
            }
            pendingJoinedPlayer.Clear();
        }
        GetPlayerOne();
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!lobbyReady) // If LobbyManager Instance hasn't been set yet, add the player to a list and handle in Start() when Instance has been set
        {
            pendingJoinedPlayer.Add(playerInput);
            return;
        }
        ProcessPlayerJoin(playerInput);
    }

    void ProcessPlayerJoin(PlayerInput playerInput)
    {
        Debug.Log("New player joined with ID: " + playerInput.playerIndex);
        PlayerInfo playerInfo = playerInput.GetComponent<PlayerInfo>();
        AllPlayers.Add(playerInfo);
        LobbyManager.Instance.AssignPlayerToCarPanel(playerInfo);
        LobbyManager.Instance.UpdateUIText(AllPlayers.Count != playerInputManager.maxPlayerCount);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        AllPlayers.Remove(playerInput.GetComponent<PlayerInfo>());
        if (lobbyReady) LobbyManager.Instance.UpdateUIText(AllPlayers.Count == playerInputManager.maxPlayerCount); // No lobby UI outside the lobby scene
        Debug.Log("Player left with ID " + playerInput.playerIndex);
    }

    public PlayerInfo GetPlayerOne()
    {
        return AllPlayers[0];
    }
}