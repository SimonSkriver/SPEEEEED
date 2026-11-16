using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float recenterSpeed = 5f;

    private CinemachineOrbitalFollow orbitalFollow;
    private InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller lookOrbitX;

    private void Start()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        var inputController = GetComponent<CinemachineInputAxisController>();
        lookOrbitX = inputController.Controllers.Find(controller => controller.Name == "Look Orbit X");
    }

    private void Update()
    {
        if (lookOrbitX.InputValue == 0f)
        {
            orbitalFollow.HorizontalAxis.Value = Mathf.Lerp(
                orbitalFollow.HorizontalAxis.Value,
                orbitalFollow.HorizontalAxis.Center,
                recenterSpeed * Time.deltaTime);
        }
    }
}