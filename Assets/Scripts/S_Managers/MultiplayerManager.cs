using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    public List<GameObject> AllPlayers = new List<GameObject>();
    [Header("Components (set automatically, visible for debugging purposes)")]
    [SerializeField] private PlayerInputManager playerInputManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    public void AddPlayer(GameObject newPlayer)
    {
        AllPlayers.Add(newPlayer);
    }
}