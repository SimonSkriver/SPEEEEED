# `SplineConnector.cs`
Namespace: `SplineArchitect`

### `Overview`
SplineConnectors are independent of any spline, and any segment can be connected to them. 
Moving or rotating a SplineConnector will update all connected splines.

### `Class info`
The SplineConnector class consists of a single file: SplineConnector.cs.

### `Properties`
```csharp
public int ConnectionCount { get; private set; }
```

### `Functions`
```csharp
public Segment GetConnectionAtIndex(int index)
public bool ContainsConnection(Spline spline)
public bool ContainsConnection(Segment segment)
```
<hr class="custom-line1">