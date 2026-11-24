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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.PlayerJoinPing, transform, 1f, 1f);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        AllPlayers.Remove(playerInput.GetComponent<PlayerInfo>());
        LobbyManager.Instance.UpdateUIText(AllPlayers.Count == playerInputManager.maxPlayerCount);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.Back, transform, 1f, 1f);
        Debug.Log("Player left with ID " + playerInput.playerIndex);
    }

    public PlayerInfo GetPlayerOne()
    {
        if (AllPlayers == null) return null;
        else return AllPlayers[0];
    }
}