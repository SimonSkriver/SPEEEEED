using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Car")]
public class TestCarData : ScriptableObject
{
    [Header("Car stats")]
    public float EnginePower;
    public float SteeringRange;
    public float TopSpeed;
    public float TireGrip;
}