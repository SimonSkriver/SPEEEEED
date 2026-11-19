# API Enums

Lists all public enums used by the Spline Architect runtime API.

These enums are referenced by various classes, properties, and functions throughout the API documentation
(e.g. `SplineType`, `ComponentMode`, `JobType`, `LinkFlags`, `SplineObjectType`, `NormalType`, etc.).

Use this page to:
- Understand the meaning of enum values used in function parameters and properties.
- See all available options for configuration and behavior.
- Look up flags and modes referenced in code examples and method signatures.

## ComponentMode
```csharp
public enum ComponentMode
{
	REMOVE_FROM_BUILD,
	INACTIVE,
	ACTIVE,
	NONE,
}
```

## ControlHandle
```csharp
public enum ControlHandle : byte
{
	NONE,
	ANCHOR,
	TANGENT_A,
	TANGENT_B
}
```

## ControlHandleType
```csharp
public enum ControlHandleType
{
	MIRRORED,
	CONTINUOUS,
	BROKEN,
	AUTO
}
```

## Easing
```csharp
public enum Easing
{
	LINJER,
	EASE_IN_SINE,
	EASE_OUT_SINE,
	EASE_IN_OUT_SINE,
	EASE_IN_QUBIC,
	EASE_OUT_QUBIC,
	EASE_IN_OUT_CUBIC,
	EASE_IN_QUINT,
	EASE_IN_OUT_QUINT,
	EASE_OUT_CIRC,
	EASE_OUT_QUINT,
}
```

## InterpolationType
```csharp
public enum InterpolationType : byte
{
	SPLINE,
	LINE
}
```

## JobState
```csharp
public enum JobState
{
	IDLE,
	RUNTIME,
	EDITOR
}
```

## JobType
```csharp
public enum JobType
{
	UPDATE,
	LATE_UPDATE,
	DO_NOTHING
}
```

## JobUpdateMode
```csharp
public enum JobUpdateMode
{
	CHECK_FOR_CHANGES,
	ALWAYS_UPDATE
}
```

## JoinType
```csharp
public enum JoinType
{
	START_TO_START,
	START_TO_END,
	END_TO_START,
	END_TO_END
}
```

## LinkFlags
```csharp
public enum LinkFlags
{
	NONE = 0,
	SKIP_LAST = 1 << 0,
	SKIP_FIRST = 1 << 1,
	SKIP_SELF = 1 << 2
}
```

## LinkTarget
```csharp
public enum LinkTarget : byte
{
	NONE,
	ANCHOR,
	SPLINE_CONNECTOR
}
```

## MeshMode
```csharp
public enum MeshMode
{
	SAVE_IN_BUILD,
	SAVE_IN_SCENE,
	GENERATE,
	DO_NOTHING,
	CREATED_AT_RUNTIME
}
```

## NoiseGroup
```csharp
public enum NoiseGroup
{
	NONE,
	A,
	B,
	C,
	D,
	E,
	F,
	G,
	H,
	I,
	J,
	K,
	L,
	M,
	N,
	O,
	P,
}
```

## NoiseType
```csharp
public enum NoiseType
{
	PERLIN_NOISE,
	BILLOW_NOISE,
	DOMAIN_WARPED_NOISE,
	VORONOI_NOISE,
	TERRACE_NOISE,
	RIDGED_PERLIN_NOISE,
	FMB_NOISE,
	HYBRID_MULTI_FRACTAL,
}
```

## NormalType
```csharp
public enum NormalType : byte
{
	SPLINE_SPACE,
	CYLINDER_BASED,
	UNITY_CALCULATED,
	UNITY_CALCULATED_SEAMLESS,
	DO_NOT_CALCULATE
}
```

## ResolutionMode
```csharp
public enum ResolutionMode : byte
{
	SUBDIVIDE_ALL_AXELS,
	SUBDIVIDE_Z_AXEL,
	SUBDIVIDE_X_AXEL,
	EDGE_LOOPS_ALONG_Z,
	EDGE_LOOPS_ALONG_X
}
```

## RotationMode
```csharp
public enum RotationMode
{
	FOLLOW_SPLINE,
	LOCK_UPWARDS,
	LOCK_UPWARDS_KEEP_Z_ROTATION,
	FOLLOW_PROJECTION,
}
```

## SnapMode
```csharp
public enum SnapMode : byte
{
	NONE,
	CONTROL_POINTS,
	SPLINE_POINT,
	CONTROL_POINT_END,
}
```

## SplineHideMode
```csharp
public enum SplineHideMode
{
	NONE,
	SELECTED,
	SELECTED_OCCLUDED
}
```

## SplineObjectType
```csharp
public enum SplineObjectType : byte
{
	DEFORMATION,
	FOLLOWER,
	NONE,
	NOT_SET,
}
```

## SplineType
```csharp
public enum SplineType : byte
{
	STATIC_3D,
	STATIC_2D,
	DYNAMIC,
	NOT_USED
}
```

## TerrainMode
Requires the Terrain Tools addon.
```csharp
public enum TerrainMode
{
	SPLINE,
	FILL
}
```

## PopulationSpawnSource
```csharp
public enum PopulationSpawnSource : byte
{
	CREATED,
	POOLED
}
```

## PopulationFillMode
```csharp
public enum PopulationFillMode : byte
{
	NONE,
	SNAP_LAST,
	FIT_USING_SCALE,
	FIT_USING_SPACING
}
```

## WorkerState
```csharp
public enum WorkerState
{
	EMPTY,
	WORKING,
	NOT_EMPTY,
	FULL
}
```

## PopulationRemoveOption
```csharp
public enum PopulationRemoveOption
{
	DISABLE_AND_POOL,
	DESTROY,
	DO_NOTHING
}
```

## SplineSearchResultFlags
```csharp
public enum SplineSearchResultFlags
{
	NONE = 0,
	SEARCH_FORWARD = 1 << 0,
	SEARCH_BACKWARD = 1 << 1,
	SEARCH_CLOSEST_LINK_FORWARD = 1 << 2,
	SEARCH_CLOSEST_LINK_BACKWARD = 1 << 3,
	NEED_SAME_X_POSITION = 1 << 4,
	NEED_SAME_Y_POSITION = 1 << 5,
}
```

# API Structs

Lists all public structs used by the Spline Architect runtime API.

These structs represent data containers and result types returned by functions
or used as parameters (e.g. `SplineClosestResult`, `AutoSmoothData`, `PointWorkerData`, etc.).

Use this page to understand the fields and meaning of the data structures
referenced throughout the API documentation.

## AutoSmoothData
```csharp
public struct AutoSmoothData
{
	public int index;
	public Vector3 anchor;
	public Vector3 tangentA;
	public Vector3 tangentB;

	public Vector3 prevTangentA;
	public Vector3 nextTangentB;
}
```

## HermiteSegment
```csharp
public struct HermiteSegment
{
	public float timeStart;
	public float timeEnd;
	public float valueStart;
	public float valueEnd;
	public float tangentStart;
	public float tangentEnd;
}
```

## MapsDataDeformation
```csharp
public struct MapsDataDeformation
{
	public bool mirrorDeformation;
	public bool alignToEnd;
	public bool skipTangents;
	public bool createRayCastCommand;
	public SnapData snapData;
	public LayerMask layerMask;
	public NormalType normalType;
	public float4x4 localSpace;
}
```

## NativeNormalFrame
```csharp
public struct NativeNormalFrame
{
	public float3 x;
	public float3 y;
	public float3 z;
}
```

## NativeSegment
```csharp
public struct NativeSegment
{
	public float3 anchor;
	public float3 tangentA;
	public float3 tangentB;
	public quaternion rotation;
	public bool isStatic;
	public float length;
	public float zPosition;
	public float zRot;
	public float contrast;
	public float noise;
	public float2 saddleSkew;
	public float2 scale;
}
```

## NoiseLayer
```csharp
public partial struct NoiseLayer
{
	public NoiseType type;
	public Vector3 scale;
	public Vector2 position;
	public int octaves;
	public float frequency;
	public float amplitude;
	public NoiseGroup group;
	public bool enabled;

	public NoiseLayer(NoiseType type, Vector3 scale, Vector2 position, 
	                  int octaves = 4, float frequency = 2, float amplitude = 2)
}
```

## NormalFrame
```csharp
public struct NormalFrame
{
	public Vector3 x;
	public Vector3 y;
	public Vector3 z;
}
```

## PointWorkerData
```csharp
public struct PointWorkerData
{
	public Vector3 point;
	public Vector3 forwardDirection;
	public Vector3 upDirection;
	public Vector3 rightDirection;
}
```

## SegmentSample
```csharp
public struct SegmentSample
{
	public float2 scale;
	public quaternion zRotation;
	public float2 saddleSkew;
	public float noise;
}
```

## SnapData
```csharp
public struct SnapData
{
	public bool start;
	public bool end;
	public float snapStartPoint;
	public float snapEndPoint;
	public float soStartPoint;
	public float soEndPoint;
}
```

## SnapSettings
```csharp
public struct SnapSettings
{
	public float startSnapDistance;
	public float endSnapDistance;
	public float startSnapOffset;
	public float endSnapOffset;
	public float snapTargetPoint;
	public SnapMode snapMode;
}
```

## SplineClosestResult
```csharp
public struct SplineClosestResult
{
	public Vector3 pointA;
	public Vector3 pointB;
	public double timeA;
	public double timeB;
}
```

## SplineSearchResult
```csharp
public struct SplineSearchResult
{
	public float distanceFromLink;
	public float distanceFromTargetToLink;
	public float distanceFromSplinePoint;
	public SplineObject target;
	public bool isOnLinkedSpline;
	public bool isForwardAlongSpline;
}
```

## TerrainChunk
Requires the Terrain Tools addon. An area of a terrain's heightmap, measured in heightmap samples.
```csharp
public struct TerrainChunk
{
	public enum Type
	{
		DEFORM,
		PAINT
	}

	public Type type;
	public int xBase;
	public int yBase;
	public int width;
	public int height;

	public TerrainChunk(Type type, int xBase, int yBase, int width, int height)
}
```

## TerrainDelta
Requires the Terrain Tools addon. The deformation of one terrain, created by `spline.DeformTerrain()`. 
Pass it to `spline.RevertTerrain()` to revert the deformation.
```csharp
public struct TerrainDelta
{
	// Height before the deformation minus the height after, 
	// one value per heightmap sample in the chunk (y * chunk.width + x).
	public float[] deltas;

	// The deformed area of the terrain's heightmap.
	public TerrainChunk chunk;

	// The deformed terrain.
	public Terrain terrain;
}
```

## UvSettings
```csharp
public struct UvSettings
{
	public Vector2 uvOffset;
	public Vector2 uv2Offset;
	public Vector2 uv3Offset;
	public Vector2 uv4Offset;
}
```