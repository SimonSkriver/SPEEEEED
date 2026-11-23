using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfo : MonoBehaviour
{
    public int PlayerID => GetComponent<PlayerInput>().playerIndex;
    public bool ready;
    public GameObject ChosenCar;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}