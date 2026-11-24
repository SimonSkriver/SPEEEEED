using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverEffect : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("New button selected");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.AudioData.Click, AudioManager.Instance.transform, 0.8f, Random.Range(0.8f, 1.2f));
    }
}
