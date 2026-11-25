using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapSelectManager : MonoBehaviour
{
    private PlayerInputHandler playerInput;
    private int currentMapIndex = 0;
    private List<GameObject> mapHovers = new List<GameObject>();

    [SerializeField] GameObject mapHover;

    void Awake()
    {
        for (int i = 0; i < mapHover.transform.childCount; i++)
        {
            mapHovers.Add(mapHover.transform.GetChild(i).gameObject);
        }
        ShowMap(0);
    }

    void OnEnable()
    {
        playerInput = MultiplayerManager.Instance.GetPlayerOne().GetComponent<PlayerInputHandler>();

        playerInput.MenuLeftPressed += PreviousMap;
        playerInput.MenuRightPressed += NextMap;
        playerInput.SubmitPressed += SelectMap;
    }

    void OnDisable()
    {
        playerInput.MenuLeftPressed -= PreviousMap;
        playerInput.MenuRightPressed -= NextMap;
        playerInput.SubmitPressed -= SelectMap;
    }

    void NextMap()
    {
        currentMapIndex += 1;
        if (currentMapIndex >= mapHover.transform.childCount)
        {
            currentMapIndex = 0;
        }
        ShowMap(currentMapIndex);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.Click, AudioManager.Instance.transform, 1f, Random.Range(0.8f, 1.2f));
    }

    void PreviousMap()
    {
        currentMapIndex -= 1;
        if (currentMapIndex < 0)
        {
            currentMapIndex = mapHover.transform.childCount - 1;
        }
        ShowMap(currentMapIndex);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.Click, AudioManager.Instance.transform, 1f, Random.Range(0.8f, 1.2f));
    }

    void ShowMap(int mapIndex)
    {
        for (int i = 0; i < mapHovers.Count; i++)
        {
            mapHovers[i].SetActive(i == mapIndex);
        }
    }

    void SelectMap()
    {
        foreach (var player in MultiplayerManager.Instance.AllPlayers)
        {
            player.GetComponent<PlayerInput>().ActivateInput(); // Everyone but player 1 was deactivated for map select
            player.SpawnRaceCar();
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.SceneChange, AudioManager.Instance.transform, 1f, 1f);
        SceneLoadManager.Instance.LoadScene(2);
    }
}