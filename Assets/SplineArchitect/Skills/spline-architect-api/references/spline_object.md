# `SplineObject.cs`
Namespace: `SplineArchitect`  

### `Overview`
SplineObjects can move along splines as either deformations or followers. Followers are regular GameObjects, 
while deformations create an instance mesh from a `MeshFilter` or `MeshCollider` on the same GameObject and deform it as it moves, scales, or rotates along the spline.

A `SplineObject` behaves similarly to a Transform, but in spline space. Changing `splineObject.localSplinePosition` will automatically update and deform it to the new position.  
`localSplinePosition` is a `Vector3` where movement along the Z axis moves the object along the spline, and the other axes offset it from the spline center.

SplineObjects can also have other SplineObjects as their children. You can change the parent by setting  
`splineObject.transform.parent = splineObjectParent.transform`. This can also be used to move a SplineObject from one spline to another.

You can add a `SplineObject` component to any GameObject, or simply attach a GameObject to a spline and a `SplineObject` will be created automatically.

One `SplineObject` can contain one or more MeshContainers.
MeshContainers are created for each MeshFilter and MeshCollider component located on the same GameObject as the `SplineObject` component.

### `Class info`
The `SplineObject` class is a partial class split across three files: `SplineObject.cs`, `SplineObject_Lifecycle.cs`, and `SplineObject_Editor.cs`. 
`SplineObject.cs` is the only one of these intended for direct user use.

### `Values`
```csharp
public Vector3 localSplinePosition;
public Quaternion localSplineRotation;
```

### `Properties`
```csharp
public int MeshContainerCount {get; private set;}
public Vector3 splinePosition {get; set;}
public Quaternion splineRotation {get; set;}
public Spline SplineParent {get; private set;}
public SplineObject SoParent {get; private set;}
public bool AlignToEnd {get; set;}
public bool SkipTangents {get; set;}
public bool ProjectToSurface {get; set;}
public bool MirrorDeformation {get; set;}
public NormalType NormalType {get; set;}
public SplineObjectType Type {get; set;}
public SnapSettings SnapSettings {get; set;}

// Only used by followers. A value from 0 to 1, where 0 follows the spline's position as usual
// and 1 locks the position so the follower only adjusts to the spline's rotation.
public float LockPosition {get; set;}
public RotationMode RotationMode {get; set;}
public LayerMask ProjectToSurfaceMask {get; set;}
```

### `Functions`
```csharp
// Enables or disables rendering for the mesh associated with this SplineObject.
// This affects the MeshRenderer on the same GameObject as well as all Renderer
// components found on its children.
public void RenderMesh(bool value)

// Returns whether rendering is enabled for this SplineObject, set by RenderMesh().
public bool IsMeshRendered()

// Returns true if this spline object is a parent, grandparent, 
// or any higher-level ancestor of the given spline object.
public bool IsAncestorOf(SplineObject so)
public MeshContainer GetMeshContainerAtIndex(int index)

// Scans the GameObject for MeshFilter and MeshCollider components 
// and creates MeshContainer entries for any that are not yet registered.
// Call SyncInstanceMeshesFromCache() after this to create 
// instance meshes for each MeshContainer created by this function.
public void SyncMeshContainers()

// Updates the instance meshes for all MeshContainers.
// Call this after changing the mesh for a MeshContainer
// using meshContainer.SetOriginMesh(), or after running
// SyncMeshContainers() on this SplineObject.
// This will fetch or create a new cached instance mesh
// and ensure that it is properly deformed by the spline.
public void SyncInstanceMeshesFromCache()
```
<hr class="custom-line1">