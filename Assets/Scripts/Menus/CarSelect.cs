using UnityEngine;
using TMPro;

public class CarSelect : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    
    private int currentCarIndex;

    private void Awake()
    {
        SelectCar(0);
    }

    private void SelectCar(int index)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == index);
            label.text = transform.GetChild(index).name;
        }
    }

    public void ChangeCar(int changeIncrement)
    {
        currentCarIndex += changeIncrement;
        if (currentCarIndex < 0)
        {
            currentCarIndex = transform.childCount-1;
        }
        else if (currentCarIndex >= transform.childCount)
        {
            currentCarIndex = 0;
        }
        SelectCar(currentCarIndex);
    }
}