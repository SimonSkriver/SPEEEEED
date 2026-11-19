# `InstanceBatch.cs`
Namespace: `SplineArchitect`  

### `Overview`
`InstanceBatch`es are automatically created and managed by `Spline`s within the spline's instance system. 
They are mainly used to store `NativeSplineInstance`s so they can be iterated over and processed inside user-created Burst jobs.

### `Class info`
Consists of one file, InstanceBatch.cs.

### `Properties`
```csharp
public Bounds Bounds { get; set; }
public NativeList<NativeSplineInstance> NativeSplineInstances { get; private set; }
public int Count { get; private set; }
```

<hr class="custom-line1">