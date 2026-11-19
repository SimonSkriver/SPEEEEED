using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public int PlayerID;
    public GameObject CarPrefab;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}