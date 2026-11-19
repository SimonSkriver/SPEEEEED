# `Spline.cs` 
Namespace: `SplineArchitect`  

### `Overview`
A `Spline` consists of two or more segments. All segments are stored in the `Spline`, and you can create or remove them using the `Spline` class.  

Each spline manages its own `SplineObjects`, and disabling the Spline component will disable all moving SplineObjects on that spline.

Because each `Spline` handles its own runtime logic, Spline Architect can easily be excluded from specific scenes, as no global components or GameObjects are used.

### `Class info`
The Spline class is a partial class split across 8 files: Spline.cs, Spline_CachedData.cs, Spline_Deformation.cs, Spline_Instancing.cs, Spline_Population.cs, Spline_Editor.cs, Spline_Event.cs, and Spline_Lifecycle.cs.
Only Spline.cs, Spline_Population.cs, Spline_CachedData.cs, Spline_Instancing.cs, Spline_Deformation.cs, and Spline_Event.cs contain public fields, properties, events, or methods intended for use by the user.
If the Terrain Tools addon is installed, Spline_TerrainTools.cs is also part of the Spline class and contains the runtime terrain deformation.

### `Values`
```csharp
// A list of noise layers that can be added or removed to affect spline deformation.
// The spline will automatically detect changes to this list and update accordingly.
public List<NoiseLayer> noises;
// Set what camera the spline line should use for different calculations.
public Camera lineRenderCamera;
public Color color;
public NoiseGroup noiseGroup;
public AnimationCurve scaleProfileCurve;
```

### `Properties`
```csharp
// Indicates whether the spline is looping. 
// Use spline.SetLoop() to enable or disable looping.
public bool Loop { get; private set; }

// The width of the rendered spline in the editor 
// and in game (when RenderInGame is true).
public float Width { get; set; }

// Controls whether the spline is occluded in the 
// editor and in game (when RenderInGame is true).
public bool Occluded { get; set; }

// The resolution of the spline when rendered in game.
public float LineResolution { get; set; }

// The LineRenderer component used for drawing the spline 
// in Play Mode and in the built game (when RenderInGame is true).
public LineRenderer lineRenderer { get; private set; }

// The material the LineRenderer uses for drawing the spline 
// in Play Mode and in the built game (when RenderInGame is true).
public Material RenderMaterial { get; set; }
public float Length { get; private set; }
public bool RenderInGame { get; set; }
public ComponentMode ComponentMode { get; set; }
public int SegmentCount { get; private set; }
public int AllSplineObjectCount { get; private set; }
public int RootSplineObjectCount { get; private set; }
```

### `Functions`
```csharp
public Segment GetSegmentAtIndex(int index)
public void AddSegment(Segment segment)
public void InsertSegment(int index, Segment segment)
public Segment CreateSegment(Vector3 anchorPos, 
							 float tangentADis,
							 float tangentBDis, 
							 Space space = Space.World)
public Segment CreateSegment(Vector3 anchorPos, 
							 float tangentADis,
							 float tangentBDis, 
							 Quaternion rotation, Space space = Space.World)
public Segment CreateSegment(int index, Vector3 anchorPos, 
							 float tangentADis,
							 float tangentBDis, 
							 Quaternion rotation, Space space = Space.World)
							 
// Creates a new static segment. Static segments use the spline transform's up direction
// as the base rotation when normals are created. No cached normals are needed, which can
// improve performance. However, static segments will not work well with loops or similar shapes.
public Segment CreateStaticSegment(Vector3 anchorPos,
								   Vector3 tangentAPos,
								   Vector3 tangentBPos,
								   Space space = Space.World)
public Segment CreateStaticSegment(int index, Vector3 anchorPos,
								   Vector3 tangentAPos,
								   Vector3 tangentBPos,
								   Space space = Space.World)

// Creates a new segment at the specified time without changing the spline's shape.
public Segment CreateSegmentAutoSmooth(float time)
public bool ContainsSegment(Segment segment)
public void RemoveSegment(Segment segment)
public void RemoveSegmentAt(int index)
public void ReverseSegments()
public int GetSegment(float time)
public SplineObject GetSplineObjectAtIndex(int index)
public SplineObject GetRootSplineObjectAtIndex(int index)
public bool ContainsSplineObject(SplineObject so)

// Gets the position on a specific segment of the spline.
public Vector3 GetPositionOnSegment(int segment, float time, Space space = Space.World)
public Vector3 GetPosition(float time, Space space = Space.World)

// Gets the local position from the cached position map. 
public Vector3 GetPositionFastLocal(float time)

// Like GetPosition(), but also supports values below 0 and above 1 by 
// extending the spline from the start or end anchor.
public Vector3 GetPositionExtended(float time)
													   
// Finds the closest points between this spline and another spline, 
// and returns both points with their corresponding spline times.
// This can be used to detect whether two splines are overlapping.
public SplineClosestResult GetNearestPoints(Spline spline, int precision = 7, 
														   float stepsPer100Meter = 20)		   
											
// Gets the closest point on the spline to a world position.					
public Vector3 GetNearestPoint(Vector3 worldPoint)

// Gets the closest point on the spline to a world position 
// and outputs the corresponding spline time.
public Vector3 GetNearestPoint(Vector3 worldPoint, out float timeValue, 
												   float startTime = 0, 
												   float endTime = 1, 
												   int precision = 8, 
												   float steps100Meter = 5, 
												   bool ignoreYAxel = false, 
												   bool useFixedTime = false)
										
// Gets the closest point on the spline to a world position using fixed time, 
// and outputs the corresponding fixedTime.
// Fixed time maps to the exact distance along the spline.
public Vector3 GetNearestPointFixedTime(Vector3 worldPoint, out float fixedTime)	
public Vector3 GetNearestPointFixedTime(Vector3 worldPoint, out float fixedTime, 
															int precision = 8, 
															float stepsPer100Meter = 5, 
															bool ignoreYAxel = false)
							
// Gets the nearest time on the spline from a world point.
public float GetNearestTime(Vector3 worldPoint, int precision = 8, 
												float steps100Meter = 5)
public float GetNearestTime(Vector3 worldPoint, float startTime, float endTime, 
												int precision = 8, 
												float steps100Meter = 5, 
												bool ignoreYAxel = false, 
												bool useFixedTime = false)

// Gets the nearest fixed time on the spline from a world point.
// Fixed time maps to the exact distance along the spline.
public float GetNearestFixedTime(Vector3 worldPoint, int precision = 8, 
													 float steps100Meter = 5)	
public Vector3 GetDirection(float time, Space space = Space.World)

// Converts normalized time to fixed time. Fixed time is the corrected parameter that
// corresponds to the actual distance along the spline.
// For example, to get the world position exactly halfway along the spline length:
// float fixedTime = TimeToFixedTime(0.5f);
// Vector3 worldPoint = spline.GetPosition(fixedTime);
public float TimeToFixedTime(float time)

// Converts fixed time back to regular spline time, which is not correlated with distance.
public float FixedTimeToTime(float fixedTime)

// Gets the center point of all control point anchors on the spline.
public Vector3 GetCenter(Space space = Space.World)
public NormalFrame GetNormalFrame(float time, 
								  Space space = Space.World, 
								  RotationMode rotationMode = RotationMode.FOLLOW_SPLINE)

// Gets the interpolated Z rotation (in degrees) along the spline at the specified time.					  
public float GetZRotationDegrees(float time)
public void SetLoop(bool enable, bool modifySegments = true)
public void Join(Spline spline, JoinType joinType)
public void Split(Spline newSpline, int segmentIndex)

// Detects if a spline object has crossed a segment that contains links 
// between the previous and current spline positions.
// Call this every frame and check if the links list contains any segments. 
// If it does, you can cross over to another
// spline by using: splineObject.transform.parent = newSegment.SplineParent.transform.
// For an exact position on the new spline without any visual lag, 
// use CalculateLinkCrossingZPosition() to set the spline object's new position.
public void FindLinkCrossingsNonAlloc(List<Segment> links, Vector3 splinePosition, 
                                                    Vector3 previousSplinePosition, 
                                                    LinkFlags linkFlags, 
                                                    out Segment currentSegment)
public void FindLinkCrossingsNonAlloc(List<Segment> links, Vector3 splinePosition, 
                                                    Vector3 previousSplinePosition)
													
// After detecting a link crossing with FindLinkCrossingsNonAlloc(),
// use this to calculate the spline object's new Z position on the spline it crossed to.
// This helps prevent visual lag when switching to a new spline.											
public float CalculateLinkCrossingZPosition(Vector3 splinePosition, Segment fromSegment, 
																    Segment toSegment)
													
// Creates and initializes a SplineObject deformation on the 
// given GameObject and registers it to this spline.										
public SplineObject CreateDeformation(GameObject go, Vector3 localSplinePosition, 
													 Quaternion localSplineRotation, 
													 Transform parent = null,
													 bool worldPositionStays = true)
		
// Creates and initializes SplineObject deformations for all
// children of a given GameObject and registers them to this spline.
public void CreateDeformationsForChildren(GameObject parentGameObject,
								          List<SplineObject> childrenResult)
		
// Creates and initializes a SplineObject follower on the 
// given GameObject and registers it to this spline.
public SplineObject CreateFollower(GameObject go, Vector3 localSplinePosition, 
												  Quaternion localSplineRotation, 
												  Transform parent = null,
												  bool worldPositionStays = true)													 

// Creates and initializes a SplineInstance 
// and registers it to this spline.
public SplineInstance CreateSplineInstance(Mesh mesh, List<Material> materials, float3 splinePosition, quaternion splineRotation, Vector3 scale)
public SplineInstance CreateSplineInstance(Mesh mesh, Material material, float3 splinePosition, quaternion splineRotation, Vector3 scale)

// Finds the closest spline objects to a given position along 
// the spline and stores the results in the provided list.
// It can also search on linked splines and filter results 
// by direction and other options using the given flags.
// This is typically used for things like collision checks 
// in traffic or movement systems.
public void DistanceToClosestSplineObjectNonAlloc(List<SplineSearchResult> searchResults, 
                                                  Vector3 searchSplinePosition, 
                                                  int maxResults, 
                                                  SplineSearchResultFlags searchFlags, 
                                                  bool includeNestedSplineObjects = false, 
                                                  bool includeIgnoredLinks = true)
public Vector3 SplinePositionToWorldPosition(Vector3 splinePosition, 
											 Transform parentTransform, 
											 float4x4 matrix, 
											 bool alignToEnd = false,
											 RotationMode rotationMode = RotationMode.FOLLOW_SPLINE)											 
public Vector3 SplinePositionToWorldPosition(Vector3 splinePosition)
public Vector3 WorldPositionToSplinePosition(Vector3 worldPosition, int precision = 8, 
																	float steps = 5,
																	bool canExtend = true)
public Quaternion SplineRotationToWorldRotation(Quaternion rotation, float time)	
public Quaternion WorldRotationToSplineRotation(Quaternion rotation, float time)	
public void TransformToCenter(out Vector3 dif)	
public void ProjectSegmentsToSurface()		  
```

<hr class="custom-line1">

## `Spline_CachedData.cs`
### `Properties`
```csharp
// Indicates whether the spline has an invalid shape. 
// A spline is considered invalid if it has only one control point,
// or if the first anchor and its Tangent B share the same position,
// or if the last anchor and its Tangent A share the same position.
// An invalid spline cannot be deformed, as this would otherwise 
// cause visual artifacts when deforming meshes along the spline.
public bool isInvalidShape { get; private set; }
public Bounds controlPointsBounds { get; private set; }
public NativeList<float> DistanceMap { get; private set; }
public NativeList<int> NormalsMapLocal { get; private set; }
public NativeList<NativeNormalFrame> NormalsLocal { get; private set; }
public NativeList<SegmentSample> SegmentSamples { get; private set; }
public NativeList<float3> PositionMapLocal { get; private set; }

// Cached world positions along the spline. Only calculated in the editor. 
// In builds, it is calculated when the terrain is deformed using DeformTerrain() (Terrain Tools addon).
public NativeList<float3> PositionMap { get; private set; }

public NativeArray<NativeSegment> NativeSegmentsLocal { get; private set; }
public NativeArray<NoiseLayer> NativeNoises { get; private set; }
public NativeList<Vector3> LinePoints { get; private set; }

```

### `Functions`
```csharp
// Rebuilds and updates all cached and native data 
// used by the spline for deformations, bounds,
// and runtime evaluation. Call this if you need
// the spline to be updated with new data
// before performing custom logic that depends on the latest spline state.
public void RebuildCache()

// Marks the spline as dirty, causing the cache to be rebuilt 
// and all spline objects to update on the next job cycle.
// In most cases this is not needed, as the system automatically
// detects changes to the spline and its spline objects.
public void MarkCacheDirty()
public float GetSplineResolution(bool rawValue = false)
public float GetNormalResolution()
public float GetSamplingStepDistanceMap(bool rawValue = false)
public void SetSplineResolution(float value)
public void SetResolutionNormal(float value)
public void SetSamplingStepDistanceMap(float value)
```

<hr class="custom-line1">

## `Spline_Deformation.cs`
### `Values`
```csharp
public JobType jobStartType;
public JobType jobEndType;

// The number of frames between job start and job completion.
public int jobInterval;

// The number of frames to wait before the first 
// job starts after the spline is initialized.
// This can be used to offset multiple splines so 
// their jobs do not complete on the same frame.
public int initialJobDelay;
```

### `Properties`
```csharp
// The current job state of the spline's internal deformation system.
public JobState jobState { get; internal set; }

// JobUpdateMode determines how the spline decides whether 
// SplineObjects should be included in the update job.
// When set to Always Update, the normal dirty check logic is bypassed 
// and spline objects are processed every update cycle.
// This can improve performance when many spline objects are known to update 
// together every frame. In such cases, performance can improve by around 15%.
public JobUpdateMode jobUpdateMode { get; set; }

// Spreads SplineObjects across multiple frames based on the value.
// Does not work with the Population system.
public int FrameSpreading { get; set; }
```

### `Functions`
```csharp
// Gets the total number of vertices currently 
// processed by the splines internal system workers.
public int GetVerticesCountInWorkers()

// Immediately updates the spline object's position 
// and deforms it if it is a deformation object.
public void DeformNow(SplineObject so, 
							  bool updateExternalComponents = false)
		

// Rebuilds all cached data on the spline and updates all SplineObjects attached to it.
// If you are using jobInterval > 0 and have added Populations to the spline,
// you can call this function to force the spline to update all populations immediately.
public void DeformSplineObjectsNow()
```

<hr class="custom-line1">

## `Spline_Instancing.cs`
### `Properties`
```csharp
public ShadowCastingMode InstancingCastShadows { get; set; } = ShadowCastingMode.On
public bool InstancingReceiveShadows { get; set; } = true;
public int InstanceCount { get; private set; }
public int NoneRenderedInstanceCount { get; private set; }
```

### `Functions`
```csharp
// Gets a dictionary of all instance batches.
// Do not add or remove any instance batches,
// as doing so may break the internal system.
// The key is generated from the instance IDs of the materials and the mesh.
// All instances that are set to not render will be placed in the batch with key 0.
public Dictionary<ulong, InstanceBatch> GetInstanceBatchesUnsafe()
```

## `Spline_Event.cs`
### `Events`
```csharp
public event Action afterRebuildCache;
public event Action beforeRebuildCache;
public event Action afterJobs;
public event Action beforeJobs;
public event Action beforeSplineDestroy;
```
<hr class="custom-line1">

## `Spline_Population.cs`
### `Functions`
```csharp
public void AddPopulation(Population population)
public void RemovePopulation(Population population, PopulationRemoveOption populationRemoveOption)
public bool ContainsPopulation(Population population)
public void ClearPopulations(PopulationRemoveOption populationRemoveOption)

// Bakes (instantiates) the given Population into SplineObjects. 
// Baked objects are not managed by the Population system
// and will not update when the spline or Population settings change.
public void BakePopulation(Population population, 
						   ICollection<SplineObject> resultSplineObjects = null)
```
<hr class="custom-line1">

## `Spline_TerrainTools.cs`
Requires the Terrain Tools addon.

### `Values`
```csharp
// Terrain deformation settings, the same settings as in the Deform terrain menu.
// They are used by DeformTerrain().
public TerrainMode terrainMode;
public List<Vector2> deformationRanges;
public float deformationWidth;
public float deformationBlendWidth;
public float deformationYOffset;
public Easing terrainBlendWidthEasing;
public float startTrim;
public float endTrim;
public bool enableStartTrim;
public bool enableEndTrim;
public float resolutionTerrain;
public NoiseGroup noiseGroupTerrain;
public float noiseErosion;
public Space noiseSpace;
```

### `Functions`
```csharp
// Deforms all terrains under the spline using the spline's terrain settings, the same way as
// the terrain deformation in the editor. The list is cleared and then filled with one TerrainDelta
// per deformed terrain. Pass them to RevertTerrain to revert the deformation.
// In the editor, deformations made during play mode that have not been reverted are reverted when exiting play mode.
public void DeformTerrain(List<TerrainDelta> terrainDeltas)

// Reverts a terrain deformation made by DeformTerrain. The deltas are added to the terrain's current heights,
// so if several deformations overlap, revert them in the reverse order they were made in.
public void RevertTerrain(TerrainDelta terrainDelta)
```
<hr class="custom-line1">