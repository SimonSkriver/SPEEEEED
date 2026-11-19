# `SplineUtility.cs`
Namespace: `SplineArchitect.Utility`

### `Overview`
Utility class for spline construction and analysis, including creation, nearest-spline queries, overlap detection, slicing, auto-smoothing, segment alignment, and rendering resolution calculations.

### `Static Functions`
```csharp
// Creates a spline from a list of points. Several overloads do exist for this function.
public static Spline Create(GameObject go, List<Vector3> segmentPoints, 
										   Color color = default, 
										   Space space = Space.Self)
				
// Creates a spline from a NativeList of NativeSegments. 				
public static Spline Create(GameObject go, NativeList<NativeSegment> nativeSegments, 
										   Color color = default, 
										   Space space = Space.Self)
						
// Creates a spline from two segments defined by their control points.
public static Spline Create(GameObject go, Vector3 anchor1Pos,
						   float tangentA1Dis,
						   float tangentB1Dis,
						   Quaternion rotation1,
						   Vector3 anchor2Pos,
						   float tangentA2Dis,
						   float tangentB2Dis,
						   Quaternion rotation2,
						   Color color = default)
						
// Gets the nearest spline to a world point. 
// Get all registerer splines in the HandleRegsitry class.			
public static Spline GetNearestSpline(Vector3 point, HashSet<Spline> splines, 
													 float distanceToBounds, 
													 out float time, 
													 out Vector3 nearestPoint)
												
public static Spline GetIntersectingSpline(Ray ray, HashSet<Spline> splines)
												
// Finds the closest points between two splines, 
// and returns both points with their corresponding spline times.
// This can be used to detect whether two splines are overlapping.											 
public static SplineClosestResult GetNearestPoints(NativeArray<NativeSegment> segmentsSpline1, 
												   float spline1Length,
												   NativeArray<NativeSegment> segmentsSpline2, 
												   float spline2Length,
												   int precision = 7, 
												   float stepsPer100Meter = 20)		

// Creates a slice of a spline defined by start and end time.
public static NativeList<NativeSegment> CreateSlice(NativeArray<NativeSegment> nativeSegments, 
													float startTime, 
													float endTime, 
													float epsilon = 0.0001f, 
													Allocator allocator = Allocator.TempJob)	

// Computes auto smooth data for inserting a new segment at specific time
// without modifiying the splines shapes. 
public static AutoSmoothData ComputeAutoSmoothData(NativeArray<NativeSegment> nativeSegements, 
												   float time)		
		
// Collects all segments from a set of splines whose 
// anchor point is at the specified world position.	
// Get all registerer splines in the HandleRegsitry class.
public static void GetSegmentsAtPointNoAlloc(List<Segment> closestSegmentContainer, 
											 HashSet<Spline> activeSplines, 
											 Vector3 worldPoint, 
											 float epsilon = 0.001f)	

// Calculates how detailed a spline segment should be 
// rendered based on its length and distance to the camera.
public static int GetSegmentLinesCount(Vector3 anchorA, 
									   Vector3 tangentA, 
									   Vector3 tangentB, 
									   Vector3 anchorB, 
									   float segmentLength, 
									   Camera camera, 
									   bool skipFrustumCheck = false,
									   bool skipViewDistanceCheck = false)	
public static int GetSegmentLinesCount(Segment segmentFrom, Segment segmentTo, Camera camera, 
									   bool skipFrustumCheck = false, 
									   bool skipViewDistanceCheck = false)

// Converts a spline time to the time within the given segment.
public static float GetSegmentTime(int segment, float segmentsCount, float time)

public static void AlignTangents(List<Segment> segments)	

public static NativeSegment CalculateAutoSmoothSegment(float tension, 
													   float roundness, 
													   NativeSegment nativeSegment, 
													   Vector3 prevAnchorPoint, 
													   bool usePrev, 
													   Vector3 nextAnchorPoint, 
													   bool useNext, bool isFirst = false)

public static Vector3 CalculateAutoSmoothTangent(Vector3 previous, 
												 Vector3 current, 
												 Vector3 next, 
												 float tension)							   
```
<hr class="custom-line1">