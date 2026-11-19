# `MeshContainer.cs`
Namespace: `SplineArchitect`

### `Overview`
MeshContainers are stored within SplineObjects. 
Each MeshContainer handles a mesh used by the SplineObject, which can be either a collider mesh or a regular rendered mesh. 
When a SplineObject has multiple MeshContainers, the MeshContainer that handles the MeshFilter is always placed at index 0.

### `Class info`
The MeshContainer is a partial class and consists of two files: MeshContainer.cs and MeshContainer_Editor.cs.
Only MeshContainer.cs is public and can be used outside of the Spline Architect API.

### `Properties`
```csharp
// Adds vertices and triangles to the instance mesh derived from the original mesh.
// A mesh’s highest resolution value is usually around 14, but can vary depending on the mesh structure.
// For the edge loop resolution modes, the resolution is the number of edge loops added.
public int Resolution { get; set; }

// Determines how the resolution adds detail to the mesh.
// The subdivide modes split the triangles on all axes, or only based on the length along the Z or X axis.
// The edge loop modes add evenly spaced edge loops along the Z or X axis.
// The Z axis modes require the mesh’s Z axis to align with the spline direction.
public ResolutionMode ResolutionMode { get; set; }

// Enables auto resolution. The mesh resolution increases based on how much the spline bends and twists under the mesh. 
public bool AutoResolution { get; set; }

// Only used when AutoResolution is enabled. Higher values increase the resolution.
// The value is clamped between 0 and 10, where 0 adds no extra resolution.
public float AutoBias { get; set; }

// Only used when AutoResolution is enabled. Limits the resolution auto resolution can set.
// Both are clamped between 0 and 32 and default to 0. A MaxAutoResolution of 0 means no limit.
// MinAutoResolution wins if it's greater than MaxAutoResolution.
// Not serialized and not shown in the UI. The values reset to 0 when scripts or the scene reloads
// and when the object is instantiated, so set them again after that if they're needed.
public int MinAutoResolution { get; set; }
public int MaxAutoResolution { get; set; }
public int MaterialCount { get; private set; }
```

### `Functions`
```csharp
public void RenderMesh(bool value)
public bool IsMeshRendered()

// Gets the deformed instance meshes used inside the mesh filter or mesh collider.
public Mesh GetInstanceMesh()

// Gets the mesh that the instance mesh originates from.
public Mesh GetOriginMesh()

// Sets the mesh that the instance mesh originates from.
// You can change this at runtime, but remember to run
// SyncInstanceMeshesFromCache on the SplineObject afterwards.
public void SetOriginMesh(Mesh originMesh)

public Component GetMeshComponent()
public bool MeshComponentIsMeshFilter()

// Sets the shared material at the specified material slot 
// index on the related MeshRenderer component.
public void SetMaterialAtIndex(int index, Material material)

// Sets the origin mesh and the shared materials on the related MeshRenderer component.
// Only works for the MeshContainer that handles the MeshFilter. Remember to run
// SyncInstanceMeshesFromCache on the SplineObject afterwards.
public void SetOriginMeshAndMaterials(Mesh originMesh, List<Material> materials)

// Gets the shared materials on the related MeshRenderer component.
public void GetMaterials(List<Material> materials)

public void SetUvOffsetAtIndex(int index, Vector2 value)
public Vector2 GetUvOffsetAtIndex(int index)
public UvSettings GetUvSettings()
```
<hr class="custom-line1">