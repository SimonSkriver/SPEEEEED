using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car")]
public class CarData : ScriptableObject
{
    [Header("Driving")]
    public float EnginePower;
    public float TopSpeed;
    public AnimationCurve PowerCurve;

    [Header("Maneuvering")]
    [Tooltip("How sharply the car can turn (measured in degrees to each side) at no speed and top speed. Typically set to 30 and 10 at max speed")]
    public float SteeringRange;
    public float SteeringRangeAtTopSpeed;
    [Range(0, 1)]
    public float TireGrip;
    [Range(0, 1)]
    public float FrontWheelDriftingTireGrip;
    [Range(0, 1)]
    public float RearWheelDriftingTireGrip;

    [Header("Braking and reversing")]
    public float BrakeForce;
    public float ReversingTopSpeed;
}