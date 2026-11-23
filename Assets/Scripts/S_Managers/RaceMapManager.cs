using UnityEngine;
using UnityEngine.InputSystem;

public class RaceMapManager : MonoBehaviour
{
    public static RaceMapManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var player in MultiplayerManager.Instance.AllPlayers)
        {
            player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        }
    }
}