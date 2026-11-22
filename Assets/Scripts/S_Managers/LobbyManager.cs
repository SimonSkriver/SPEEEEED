using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [SerializeField] CarSelectManager[] carSelectors;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AssignPlayerToCarPanel(PlayerInfo player)
    {
        var panel = carSelectors[player.PlayerID];
        print("Assigning player " + player.PlayerID + " to panel number " + System.Array.IndexOf(carSelectors, panel));
        panel.Bind(player.GetComponent<PlayerInputHandler>());
    }
}