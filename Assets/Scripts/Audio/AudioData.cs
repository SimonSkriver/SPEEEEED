using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Scriptable Objects/AudioData")]
public class AudioData : ScriptableObject
{
    public AudioClip Back;
    public AudioClip Click;
    public AudioClip Confirm;
    public AudioClip SceneChange;
    public AudioClip PlayerJoinPing;
}
