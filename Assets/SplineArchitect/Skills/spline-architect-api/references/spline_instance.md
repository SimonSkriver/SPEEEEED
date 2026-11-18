# `SplineInstance.cs`
Namespace: `SplineArchitect`  

### `Overview`
A `SplineInstance` is very similar to a `SplineObject`, but instead of using Unity's `GameObject` system, it uses GPU instancing, enabling you to move 10,000 to even 1,000,000 objects (on powerful computers) along splines.

A `SplineInstance` has a `SplinePosition` and `SplineRotation`, just like a `SplineObject`, and updates automatically whenever either value changes.
However, because of how GPU instancing works, you can't deform meshes with a `SplineInstance`, so you can think of it as an extremely high-performance follower.

`SplineInstance`s cannot be created through the Spline Architect UI like regular `SplineObject`s and are an API-only feature.

When you create a `SplineInstance` using `Spline.CreateSplineInstance()`, or create one manually and assign it to a parent spline, 
a `NativeSplineInstance` struct within the spline's instance system is automatically created and linked to that `SplineInstance`.
This struct can be used directly inside your own Unity Burst jobs, making it extremely performant, and it is automatically updated whenever its values change.

Many settings are available directly on the `NativeSplineInstance` for use inside Burst jobs, but if you want to cross over to other splines, 
change its mesh, or change its material, you need to use the `SplineInstance` class.

### `Class info`
Consists of one file, SplineInstance.cs.

### `Properties`
```csharp
public Spline SplineParent { get; private set; }
public NativeSplineInstance NativeSplineInstance { get; private set; }
public float3 SplinePosition { get; set; }
public float3 Scale { get; set; }
public quaternion SplineRotation { get; set; }
public RotationMode RotationMode { get; set; }
public bool AlignToEnd { get; set; }
public int MaterialCount { get; private set; }
public Mesh Mesh { get; set; }
```

### `Functions`
```csharp
public Material GetMaterialAtIndex(int index)
public void SetMaterialAtIndex(int index, Material material)
public void SetMeshAndMaterials(Mesh mesh, List<Material> materials)
public void SetMeshAndMaterial(Mesh mesh, Material material)
public bool TryFindLinkCrossings(List<Segment> links, Vector3 splinePosition,
                                                      Vector3 previousSplinePosition,
                                                      out float zDif)
public void RenderMesh(bool value)
public bool IsMeshRendered()
public void SetParent(Spline newParent)
```
<hr class="custom-line1">