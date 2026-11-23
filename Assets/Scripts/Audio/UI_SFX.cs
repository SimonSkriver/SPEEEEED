using UnityEngine;

public class UI_SFX : MonoBehaviour
{
    [SerializeField] private Vector2 pitchMinMax;
    
    public void SFX(AudioClip sfx)
    {
        AudioManager.Instance.PlaySFX(sfx, transform, 1f, 1f);
    }

    public void SFX_RandomPitch(AudioClip sfx)
    {
        AudioManager.Instance.PlaySFX(sfx, transform, 1f, Random.Range(pitchMinMax.x, pitchMinMax.y));
    }
}
