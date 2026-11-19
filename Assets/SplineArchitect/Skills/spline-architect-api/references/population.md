# `Population.cs`
Namespace: `SplineArchitect`  

### `Overview`
Populations can be added to any `Spline`. A `Population` automatically fills the spline with instances of a specific prefab. 
Each `Population` manages only one prefab at a time.

You can configure various settings such as `spacing`, `startPadding`, `endPadding`, `scale`, `fillMode`, `maxInstances`, and more.

A `Population` automatically updates when the spline changes or when any settings in the `Population` are modified.  
You can use multiple `Population`s on the same `Spline`.

You can also bake a `Population` by calling: `spline.BakePopulation(yourPopulation)` 
After a `Population` is baked, it will no longer respond to spline changes.

### `Class info`
Consists of one file, Population.cs.

### `Constructor`
```csharp
public Population(GameObject prefab, bool deform, bool updateOverTime = true, 
				  Transform parent = null, bool worldPositionStays = true)
```

### `Events`
```csharp
public event Action<SplineObject, PopulationSpawnSource> afterInstanceSpawned;
public event Action<SplineObject> afterInstanceDespawned;
public event Action afterUpdate;
```

### `Properties`
```csharp
public float StartPadding { get; set; }
public float EndPadding { get; set; }
public float XOffset { get; set; }
public float YOffset { get; set; }
public float Spacing { get; set; }
public int Resolution { get; set; }

// Determines how the resolution adds detail to the meshes. Default is EDGE_LOOPS_ALONG_Z.
public ResolutionMode ResolutionMode { get; set; }

// Sets the resolution automatically based on how much the spline bends and twists.
// The Resolution value is not used when this is enabled.
public bool AutoResolution { get; set; }

// Only used when AutoResolution is enabled. Higher values increase the resolution, 
// 0 adds no extra resolution. Default is 1.
public float ResolutionBias { get; set; }

// Sets MinAutoResolution and MaxAutoResolution on all mesh containers in the population.
// Only used when AutoResolution is enabled. Limits the resolution auto resolution can set.
// Both are clamped between 0 and 32 and default to 0. A MaxAutoResolution of 0 means no limit.
// MinAutoResolution wins if it's greater than MaxAutoResolution.
public int MinAutoResolution { get; set; }
public int MaxAutoResolution { get; set; }
public bool SkipTangents { get; set; }

// Determines how the population fills the spline.
// Keep the original layout, snap the last deformation to the end of the spline,
// or fit all deformations to the spline length using scale or spacing.
public PopulationFillMode FillMode { get; set; }
public int MaxInstances { get; set; }
public Quaternion PrefabRotationOffset { get; set; }

// Scale applied to all instances on top of the prefab's own scale.
public Vector3 Scale { get; set; }
public GameObject Prefab { get; private set; }
public Bounds PrefabBounds { get; set; }
public bool Deform { get; private set; }
public Transform Parent { get; private set; }
public bool UpdateOverTime { get; private set; }
public bool WorldPositionStays { get; private set; }
```
<hr class="custom-line1">