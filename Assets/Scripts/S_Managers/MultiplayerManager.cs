using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();
    [Header("Components (set automatically, visible for debugging purposes)")]
    [SerializeField] private PlayerInputManager playerInputManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
        AllPlayers.Add(FindAnyObjectByType<PlayerInfo>()); // Find Player1 who's persistent from Main Menu scene
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        AllPlayers.Add(playerInput.GetComponent<PlayerInfo>());
        Debug.Log("New player joined with ID: " + playerInput.playerIndex);
    }

    public void OnPlayerLef(PlayerInput playerInput)
    {
        AllPlayers.Remove(playerInput.GetComponent<PlayerInfo>());
        Debug.Log("Player left with ID " + playerInput.playerIndex);
    }
}