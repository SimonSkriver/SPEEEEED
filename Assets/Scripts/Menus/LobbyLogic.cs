using UnityEngine;

public class LobbyLogic : MonoBehaviour
{
    [SerializeField] private int playerCount = 1;
    [SerializeField] private int readyCount = 0;

    [SerializeField] private Camera carCam;
    [SerializeField] private Camera mapCam;
    
    void Start()
    {
        carCam.enabled = true;
        mapCam.enabled = false;
    }

    public void MapSelect()
    {
        if (readyCount == playerCount)
        {
            carCam.enabled = false;
            mapCam.enabled = true;
        }
    }

    public void JoinPlayer()
    {
        //On player input
        //Activate and assign PlayerXX elements in hierarchy
        playerCount++;
    }

    public void DisconnectPlayer()
    {
        //On player input
        //Deactivate and unassign PlayerXX elements in hierarchy
        playerCount--;
    }
}


