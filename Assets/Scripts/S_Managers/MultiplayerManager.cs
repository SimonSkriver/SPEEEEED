using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();

    List<PlayerInput> pendingJoinedPlayer = new List<PlayerInput>();
    private bool lobbyReady => LobbyManager.Instance != null;
    public PlayerInputManager playerInputManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

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
        PlayerInfo joinedPlayerInfo = playerInput.GetComponent<PlayerInfo>();
        AllPlayers.Add(joinedPlayerInfo);
        LobbyManager.Instance.AssignPlayerToCarPanel(joinedPlayerInfo);
        LobbyManager.Instance.UpdateUIText(AllPlayers.Count == playerInputManager.maxPlayerCount);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        AllPlayers.Remove(playerInput.GetComponent<PlayerInfo>());
        LobbyManager.Instance.UpdateUIText(AllPlayers.Count == playerInputManager.maxPlayerCount);
        Debug.Log("Player left with ID " + playerInput.playerIndex);
    }
}