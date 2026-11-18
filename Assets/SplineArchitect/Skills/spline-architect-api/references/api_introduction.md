# API Introduction 
This section introduces the Spline Architect runtime API, explains its core concepts, and shows how to use it efficiently in code.

Everything in the runtime API is included in the base Spline Architect package, you do not need to purchase any addons to access its features.
The only exception is runtime terrain deformation, which requires the Terrain Tools addon. All other addon features are designed for editor use only and do not extend the runtime API.

## Core concepts
The following sections describe the main building blocks of the Spline Architect API and how they relate to each other.
All runtime behavior is handled by Splines, SplineObjects, and SplineConnectors. No global GameObjects or components are used.

### Spline
A `Spline` consists of two or more segments. All segments are stored in the `Spline`, and you can create or remove them using the `Spline` class.  

Each spline manages its own `SplineObjects`, and disabling the Spline component will disable all moving SplineObjects on that spline.

Because each `Spline` handles its own runtime logic, Spline Architect can easily be excluded from specific scenes, as no global components or GameObjects are used.

### Segment
A `Segment` consists of three points: the main anchor point, tangent A, and tangent B.  
Each `Segment` stores a reference to its parent spline (`segment.SplineParent`) and its index in that spline (`segment.IndexInSpline`).

Segments can be created, moved, rotated, removed, linked, or unlinked at runtime. 
They can be linked to other segments on the same spline or on different splines, and they can also be linked to `SplineConnectors`.

They also have various effects that can be adjusted to control how SplineObjects are deformed along the spline, such as rotation, scale, noise layers, and more.

Moving or changing any of the effects of the `Segment` will automatically update the `Spline` and all attached SplineObjects attached.

### SplineConnector
SplineConnectors are independent of any spline, and any segment can be connected to them. 
Moving or rotating a SplineConnector will update all connected splines.

### SplineObject
SplineObjects can move along splines as either deformations or followers. Followers are regular GameObjects, 
while deformations create an instance mesh from a `MeshFilter` or `MeshCollider` on the same GameObject and deform it as it moves, scales, or rotates along the spline.

A `SplineObject` behaves similarly to a Transform, but in spline space. Changing `splineObject.localSplinePosition` will automatically update and deform it to the new position.  
`localSplinePosition` is a `Vector3` where movement along the Z axis moves the object along the spline, and the other axes offset it from the spline center.

SplineObjects can also have other SplineObjects as their children. You can change the parent by setting  
`splineObject.transform.parent = splineObjectParent.transform`. This can also be used to move a SplineObject from one spline to another.

You can add a `SplineObject` component to any GameObject, or simply attach a GameObject to a spline and a `SplineObject` will be created automatically.

One `SplineObject` can contain one or more MeshContainers.
MeshContainers are created for each MeshFilter and MeshCollider component located on the same GameObject as the `SplineObject` component.

### MeshContainer
MeshContainers are stored within SplineObjects.
Each `MeshContainer` handles a mesh used by the `SplineObject`, which can be either a collider mesh or a regular rendered mesh.

### HandleRegistry
`HandleRegistry` is a static class where all Splines and SplineConnectors register themselves.  
This allows you to access all splines and spline connectors in the currently open scene(s).

Destroyed Splines and SplineConnectors are unregistered, but disabled ones remain registered.

### SplineObjectWorker
Each spline creates its own `SplineObjectWorker` internally, which handles all deformation jobs and follower updates.  

You can disable the internal deformation system for a spline, create your own `SplineObjectWorker`, and fully control the deformation process yourself.

### Population
Populations can be added to any `Spline`. A `Population` automatically fills the spline with instances of a specific prefab. 
Each `Population` manages only one prefab at a time.

You can configure various settings such as `spacing`, `startPadding`, `endPadding`, `scale`, `fillMode`, `maxInstances`, and more.

A `Population` automatically updates when the spline changes or when any settings in the `Population` are modified.  
You can use multiple `Population`s on the same `Spline`.

You can also bake a `Population` by calling: `spline.BakePopulation(yourPopulation)` 
After a `Population` is baked, it will no longer respond to spline changes.

### SplineInstance
A `SplineInstance` is very similar to a `SplineObject`, but instead of using Unity's `GameObject` system, it uses GPU instancing, enabling you to move 10,000 to even 1,000,000 objects (on powerful computers) along splines.

A `SplineInstance` has a `SplinePosition` and `SplineRotation`, just like a `SplineObject`, and updates automatically whenever either value changes.
However, because of how GPU instancing works, you can't deform meshes with a `SplineInstance`, so you can think of it as an extremely high-performance follower.

`SplineInstance`s cannot be created through the Spline Architect UI like regular `SplineObject`s and are an API-only feature.

When you create a `SplineInstance` using `Spline.CreateSplineInstance()`, or create one manually and assign it to a parent spline, 
a `NativeSplineInstance` struct within the spline's instance system is automatically created and linked to that `SplineInstance`.
This struct can be used directly inside your own Unity Burst jobs, making it extremely performant, and it is automatically updated whenever its values change.

Many settings are available directly on the `NativeSplineInstance` for use inside Burst jobs, but if you want to cross over to other splines, 
change its mesh, or change its material, you need to use the `SplineInstance` class.

## Usage Examples
We will start with simple usage examples and work us down to more advanced coding.

### Move SplineObject
Below is a simple example showing how to move a `SplineObject` along its spline. 
Here we use the Z axis of `splineObject.localSplinePosition` to move it forward. 
The X and Y axes can be used to offset the `SplineObject` from the center of the spline.

!>Remember that the `Spline` and `SplineObject` component settings in the Spline Architect control panel must be enabled, otherwise the `SplineObject` will not move.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>

```csharp
using UnityEngine;
using SplineArchitect;

public class MoveSplineObject : MonoBehaviour
{
	public float speed;
	public SplineObject splineObject;

	void Update()
	{
		splineObject.localSplinePosition.z += Time.deltaTime * speed;
	}
}
```

</details>

### Create SplineObjects
Creating SplineObjects during runtime is very easy and is done by using `spline.CreateDeformation()` or `spline.CreateFollower()`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class CreateSplineObjects : MonoBehaviour
{
    public Spline spline;
    public GameObject prefab;

    void Start()
    {
        //Create a deformation at the middle of the spline
        GameObject deformationGo = Instantiate(prefab);
        Vector3 splinePosition = new Vector3(0, 0, spline.Length / 2);
        spline.CreateDeformation(deformationGo, splinePosition, Quaternion.identity);

        //Create a follower at the end of the spline.
        deformationGo = Instantiate(prefab);
        splinePosition = new Vector3(0, 0, spline.Length);
        spline.CreateFollower(deformationGo, splinePosition, Quaternion.identity);
    }
}
```

</details>

### Create Spline and segments
Below we use a utility function called `SplineUtility.Create()` to create a `Spline`.

To create segments, use `spline.CreateSegment()`.
To remove segments, use `spline.RemoveSegment()`.
To get the last segment, use `spline.GetSegmentAtIndex(spline.SegmentCount - 1)`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using System.Collections.Generic;
using UnityEngine;
using SplineArchitect;
using SplineArchitect.Utility;

public class CreateSpline : MonoBehaviour
{
    public List<Vector3> points;

    void Start()
    {
        //Create spline
        GameObject splineGo = new GameObject("Spline (1)");
        Spline spline = SplineUtility.Create(splineGo, points, Color.blue);

        //Create segment on spline
        Vector3 anchor = new Vector3(20, 0, 0);
        float tangentDistance = 5;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        spline.CreateSegment(anchor, tangentDistance, tangentDistance, rotation);
    }
}
```

</details>

### Get the direction
Getting the direction directly from the spline or segment is easy. Either use `spline.GetDirection(float time)` or `segment.GetDirection()`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class GetDirection : MonoBehaviour
{
    public Spline spline;

    void Start()
    {
		// Get the direction at time 0.5f on the spline.
		Debug.Log(spline.GetDirection(0.5f));
		
		if(spline.SegmentCount > 0)
		{
			// Get the direction on the first segment.
			Debug.Log(spline.GetSegmentAtIndex(0).GetDirection());
		}
    }
}
```

</details>

### Move segments
Moving the segments of a `Spline` is straightforward. First, you get the `Segment` you want to move by calling `spline.GetSegmentAtIndex(index)`.
Use `spline.SegmentCount` to determine how many segments the `Spline` has.

Each `Segment` has three control points you can move: the Anchor, Tangent A, and Tangent B.
If you use `segment.SetAnchorPosition(newPosition)`, both the anchor and the tangents will move. If you use `segment.SetPosition(ControlHandle.ANCHOR, newPosition)`, only the anchor will move.
The `spline` will automatically detect any changes made to its segments and update all cached values and attached SplineObjects.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class CreateSpline : MonoBehaviour
{
    public Spline spline;

    void Start()
    {
        // Get the last segment
        Segment lastSegment = spline.GetSegmentAtIndex(spline.SegmentCount - 1);

        // Here we move the anchor and the tangents.
        Vector3 newPosition = new Vector3(0, 5, 0);
        lastSegment.SetAnchorPosition(newPosition);

        // Here we move only the anchor without affecting the tangents.
        newPosition = new Vector3(0, 10, 0);
        lastSegment.SetPosition(ControlHandle.ANCHOR, newPosition);

        // Here we move tangent B and make tangent A mirror tangent B.
        newPosition = new Vector3(0, 15, 0);
        lastSegment.SetMirroredPosition(ControlHandle.TANGENT_B, newPosition);
    }
}
```

</details>

### Find nearest point on spline
You can also find the nearest point on a `Spline` from a world position.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class FindNearestPoint : MonoBehaviour
{
    public Spline spline;
    public Transform point;

    void Start()
    {
        Vector3 nearestPoint = spline.GetNearestPoint(point.position);
    }
}
```

</details>

### Change mesh for MeshContainer
Change and deform the mesh for a MeshContainer.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;

using SplineArchitect;

public class SwitchMesh : MonoBehaviour
{
    public Mesh newMesh;
    public float delay = 2;

    private SplineObject splineObject;
    private MeshContainer meshContainer;

    private float timer;

    void Start()
    {
        splineObject = GetComponent<SplineObject>();

        // The MeshContainer at index 0 will always
        // handle a MeshFilter component if it exists.
        meshContainer = splineObject.GetMeshContainerAtIndex(0);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > delay)
        {
            // Set the new origin mesh
            meshContainer.SetOriginMesh(newMesh);

			// Sync all MeshContainers. If any change is detected, it will
			// also mark the SplineObject dirty and add it to the next deformation job.
            splineObject.SyncInstanceMeshesFromCache();
        }
    }
}
```

</details>

### Detach and attach SplineObjects
After creating a `SplineObject`, you can easily detach it, move it between splines, and reattach it by changing its parent transform.
This is done by setting `splineObject.transform.parent`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class DetachAndAttach : MonoBehaviour
{
    public SplineObject splineObject;
	public Spline spline;

    void Start()
    {
        // Detach the SplineObject. You can now destroy it, pool it, or reattach it.
		splineObject.transform.parent = null;
		
		// Attach or change to another spline
		splineObject.transform.parent = spline.transform;
    }
}
```

</details>

### Populate a spline with prefabs
You can easily populate a `Spline` with prefabs and have it update dynamically as the spline shape changes.  
Create a `Population` by calling: `Population population = new Population(prefab, deform);`  
Then add it to the spline: `spline.AddPopulation(population);`

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using UnityEngine;
using SplineArchitect;

public class PopulateSpline : MonoBehaviour
{
    public Spline spline;
    public GameObject prefab1;
    public GameObject prefab2;

    void Start()
    {
        //Populate from start to end with prefab1.
		Population population = new Population(prefab1, true);
		spline.AddPopulation(population);

        //Populate from 5 meters in and start 5 meters from the end and have 10 meter gaps between each prefab2.
		population = new Population(prefab2, true);
		population.StartPadding = 5;
		population.EndPadding = 5;
		population.Spacing = 10;
		spline.AddPopulation(population);
		
		// You can also easily remove any population from the spline.
		// After it is removed, all SplineObjects in the population will become disabled and invisible.
		// You can choose to either keep them in the spline's pool system for later reuse,
		// destroy them, or do nothing.
		spline.RemovePopulation(population, PopulationRemoveOption.DISABLE_AND_POOL);
    }
}

```

</details>

### Deform terrain during runtime
Requires the Terrain Tools addon. Deform all terrains under a spline with `spline.DeformTerrain()`, using the same terrain settings as in the Deform terrain menu.
The list you pass in is filled with one `TerrainDelta` for each deformed terrain. Pass them to `spline.RevertTerrain()` to revert the deformation.

The Layer setting is not used during runtime, each call deforms the terrain as it currently is. If several deformations overlap, revert them in the reverse order they were made in.

In the editor, changes made to a terrain during Play mode are saved to the terrain's TerrainData asset. 
Because of this, all deformations made during Play mode that have not been reverted are reverted automatically when you exit Play mode.

!>Remember that the `Spline` component setting in the Spline Architect control panel must be set to Active, otherwise the spline data needed for deforming the terrain will not exist in your built game.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using System.Collections.Generic;

using UnityEngine;
using SplineArchitect;

public class DeformTerrainRuntime : MonoBehaviour
{
    public Spline spline;
    public float revertAfter = 5;

    private List<TerrainDelta> terrainDeltas = new List<TerrainDelta>();
    private float timer;

    void Start()
    {
        // Deforms all terrains under the spline and fills the list
        // with one TerrainDelta for each deformed terrain.
        spline.DeformTerrain(terrainDeltas);
    }

    void Update()
    {
        if (terrainDeltas.Count == 0)
            return;

        timer += Time.deltaTime;

        if (timer > revertAfter)
        {
            // Reverts the deformation on each terrain.
            foreach (TerrainDelta terrainDelta in terrainDeltas)
                spline.RevertTerrain(terrainDelta);

            terrainDeltas.Clear();
        }
    }
}

```

</details>

### Move SplineObjects between linked splines.
Moving SplineObjects between linked splines is straightforward. Pass the previous and new `Spline` positions to `spline.FindLinkCrossingsNonAlloc()`.
If the function returns any segments, it means the movement crossed one or more linked segments. 
You can then use these segments to transfer the `SplineObject` to the connected `Spline`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using System.Collections.Generic;
using UnityEngine;
using SplineArchitect;

public class CrossBetweenLinks : MonoBehaviour
{
    public SplineObject so;
    public float speed;

    private List<Segment> links = new List<Segment>();

    void Update()
    {
        Spline spline = so.SplineParent;

        //Get previous position before updating to the new position
        Vector3 previusSplinePosition = so.localSplinePosition;

        //Update spline position
        so.localSplinePosition.z += Time.deltaTime * speed;

        links.Clear();
		//Try find links
        spline.FindLinkCrossingsNonAlloc(links, so.localSplinePosition, 
		                                        previusSplinePosition, 
												LinkFlags.NONE, 
												out Segment currentSegment);

        if (links.Count == 0)
            return;

        Segment fromSegment = currentSegment;
        //Get a random segment
        Segment toSegment = links[Random.Range(0, links.Count)];

        //Swtich to new parent
        so.transform.parent = toSegment.SplineParent.transform;

        //Set new position on the new spline
        so.localSplinePosition.z = spline.CalculateLinkCrossingZPosition(so.localSplinePosition, 
		                                                                 fromSegment, 
																		 toSegment);
    }
}

```

</details>

### Move SplineInstances along a spline
Below we move `SplineInstance`s that are rendered directly using Unity's GPU instancing system. 
You can change values directly on the `SplineInstance`, and it will update correctly, but that is not the most efficient way to do it.

When a `SplineInstance` is created and assigned to a `Spline`, a `NativeSplineInstance` is automatically created within the spline's instance system and linked to the `SplineInstance`. 
Instead of iterating over `SplineInstance`s, we will iterate directly over `NativeSplineInstance`s inside our own Burst job for optimal performance.

We get all `NativeSplineInstance`s by calling `spline.GetInstanceBatchesUnsafe()`. 
This returns `InstanceBatch`es that each contain all `NativeSplineInstance`s using the same mesh and materials.

We can also set the bounds for each `InstanceBatch`. Doing so skips Unity's internal bounds calculation, resulting in performance improvements.
Another good thing to know is that all non-rendered `NativeSplineInstance`s are located in the `InstanceBatch` with ID `0`.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using System.Collections.Generic;

using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = UnityEngine.Random;

using SplineArchitect;

public class SplineInstancing : MonoBehaviour
{
    [BurstCompile]
    private struct SetPositionJob : IJobParallelFor
    {
        public NativeArray<NativeSplineInstance> nativeSplineInstances;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float speed;
        [ReadOnly] public float splineLength;

        public void Execute(int i)
        {
            NativeSplineInstance nsi = nativeSplineInstances[i];
            float3 prevPos = nsi.SplinePosition;

            nsi.SplinePosition = new float3(nsi.SplinePosition.x, nsi.SplinePosition.y, nsi.SplinePosition.z + deltaTime * speed);

            if (nsi.SplinePosition.z > splineLength)
                nsi.SplinePosition = new float3(nsi.SplinePosition.x, nsi.SplinePosition.y, 0);

            nativeSplineInstances[i] = nsi;
        }
    }

    public Mesh mesh;
    public Material material;
    public int amount = 100;
    public float speed = 8;
    public float maxScale;
    public float minScale;
    public float xRange = 5;
    public float yRange = 5f;

    private List<SplineInstance> splineInstances = new List<SplineInstance>();
    private Spline startSpline;

    private void Start()
    {
        startSpline = GetComponent<Spline>();
        startSpline.InstancingReceiveShadows = false;
        startSpline.InstancingCastShadows = UnityEngine.Rendering.ShadowCastingMode.Off;
        startSpline.instancingOptimizationFlags = SplineOptimizationFlags.DISABLE_ALL;

        for (int i = 0; i < amount; i++)
        {
            //Create instance
            SplineInstance si = new SplineInstance(mesh, material);

            //Set position
            float x = Random.Range(-xRange, xRange);
            float y = Random.Range(-yRange, yRange);
            float z = Random.Range(0f, startSpline.Length);
            si.SplinePosition = new float3(x, y, z);

            //Add to spline
            si.SetParent(startSpline);

            //Add to list
            splineInstances.Add(si);
        }
    }

    private void Update()
    {
        foreach (Spline spline in HandleRegistry.GetSplinesUnsafe())
        {
            foreach (KeyValuePair<ulong, InstanceBatch> item in spline.GetInstanceBatchesUnsafe())
            {
                InstanceBatch batch = item.Value;
                batch.Bounds = spline.controlPointsBounds;

                // Set positions
                SetPositionJob setPositionJob = new SetPositionJob()
                {
                    nativeSplineInstances = batch.NativeSplineInstances.AsArray(),
                    speed = speed,
                    deltaTime = Time.deltaTime,
                    splineLength = spline.Length,
                };

                JobHandle jobHandle = setPositionJob.Schedule(batch.Count, 32);
                jobHandle.Complete();
            }
        }
    }
}

```

</details>

### Move SplineInstances, cross to other splines and change material.
In this example, we not only move `SplineInstance`s rendered using GPU instancing, 
but also cross them over to other linked `Spline`s and change their materials when they reach the end.

Crossing to other splines is done by detecting whether a `NativeSplineInstance` should cross over directly inside our Burst job. 
We then add it to a parallel list and handle the `NativeSplineInstance` outside the job.

Outside the job, we can directly access the `SplineInstance` handler linked to the `NativeSplineInstance`. 
Through this handler, we can cross it over to another `Spline`, change its material, and enable or disable rendering.

<details>
<summary><span style="color:#FFB34A;">Show script</span></summary>


```csharp
using System.Collections.Generic;
using System;

using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = UnityEngine.Random;

using SplineArchitect;

public class SAMoveSplineInstances : MonoBehaviour
{
    [Serializable]
    public class Instance
    {
        public Mesh mesh;
        public List<Material> materials;
    }

    public enum EventType
    {
        CROSSING,
        RACHED_END
    }

    public struct InstanceEvent
    {
        public EventType type;
        public NativeSplineInstance instance;
    }

    [BurstCompile]
    private struct SetPositionJob : IJobParallelFor
    {
        public NativeArray<NativeSplineInstance> nativeSplineInstances;
        public NativeList<InstanceEvent>.ParallelWriter events;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float speed;
        [ReadOnly] public float splineLength;

        public void Execute(int i)
        {
            NativeSplineInstance nsi = nativeSplineInstances[i];
            float3 prevPos = nsi.SplinePosition;

            nsi.SplinePosition = new float3(nsi.SplinePosition.x, nsi.SplinePosition.y, nsi.SplinePosition.z + deltaTime * speed);

            if (nsi.SplinePosition.z > splineLength)
            {
                InstanceEvent nsie = new InstanceEvent();
                nsie.type = EventType.RACHED_END;
                nsie.instance = nsi;
                events.AddNoResize(nsie);
            }
            else if (nsi.DidCrossSegment(prevPos, nsi.SplinePosition))
            {
                InstanceEvent nsie = new InstanceEvent();
                nsie.type = EventType.CROSSING;
                nsie.instance = nsi;
                events.AddNoResize(nsie);
            }

            nativeSplineInstances[i] = nsi;
        }
    }

    public float speed;
    public float spawnSpeed = 1.0f;
    public float ranOffset = 2;
    public float ranOffsetIncreaseSpeed = 0.1f;
    public List<Instance> instances;
    public bool switchToBlack;
    public float switchToBlackTime;
    public Material blackMaterial;
    private Spline spline;

    private float switchToBlackTimer;
    private float spawnAmount;
    private float spawnTimer;

    void Start()
    {
        spline = GetComponent<Spline>();
        spline.instancingOptimizationFlags = SplineOptimizationFlags.DISABLE_NOISE | 
                                                SplineOptimizationFlags.DISABLE_SADDLE_SKEW | 
                                                SplineOptimizationFlags.DISABLE_Z_ROTATION | 
                                                SplineOptimizationFlags.DISABLE_SPLINE_PROFILE;
        spline.InstancingReceiveShadows = false;
    }

    void Update()
    {
        spawnAmount += Time.deltaTime * spawnSpeed;
        ranOffset += Time.deltaTime * ranOffsetIncreaseSpeed;
        spawnTimer += Time.deltaTime;
        switchToBlackTimer += Time.deltaTime;

        if (spawnTimer > 0.1f)
        {
            spawnTimer = 0;
            CreateInstances(spawnAmount);
        }

        foreach (KeyValuePair<ulong, InstanceBatch> item in spline.GetInstanceBatchesUnsafe())
        {
            InstanceBatch batch = item.Value;
            batch.Bounds = spline.controlPointsBounds;

            // Set positions
            NativeList<InstanceEvent> events = new NativeList<InstanceEvent>(batch.Count, Allocator.TempJob);
            SetPositionJob setPositionJob = new SetPositionJob()
            {
                nativeSplineInstances = batch.NativeSplineInstances.AsArray(),
                events = events.AsParallelWriter(),
                speed = speed,
                deltaTime = Time.deltaTime,
                splineLength = spline.Length,
            };
            JobHandle jobHandle = setPositionJob.Schedule(batch.Count, 32);
            jobHandle.Complete();

            // Handle events
            for (int i = 0; i < events.Length; i++)
            {
                InstanceEvent nsia = events[i];
                NativeSplineInstance nsi = nsia.instance;
                SplineInstance si = nsi.GetSplineInstance(spline);

                if (nsia.type == EventType.CROSSING)
                {
                    Segment segment = spline.GetSegmentAtIndex(nsi.closestSegmentIndex);

                    if (segment.LinkCount == 0)
                        continue;

                    Segment newSegment = segment.GetLinkAtIndex(Random.Range(0, segment.LinkCount));

                    si.SetParent(newSegment.SplineParent);
                    si.SplinePosition = new float3(nsi.SplinePosition.x, nsi.SplinePosition.y, newSegment.ZPosition);
                }
                else if (nsia.type == EventType.RACHED_END)
                {
                    si.SplinePosition = new float3(nsi.SplinePosition.x, nsi.SplinePosition.y, 0);

                    if (switchToBlack && switchToBlackTimer > switchToBlackTime && si.GetMaterialAtIndex(0) != blackMaterial)
                        si.SetMaterialAtIndex(0, blackMaterial);
                }
            }

            // Dispose
            events.Dispose();
        }
    }

    private void CreateInstances(float amount)
    {
        int a = Mathf.FloorToInt(amount);
        float r = Random.Range(0f, 1f);

        if (r < (amount - (float)a))
            a++;

        for (int i = 0; i < a; i++)
        {
            Create(0.4f, 1.0f, null);
        }

        void Create(float minScale, float maxScale, Material material)
        {
            int randomIndex = Random.Range(0, instances.Count);
            Mesh mesh = instances[randomIndex].mesh;

            if (material == null)
            {
                int randomMaterialIndex = Random.Range(0, instances[randomIndex].materials.Count);
                material = instances[randomIndex].materials[randomMaterialIndex];
            }

            float3 startPos = new float3(Random.Range(-ranOffset, ranOffset), 
                                            Random.Range(-ranOffset, ranOffset), 
                                            Random.Range(-ranOffset, ranOffset));
            float scale = Random.Range(minScale, maxScale);

            spline.CreateSplineInstance(mesh, material, startPos, quaternion.identity, 
                                                                  new Vector3(scale, scale, scale));
        }
    }
}

```

</details>