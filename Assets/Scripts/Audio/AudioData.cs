using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Scriptable Objects/AudioData")]
public class AudioData : ScriptableObject
{
    public AudioClip ReadyUp;
    public AudioClip ButtonHover;
    public AudioClip ButtonClick;
}
