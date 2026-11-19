# `SplineObjectWorker.cs`
Namespace: `SplineArchitect.Workers`

### `Overview`
Worker class responsible for placing and deforming `SplineObject`s along a `Spline`.

By setting `spline.jobStartType = JobType.DO_NOTHING` and `spline.jobEndType = JobType.DO_NOTHING`, you can disable the spline’s internal deformation system, 
create your own `SplineObjectWorker`, and fully control when and how spline object deformation is processed.

### `Functions`
```csharp
public SplineObjectWorker(Spline spline = null)
public void Add(SplineObject so)
public void Start()
public void Complete()
public void CompleteWithoutAssignData()
public bool Contains(SplineObject so)
public void Deform(SplineObject so, bool updateExternalComponents = false)
public void SetSpline(Spline spline)
public bool IsWorking()
public bool HasWork()
public int GetVerticesCount()
```
<hr class="custom-line1">