# `GeneralUtility.cs`
Namespace: `SplineArchitect.Utility`  

### `Overview`
General utility class providing epsilon-based comparisons, zero checks, rounding, rotation helpers, and world/local bounds transformation functions.

### `Static Functions`
```csharp
public static bool IsEqual(Quaternion a, Quaternion b, float epsilon = 0.0001f)		
public static bool IsEqual(Bounds a, Bounds b, float epsilon = 0.0001f)
public static bool IsEqual(Color a, Color b, float epsilon = 0.0001f)
public static bool IsEqual(Rect a, Rect b, float epsilon = 0.0001f)
public static bool IsEqual(Vector3 a, Vector3 b, float epsilon = 0.0001f)
public static bool IsEqualFast(Quaternion a, Quaternion b, float epsilon = 0.0001f)
public static bool IsEqualFast(Vector3 a, Vector3 b, float epsilon = 0.0001f)
public static bool IsEqual(float a, float b, float epsilon = 0.0001f)
public static bool IsZero(float value, float epsilon = 0.0001f)
public static bool IsZero(double value, double epsilon = 0.00000000001f)
public static bool IsZero(Vector3 a, float epsilon = 0.0001f)
public static bool IsZero(Quaternion a, float epsilon = 0.0001f)
public static bool IsZeroFast(float2 value, float epsilon = 0.0001f)
public static bool IsInsideBoundsIgnoringY(Bounds bounds, Vector3 point)

// Returns true if the point is visible within the camera's view.
public static bool PointInsideCameraFrustum(Vector3 point, Camera camera)

// The angle in degrees between two directions, signed by the given axis.
public static float SignedAngle(float3 from, float3 to, float3 axis)
public static float RoundToClosest(float value, float roundPoint)
public static Vector3 RoundToClosest(Vector3 value, float roundPoint)
public static Vector3 RotateAroundCenter(Vector3 point, Vector3 center, Vector3 upDirection, float amount)
public static Vector3 RotateAroundCenterXZ(Vector3 point, Vector3 center, float amount)
public static Bounds TransformBounds(Bounds localBounds, float4x4 aToB)
public static Bounds TransformBoundsToLocalSpace(Bounds worldBounds, Transform t)
public static Bounds TransformBoundsToWorldSpace(Bounds localBounds, Transform t)
```
<hr class="custom-line1">