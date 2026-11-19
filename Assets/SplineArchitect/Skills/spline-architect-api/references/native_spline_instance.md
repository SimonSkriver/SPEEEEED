# `NativeSplineInstance.cs`
Namespace: `SplineArchitect`  

### `Overview`
A `NativeSplineInstance` is created whenever a `SplineInstance` is added to a `Spline`, either by creating the `SplineInstance` with `spline.CreateSplineInstance()`, 
or by creating it manually and then assigning it to a parent spline using `SplineInstance.SetParent()`.

The `NativeSplineInstance` is designed to be used inside your own Unity Burst jobs. You can set its scale, position, rotation, and more, and it will update automatically.

The `NativeSplineInstance` always has access to its linked `SplineInstance` handler class. However, the `SplineInstance` itself cannot be accessed from within Burst jobs.

### `Struct info`
Consists of one file, NativeSplineInstance.cs.

### `Properties`
```csharp
public float closestSegmentZPosition { get; internal set; }
public int closestSegmentIndex { get; internal set; }
public float3 SplinePosition { get; set; }
public quaternion SplineRotation { get; set; }
public float3 Scale { get; set; }
public bool AlignToEnd { get; set; }
public RotationMode RotationMode { get; set; }
```

### `Functions`
```csharp
public bool DidCrossSegment(float3 prevSplinePosition, float3 splinePosition)
public SplineInstance GetSplineInstance(Spline spline)
```
<hr class="custom-line1">