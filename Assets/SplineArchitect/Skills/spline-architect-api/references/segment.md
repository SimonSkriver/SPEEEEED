# `Segment.cs`
Namespace: `SplineArchitect`  

### `Overview`
A `Segment` consists of three points: the main anchor point, tangent A, and tangent B.  
Each `Segment` stores a reference to its parent spline (`segment.SplineParent`) and its index in that spline (`segment.IndexInSpline`).

Segments can be created, moved, rotated, removed, linked, or unlinked at runtime. 
They can be linked to other segments on the same spline or on different splines, and they can also be linked to `SplineConnectors`.

They also have various effects that can be adjusted to control how SplineObjects are deformed along the spline, such as rotation, scale, noise layers, and more.

Moving or changing any of the effects of the `Segment` will automatically update the `Spline` and all attached SplineObjects attached.

### `Values`
```csharp
public bool ignoreLink;
public Vector3 connectorPosOffset;
public Vector3 connectorEulerOffset;
```

### `Properties`
```csharp
public float Length { get; private set; }
public float ZPosition { get; private set; }
public LinkTarget LinkTarget { get; private set; }
public int LinkCount { get; private set; }
public bool IsStatic { get; set; }
public bool IgnoreSnapping { get; set; }
public float TangentADistance { get; private set; }
public float TangentBDistance { get; private set; }
public Quaternion Rotation { get; set; }
public Quaternion LocalRotation { get; set; }
public float ZRotation { get; set; }
public float Contrast { get; set; }
public float Noise { get; set; }
public Vector2 SaddleSkew { get; set; }
public Vector2 Scale { get; set; }
public int IndexInSpline { get; private set; }
public Spline SplineParent { get; private set; }
public SplineConnector SplineConnector { get; private set; }
```

### `Functions`
```csharp
public Segment(Spline splineParent, Vector3 position, float distanceTangentA, float distanceTangentB, 
			   Quaternion rotation, Space space, int indexInSpline = -1)

// Creates a static segment.
public Segment(Spline splineParent, Vector3 position, Vector3 tangentA, Vector3 tangentB, 
			   Space space, int indexInSpline = -1)

public Vector3 GetPosition(ControlHandle controlHandle, Space space = Space.World)

// Gets the direction from the anchor to tangent A.
public Vector3 GetDirection(Space space = Space.World)
public void Rotate(Vector3 eulerAngles, Space space = Space.Self)
public void SetPosition(ControlHandle controlHandle, Vector3 newPosition, 
												     Space space = Space.World)										 
public void Translate(ControlHandle controlHandle, Vector3 value, 
												   Space space = Space.World)
public void SetAnchorPosition(Vector3 newPosition, Space space = Space.World)
public void TranslateAnchor(Vector3 value, Space space = Space.World)

// Moves the control handle and preserves the direction of the opposite tangent, 
// but not its length. Moving the anchor translates both tangents along with it.
public void SetContinuousPosition(ControlHandle controlHandle, 
								  Vector3 newPosition, 
								  Space space = Space.World)
							
// Moves the control handle and mirrors the opposite tangent. 
// Moving the anchor also translates both tangents along with it.						
public void SetMirroredPosition(ControlHandle controlHandle, 
								Vector3 newPosition, 
								Space space = Space.World)	

// Moving a tangent does not affect the opposite tangent. 
// Moving the anchor translates both tangents along with it.
public void SetBrokenPosition(ControlHandle controlHandle, 
							  Vector3 newPosition, 
					          Space space = Space.World)	

public void SetTangentDirection(Vector3 direction)
public void ApplyAutoSmooth()
public void ApplyAutoSmooth(Vector3 prevAnchorPos, bool usePrev, Vector3 nextAnchorPos, bool useNext)

// Sets the interpolation type of the segment, 
// switching between a smooth spline curve and a straight line,
// and updates the tangents accordingly.
public void SetInterpolationType(InterpolationType interpolationType)		

// Gets the current interpolation type of the segment (Spline or Line).
public InterpolationType GetInterpolationType()		
public Segment NextSegment()
public Segment PrevSegment()
public Segment GetLinkAtIndex(int index)

// Manually links this segment to another segment at their anchor positions.
public void AddSegmentLink(Segment segment)

// Removes and unlinks a specific segment that is currently linked to this segment.
public void RemoveSegmentLink(Segment segment)
public bool ContainsLink(Segment segment)

// Moves and links this segment to a specific position 
// and connects it with all other segments at the same point.
// If forceLinkUnlinked is true, segments at this point 
// with LinkTarget = None will also be linked.
public bool LinkToSegment(Vector3 point, bool forceLinkUnlinked = true)

// Moves this segment to the specified position 
// and links it to the first SplineConnector found at that point.
public bool LinkToConnector(Vector3 point)

// Unlinks this segment from any other segment or spline connector.
public void Unlink()

// Projects the segment downward onto a surface, using the spline transform's downward direction.
public void ProjectToSurface()
```
<hr class="custom-line1">