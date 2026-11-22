using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfo : MonoBehaviour
{
    public int PlayerID => GetComponent<PlayerInput>().playerIndex;
    public GameObject CarPrefab;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}