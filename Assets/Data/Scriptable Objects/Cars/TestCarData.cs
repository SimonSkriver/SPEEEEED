using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car")]
public class TestCarData : ScriptableObject
{
    [Header("Driving")]
    public float EnginePower;
    public float TopSpeed;

    [Header("Maneuvering")]
    [Tooltip("How sharply the car can turn, measured in degrees to each side")]
    public float SteeringRange;
    public float TireGrip;

    [Header("Braking and reversing")]
    public float BrakeForce;
    public float ReversingMaxSpeed;
}