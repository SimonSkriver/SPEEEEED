using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfo : MonoBehaviour
{
    public int PlayerID => playerInput.playerIndex;
    public GameObject CarPrefab;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        DontDestroyOnLoad(gameObject);
    }
}