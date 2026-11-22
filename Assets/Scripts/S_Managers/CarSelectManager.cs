using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CarSelectManager : MonoBehaviour
{
    [HideInInspector] public PlayerInputHandler player;

    [SerializeField] private TMP_Text label;

    private int currentCarIndex = 0;
    private List<GameObject> cars = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            cars.Add(transform.GetChild(i).gameObject);
        }
        label.text = null;
    }

    public void Bind(PlayerInputHandler playerInputHandler)
    {
        SelectCar(0);
        player = playerInputHandler;
        player.MenuLeftPressed += Previous;
        player.MenuRightPressed += Next;
    }

    void OnDisable()
    {
        if (player == null) return;

        player.MenuLeftPressed -= Previous;
        player.MenuRightPressed -= Next;
    }

    void SelectCar(int index)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].SetActive(i == index);
            label.text = cars[index].name;
        }
    }

    void Next()
    {
        currentCarIndex += 1;
        if (currentCarIndex >= transform.childCount)
        {
            currentCarIndex = 0;
        }
        SelectCar(currentCarIndex);
    }

    void Previous()
    {
        currentCarIndex -= 1;

        if (currentCarIndex < 0)
        {
            currentCarIndex = transform.childCount - 1;
        }
        SelectCar(currentCarIndex);
    }
}