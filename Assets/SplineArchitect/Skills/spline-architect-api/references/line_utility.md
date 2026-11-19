# `LineUtility.cs`
Namespace: `SplineArchitect.Utility`  

### `Overview`
Utility class for line projection and closest-point calculations (point-to-line and line-to-line), with optional `time` outputs.

### `Static Functions`
```csharp
public static Vector3 GetNearestPointOnLineFromLine(Vector3 line1Point, 
                                                    Vector3 line1Direction, 
                                                    Vector3 line2Point, 
                                                    Vector3 line2Direction, 
                                                    float maxLength = 0, 
                                                    bool noBackwards = false)

public static Vector3 GetNearestPointOnLineFromLine(Vector3 line1Point, 
													Vector3 line1Direction, 
													Vector3 line2Point, 
													Vector3 line2Direction, 
													out float time)

public static Vector3 GetNearestPoint(Vector3 startPoint, 
									  Vector3 direction, 
									  Vector3 point, 
									  out float time)		

public static Vector3 GetNearestPoint(Vector3 startPoint, 
									  Vector3 direction, 
									  Vector3 point, 
									  float minTime, 
									  float maxTime)									  
```
<hr class="custom-line1">