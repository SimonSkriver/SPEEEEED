# `PointWorker.cs`
Namespace: `SplineArchitect.Workers`

### `Overview`
Worker class for converting spline-space positions to world-space positions.

### `Functions`
```csharp
public PointWorker(Spline spline = null)
public void Add(Vector3 splinePosition)
public void Add(Vector3 splinePosition, float4x4 matrix, bool alignToEnd, RotationMode rotationMode)
public override bool Start()
public override void Complete()
public void Complete(List<PointWorkerData> pointWorkerData, int maxAmount = int.MaxValue)
public void Complete(out PointWorkerData p0)
public void Complete(out PointWorkerData p0, out PointWorkerData p1)
public void Complete(out PointWorkerData p0, out PointWorkerData p1, out PointWorkerData p2)
public void Complete(out PointWorkerData p0, out PointWorkerData p1, out PointWorkerData p2, 
					 out PointWorkerData p3)
public void Complete(out PointWorkerData p0, out PointWorkerData p1, out PointWorkerData p2, 
					 out PointWorkerData p3, out PointWorkerData p4)
public override int GetWorkCount()
public override void CompleteWithoutAssignData()
public override void DisposeNativeData()
```
<hr class="custom-line1">