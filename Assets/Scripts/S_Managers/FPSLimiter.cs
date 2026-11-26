using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField] int targetFPS = 120;
    void Awake()
    {
       QualitySettings.vSyncCount = 0;
       Application.targetFrameRate = targetFPS; 
    }

    
}
