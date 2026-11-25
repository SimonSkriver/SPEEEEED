using UnityEngine;
using UnityEngine.Audio;

public class CarSFX : MonoBehaviour
{
    //Audio stuff
    [SerializeField] AudioMixerGroup audioMixer;
    private AudioSource engineAudioSource;
    private AudioSource tireSquealAudioSource;
    private AudioSource speedRattleAudioSource;
    [SerializeField] private AudioClip engineSFX;
    [SerializeField] private AudioClip gearshiftSFX;
    [SerializeField] private AudioClip TireSquealSFX;
    [SerializeField] private AudioClip SpeedRattleSFX;
    [SerializeField] private AnimationCurve powerCurve;
    
    //External references
    private Rigidbody rb;
    //private CarWheel carData;
    [SerializeField] private float traction = 1f; //Add current tire grip from car instead
    private float currentSpeed;
    private PlayerInputHandler playerInput;

    //Properties
    private float throttleValue;
    private float breakValue;
    private float currentRPM = 0f;
    [SerializeField] private float idleRPM = 800f;
    [SerializeField] private float maxRPM = 7000f;
    [SerializeField] private float shiftUpRPM = 6500f;
    [SerializeField] private float shiftDownRPM = 3500f;
    [SerializeField] private float shiftRate = 1f;
    [SerializeField] private int maxGears = 6;
    [SerializeField] private int currentGear = 1;

    void Awake()
    {
        //Get player input for throttle
        playerInput = GetComponentInParent<PlayerInputHandler>();
        rb = GetComponentInParent<Rigidbody>();
        //carData = GetComponentInChildren<CarWheel>();

        //Adds required audio sources
        HandleAudioSources();
    }

    void Update()
    {
        throttleValue = playerInput.AccelerationInputValue;
        breakValue = playerInput.BrakeInputValue;
        currentSpeed = rb.linearVelocity.magnitude;
        //traction = carData.CarData.TireGrip;

        HandleGearShifting();
        HandleEngineSound();
        HandleTireSqueal();
        HandleSpeedRattle();
    }
    
    private void HandleEngineSound()
    {
        //This thing updates the emulated RPM based on throttle input. It's modified by the gear index, emulating the effect of gear shifting and acceleration. The higher the gear the longer it takes to reach max RPM and engine goes vrrr at slower rates.

        if (throttleValue > 0)
        {
            //Some math bullshit that makes the rpm increase slower in higher gears, by lerping from currentRPM to target RPM calculated from throttle input, at a rate based on a percentage decrease per gear. That's the 0.85 part.
            //Shift rate can be used as an overall speed adjustement.
            //Should probalby be based on power curve or movement speed instead
            
            currentRPM = Mathf.Lerp(currentRPM, throttleValue * maxRPM, Time.deltaTime * (shiftRate * Mathf.Pow(0.75f, currentGear - 1)));
        }
        else
        {
            //If no throttle, decrease rpm back to idle, going through the gears.
            currentRPM = Mathf.Lerp(currentRPM, idleRPM, Time.deltaTime * (shiftRate * Mathf.Pow(0.80f, currentGear - 1)));
        }

        //Change pitch of SFX based on RPM. CurrentRPM divded by maxRPM to find correct ratio for lerp
        engineAudioSource.pitch = Mathf.Lerp(0.25f, 1.25f, currentRPM / maxRPM);
    }

    private void HandleGearShifting()
    {

        //Emulating gearshifting by setting RPM threshholds kinda like a limiter in a car. Gears are cycled through and fed to HandleEngineSound
        if (currentRPM >= shiftUpRPM && currentGear < maxGears)
        {
            //engineAudioSource.PlayOneShot(gearshiftSFX); //SFX sounds like ass. Find another
            
            currentGear++;
            //When shifting gear, lower RPM by 2000 as if decoupling
            currentRPM = Mathf.Max(idleRPM, currentRPM - 2000f);
        }
        else if (currentRPM <= shiftDownRPM && currentGear > 1)
        {
            // Play gear shift sound here??

            currentGear--;
            //currentRPM = Mathf.Lerp(currentRPM, currentRPM + 1000f, Time.deltaTime * 5f);
            currentRPM = Mathf.Max(idleRPM, currentRPM + 500f);
        }
    }

    private void HandleTireSqueal()
    {
        //Tires be squealing. Inverse relationship between volume and traction, clamped from 0-1. Traction of 0 = loud squeal, and 1 = no squeal
        traction = breakValue;
        tireSquealAudioSource.volume = 0f + Mathf.Clamp01(traction);
    }

    private void HandleSpeedRattle()
    {
        speedRattleAudioSource.volume = Mathf.Clamp01(currentSpeed);
    }

    private void HandleAudioSources()
    {
        //There's probably a smarter way of doing this. Maybe a AudioSource prefab with predefined settings.

        //Create and configure audio source for engine SFX
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.outputAudioMixerGroup = audioMixer;
        engineAudioSource.clip = engineSFX;
        engineAudioSource.loop = true;
        //engineAudioSource.spatialBlend = 1f;
        engineAudioSource.Play();

        //Create and configure audio source for tire squeal SFX
        tireSquealAudioSource = gameObject.AddComponent<AudioSource>();
        tireSquealAudioSource.outputAudioMixerGroup = audioMixer;
        tireSquealAudioSource.clip = TireSquealSFX;
        tireSquealAudioSource.loop = true;
        //tireSquealAudioSource.spatialBlend = 1f;
        tireSquealAudioSource.volume = 0f;
        tireSquealAudioSource.Play();

        //Create and configure audio source for speed rattles SFX
        speedRattleAudioSource = gameObject.AddComponent<AudioSource>();
        speedRattleAudioSource.outputAudioMixerGroup = audioMixer;
        speedRattleAudioSource.clip = SpeedRattleSFX;
        speedRattleAudioSource.loop = true;
        //speedRattleAudioSource.spatialBlend = 1f;
        speedRattleAudioSource.volume = 0f;
        speedRattleAudioSource.Play();
    }
}
