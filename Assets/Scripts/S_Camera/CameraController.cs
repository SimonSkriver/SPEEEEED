using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform[] pivotPoints; // [0] = center/base, [1] = left, [2] = right
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float stickDeadzone = 0.1f;
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private Transform orbitCenter;

    private float lookPosition;

    void Start()
    {
        if (playerInput == null || pivotPoints == null || pivotPoints.Length < 3 ||
            pivotPoints[0] == null || pivotPoints[1] == null || pivotPoints[2] == null)
        {
            Debug.LogError("CameraController needs a player input handler and center, left, and right pivot points.", this);
            enabled = false;
            return;
        }

        if (orbitCenter == null)
            orbitCenter = playerInput.transform;

        transform.position = pivotPoints[0].position;
        transform.rotation = pivotPoints[0].rotation;
    }

    void LateUpdate()
    {
        float horizontalInput = playerInput.LookValue.x;
        float targetPosition = Mathf.Abs(horizontalInput) > stickDeadzone
            ? Mathf.Sign(horizontalInput)
            : 0f;
        float blend = 1f - Mathf.Exp(-Mathf.Max(0f, lerpSpeed) * Time.deltaTime);
        lookPosition = Mathf.Lerp(lookPosition, targetPosition, blend);
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        Transform basePivot = pivotPoints[0];
        Transform sidePivot = pivotPoints[lookPosition < 0f ? 1 : 2];
        float t = Mathf.Abs(lookPosition);

        Vector3 origin = orbitCenter.position;
        Vector3 baseOffset = basePivot.position - origin;
        Vector3 sideOffset = sidePivot.position - origin;
        Vector3 baseHorizontal = Vector3.ProjectOnPlane(baseOffset, Vector3.up);
        Vector3 sideHorizontal = Vector3.ProjectOnPlane(sideOffset, Vector3.up);

        Vector3 horizontal;
        if (baseHorizontal.sqrMagnitude < 0.000001f || sideHorizontal.sqrMagnitude < 0.000001f)
        {
            horizontal = Vector3.Lerp(baseHorizontal, sideHorizontal, t);
        }
        else
        {
            float angle = Vector3.SignedAngle(baseHorizontal, sideHorizontal, Vector3.up);
            // Opposite points have two equally short paths; choose the appropriate side.
            if (Mathf.Approximately(Mathf.Abs(angle), 180f))
                angle = lookPosition < 0f ? 180f : -180f;

            float radius = Mathf.Lerp(baseHorizontal.magnitude, sideHorizontal.magnitude, t);
            horizontal = Quaternion.AngleAxis(angle * t, Vector3.up) * baseHorizontal.normalized * radius;
        }

        float height = Mathf.Lerp(baseOffset.y, sideOffset.y, t);
        transform.position = origin + horizontal + Vector3.up * height;
        transform.rotation = Quaternion.Slerp(basePivot.rotation, sidePivot.rotation, t);
    }
}
