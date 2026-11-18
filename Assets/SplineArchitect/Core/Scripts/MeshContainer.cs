// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: MeshContainer.cs
//
// Author: Mikael Danielsson
// Date Created: 06-04-2023
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;

using SplineArchitect.Utility;

namespace SplineArchitect
{
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    [Serializable]
    public partial class MeshContainer
    {
        public const int dataUsage = 24 + 
                                     8 + 8 + 8 + 8 + 64 + 64 + 4;

        // Stored data
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private Mesh originMesh;
        [SerializeField] private ulong versionHash;
        [SerializeField] private int resolution;
        [SerializeField] private float autoBias = 1;
        [SerializeField] private ResolutionMode resolutionMode;
        [SerializeField] private bool autoResolution;
        [SerializeField] private UvSettings uvSettings;

        // Runtime data
        [NonSerialized] private string cachedSceneName;
        [NonSerialized] private List<Material> materialContainer;
        [NonSerialized] private MeshRenderer meshRenderer;
        // Only set through the API. 0 is the default for both, a max of 0 means no limit.
        [NonSerialized] private int minAutoResolution;
        [NonSerialized] private int maxAutoResolution;

        // Runtime, keys
        [NonSerialized] private string meshKey;
        [NonSerialized] private string dataKey;

        // Runtime, monitor values
#if UNITY_6000_4_OR_NEWER
        [NonSerialized] private ulong monitorTransformId;
        [NonSerialized] private ulong monitorOriginMeshId;
#else
        [NonSerialized] private int monitorTransformId;
        [NonSerialized] private int monitorOriginMeshId;
#endif
        [NonSerialized] private int monitorResolutionId;
        [NonSerialized] private ResolutionMode monitorResolutionModeId;

        /// <summary>
        /// Adds vertices and triangles to the instance mesh derived from the original mesh.
        /// A mesh’s highest resolution value is usually around 14, but can vary depending on the mesh structure.
        /// </summary>
        public int Resolution
        {
            get => resolution;
            set
            {
                if(value > 32)
                    value = 32;

                resolution = value;
            }
        }
        /// <summary>
        /// Adds detail only along the Z axis, improving smoothness while keeping vertex count lower. 
        /// Requires the mesh’s Z axis to align with the spline direction. Do not rotate meshes when this option is enabled.
        /// </summary>
        public ResolutionMode ResolutionMode
        {
            get => resolutionMode;
            set => resolutionMode = value;
        }
        /// <summary>
        /// Enables auto resolution. The mesh resolution increases based on the curvature of the spline. 
        /// </summary>
        public bool AutoResolution
        {
            get => autoResolution;
            set => autoResolution = value;
        }
        public int MaterialCount
        {
            get
            {
                Component meshComponent = GetMeshComponent();

                if (meshComponent == null) return 0;
                if (meshComponent is not MeshFilter) return 0;

                if (meshRenderer == null) 
                    meshRenderer = GetMeshComponent().GetComponent<MeshRenderer>();

                if (meshRenderer == null) return 0;

                if(materialContainer == null)
                    materialContainer = new List<Material>();

                meshRenderer.GetSharedMaterials(materialContainer);

                return materialContainer.Count;
            }
        }   
        public float AutoBias
        {
            get => autoBias;
            set
            {
                value = Mathf.Clamp(value, 0, 10);
                value = Mathf.Round(value * 100) / 100;
                autoBias = value;
            }
        }
        /// <summary>
        /// The lowest resolution auto resolution can set. Only used when AutoResolution is enabled.
        /// Clamped between 0 and 32. Wins over MaxAutoResolution if it's greater. Default is 0.
        /// Not serialized, it resets to 0 when scripts or the scene reloads and when the object is instantiated.
        /// </summary>
        public int MinAutoResolution
        {
            get => minAutoResolution;
            set => minAutoResolution = Mathf.Clamp(value, 0, 32);
        }
        /// <summary>
        /// The highest resolution auto resolution can set. Only used when AutoResolution is enabled.
        /// Clamped between 0 and 32, where 0 means no limit. Default is 0.
        /// Not serialized, it resets to 0 when scripts or the scene reloads and when the object is instantiated.
        /// </summary>
        public int MaxAutoResolution
        {
            get => maxAutoResolution;
            set => maxAutoResolution = Mathf.Clamp(value, 0, 32);
        }

        internal MeshContainer(Component component)
        {
            MeshFilter meshFilter = component as MeshFilter;
            MeshCollider meshCollider = component as MeshCollider;

            if (meshFilter == null && meshCollider == null)
                throw new InvalidOperationException($"Both MeshFilter and MeshCollider cant be null.");
            else if (meshFilter != null && meshCollider != null)
                throw new InvalidOperationException($"Can't contain a valid MeshCollider and MeshFilter. Can only contain one of them.");

            this.meshFilter = meshFilter;
            this.meshCollider = meshCollider;

            if (meshFilter != null)
                originMesh = meshFilter.sharedMesh;

            if (meshCollider != null)
                originMesh = meshCollider.sharedMesh;

#if UNITY_EDITOR
            EnsureValidOriginMesh();
            TryUpdateVersionHash();
#endif
        }

        public void RenderMesh(bool value)
        {
            if (meshFilter != null)
            {
                if(meshFilter.TryGetComponent(out MeshRenderer meshRender))
                    meshRender.enabled = value;
            }
        }

        public bool IsMeshRendered()
        {
            if(meshFilter != null)
            {
                if(meshFilter.TryGetComponent(out MeshRenderer meshRender))
                    return meshRender.enabled;
            }

            return false;
        }

        /// <summary>
        /// Gets the deformed instance meshes used inside the mesh filter or mesh collider.
        /// </summary>
        public Mesh GetInstanceMesh()
        {
            if (meshCollider != null && meshCollider.sharedMesh != null)
                return meshCollider.sharedMesh;
            else if (meshFilter != null && meshFilter.sharedMesh != null)
                return meshFilter.sharedMesh;
            else
                return null;
        }

        /// <summary>
        /// Gets the mesh that the instance mesh originates from.
        /// </summary>
        public Mesh GetOriginMesh()
        {
            return originMesh;
        }

        /// <summary>
        /// Sets the mesh that the instance mesh originates from.
        /// You can change this at runtime, but remember to run
        /// SyncInstanceMeshesFromCache on the SplineObject afterwards.
        /// </summary>
        public void SetOriginMesh(Mesh originMesh)
        {
#if UNITY_EDITOR
            string path = EGeneralUtility.GetAssetPathOnlyEditor(originMesh);
            if (!Application.isPlaying && path == "")
            {
                if (originMesh != null)
                    Debug.LogError($"[Spline Architect] Can't set origin mesh! {originMesh.name} does not have an asset path!");
                else
                    Debug.LogError($"[Spline Architect] Can't set origin mesh! OriginMesh does not have an asset path!");
                return;
            }
#endif

            this.originMesh = originMesh;
        }

        public Component GetMeshComponent()
        {
            if (meshFilter != null)
                return meshFilter;
            else
                return meshCollider;
        }

        public bool MeshComponentIsMeshFilter()
        {
            if (meshFilter != null) return true;
            return false;
        }

        /// <summary>
        /// Sets the shared material at the specified material slot index.
        /// </summary>
        public void SetMaterialAtIndex(int index, Material material)
        {
            Component meshComponent = GetMeshComponent();

            if (meshComponent == null)
            {
                Debug.LogError($"[Spline Architect] Can't set material, meshComponent is null!");
                return;
            }

            if(meshComponent is not MeshFilter)
            {
                Debug.LogError($"[Spline Architect] Can't set material, meshComponent is not a MeshFilter!");
                return;
            }

            if (meshRenderer == null)
                meshRenderer = meshComponent.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError($"[Spline Architect] Can't set material, no MeshRenderer found!");
                return;
            }

            if(meshRenderer.transform != meshComponent.transform)
            {
                Debug.LogError($"[Spline Architect] Can't set material, meshRenderer and meshComponent are on different transforms!");
                return;
            }

            if(materialContainer == null)
                materialContainer = new List<Material>();

            materialContainer.Clear();
            meshRenderer.GetSharedMaterials(materialContainer);
            materialContainer[index] = material;
            meshRenderer.SetSharedMaterials(materialContainer);
        }

        public void SetOriginMeshAndMaterials(Mesh originMesh, List<Material> materials)
        {
            Component meshComponent = GetMeshComponent();

            if (meshComponent == null)
            {
                Debug.LogError($"[Spline Architect] Can't set origin mesh and materials, meshComponent is null!");
                return;
            }

            if (meshComponent is not MeshFilter)
            {
                Debug.LogError($"[Spline Architect] Can't set origin mesh and materials, meshComponent is not a MeshFilter!");
                return;
            }

            if (meshRenderer == null)
                meshRenderer = meshComponent.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError($"[Spline Architect] Can't set origin mesh and materials, no MeshRenderer found!");
                return;
            }

            if (meshRenderer.transform != meshComponent.transform)
            {
                Debug.LogError($"[Spline Architect] Can't set origin mesh and materials, meshRenderer and meshComponent are on different transforms!");
                return;
            }

            SetOriginMesh(originMesh);
            meshRenderer.SetSharedMaterials(materials);
        }

        /// <summary>
        /// Gets the shared materials.
        /// </summary>
        public void GetMaterials(List<Material> materials)
        {
            Component meshComponent = GetMeshComponent();
            
            if (meshComponent == null)
            {
                Debug.LogError($"[Spline Architect] Can't get materials, meshComponent is null!");
                return;
            }

            if (meshComponent is not MeshFilter)
            {
                Debug.LogError($"[Spline Architect] Can't get materials, meshComponent is not a MeshFilter!");
                return;
            }

            if (meshRenderer == null)
                meshRenderer = meshComponent.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError($"[Spline Architect] Can't get materials, no MeshRenderer found!");
                return;
            }

            if (meshRenderer.transform != meshComponent.transform)
            {
                Debug.LogError($"[Spline Architect] Can't get materials, meshRenderer and meshComponent are on different transforms!");
                return;
            }

            meshRenderer.GetSharedMaterials(materials);
        }

        public void SetUvOffsetAtIndex(int index, Vector2 value)
        {
            if(index == 0) uvSettings.uvOffset = value;
            else if(index == 1) uvSettings.uv2Offset = value;
            else if(index == 2) uvSettings.uv3Offset = value;
            else if(index == 3) uvSettings.uv4Offset = value;
        }

        public Vector2 GetUvOffsetAtIndex(int index)
        {
            if(index == 0) return uvSettings.uvOffset;
            else if(index == 1) return uvSettings.uv2Offset;
            else if(index == 2) return uvSettings.uv3Offset;
            else if(index == 3) return uvSettings.uv4Offset;

            Debug.LogError("[SplineArchitect] Invalid index! Need to be between 0 to 3.");

            return Vector2.zero;
        }

        public UvSettings GetUvSettings()
        {
            return uvSettings;
        }

        internal bool ContainsMeshComponent(Component component)
        {
            if (component == null)
                return false;

            if (meshFilter == component) return true;
            if (meshCollider == component) return true;
            return false;
        }

        internal void SetInstanceMesh(Mesh instanceMesh)
        {
#if UNITY_EDITOR
            if (originMesh == null)
                return;

            if (instanceMesh == originMesh)
            {
                Debug.LogError("[Spline Architect] InstanceMesh and OriginMesh is the same!");
                return;
            }
#endif

            if (meshFilter != null) 
                meshFilter.sharedMesh = instanceMesh;
            else if (meshCollider != null) 
                meshCollider.sharedMesh = instanceMesh;
#if UNITY_EDITOR
            else
                Debug.LogError($"[Spline Architect] Could not find MeshFilter or MeshCollider for: {instanceMesh.name}");
#endif
        }

        internal void SetInstanceMeshToOriginMesh()
        {
            if (meshFilter != null)
                meshFilter.sharedMesh = originMesh;
            else if (meshCollider != null)
                meshCollider.sharedMesh = originMesh;
#if UNITY_EDITOR
            else
                Debug.LogError($"[Spline Architect] Could not find MeshFilter or MeshCollider for: {originMesh.name}");
#endif
        }

        internal string GetMeshKey()
        {
#if UNITY_6000_4_OR_NEWER
            ulong transformId = EntityId.ToULong(GetMeshComponent().transform.GetEntityId());
            ulong originMeshId = EntityId.ToULong(originMesh.GetEntityId());
            if (string.IsNullOrEmpty(meshKey) || monitorTransformId != transformId ||
                                                 monitorOriginMeshId != originMeshId ||
                                                 monitorResolutionId != resolution ||
                                                 monitorResolutionModeId != resolutionMode)
            {
                UpdateKeys();
            }
#else
            if (string.IsNullOrEmpty(meshKey) || monitorTransformId != GetMeshComponent().transform.GetInstanceID() ||
                                                 monitorOriginMeshId != originMesh.GetInstanceID() ||
                                                 monitorResolutionId != resolution ||
                                                 monitorResolutionModeId != resolutionMode)
            {
                UpdateKeys();
            }
#endif

            return meshKey;
        }

        internal string GetDataKey()
        {
#if UNITY_6000_4_OR_NEWER
            ulong originMeshId = EntityId.ToULong(originMesh.GetEntityId());
            if (string.IsNullOrEmpty(dataKey) || monitorOriginMeshId != originMeshId ||
                                                 monitorResolutionId != resolution ||
                                                 monitorResolutionModeId != resolutionMode)
                UpdateKeys();

            return dataKey;
#else
            if (string.IsNullOrEmpty(dataKey) || monitorOriginMeshId != originMesh.GetInstanceID() ||
                                                 monitorResolutionId != resolution || 
                                                 monitorResolutionModeId != resolutionMode)
                UpdateKeys();

            return dataKey;
#endif
        }

        internal string GetCachedSceneName()
        {
            if (cachedSceneName == null)
                cachedSceneName = GetMeshComponent().gameObject.scene.name;

            return cachedSceneName;
        }

        internal void UpdateKeys()
        {
            Component component = GetMeshComponent();
            if (component == null) return;

#if UNITY_6000_4_OR_NEWER
            monitorTransformId = EntityId.ToULong(component.transform.GetEntityId());
            monitorOriginMeshId = EntityId.ToULong(originMesh.GetEntityId());
#else
            monitorTransformId = component.transform.GetInstanceID();
            monitorOriginMeshId = originMesh.GetInstanceID();
#endif
            monitorResolutionId = resolution;
            monitorResolutionModeId = resolutionMode;
            string sceneName = GetCachedSceneName();
            ulong version = versionHash;

#if !UNITY_EDITOR
            version = 0;
#endif

            ulong instanceMeshHash = CreateHashForMeshKey((ulong)monitorTransformId, (ulong)monitorOriginMeshId, version, monitorResolutionId, monitorResolutionModeId);
            meshKey = HandleCachedResources.FeatchStringKey(instanceMeshHash);
            if(meshKey == null)
            {
                meshKey = $"{monitorTransformId}*{monitorOriginMeshId}*{version}*{resolution}*{(int)resolutionMode}";
                HandleCachedResources.AddStringKey(instanceMeshHash, meshKey, sceneName);
            }

            ulong originalMeshHash = CreateHashForDataKey(sceneName, (ulong)monitorOriginMeshId, version, monitorResolutionId, monitorResolutionModeId);
            dataKey = HandleCachedResources.FeatchStringKey(originalMeshHash);
            if(dataKey == null)
            {
                dataKey = $"{sceneName}*{monitorOriginMeshId}*{version}*{resolution}*{(int)resolutionMode}";
                HandleCachedResources.AddStringKey(originalMeshHash, dataKey, sceneName);
            }
        }

        internal ulong CreateHashForMeshKey()
        {
            ulong version = versionHash;

#if !UNITY_EDITOR
            version = 0;
#endif

#if UNITY_6000_4_OR_NEWER
            ulong transformId = EntityId.ToULong(GetMeshComponent().transform.GetEntityId());
            ulong uriginMeshId = EntityId.ToULong(originMesh.GetEntityId());
            return CreateHashForMeshKey(transformId, uriginMeshId, version, resolution, resolutionMode);
#else
            int transformId = GetMeshComponent().transform.GetInstanceID();
            int uriginMeshId = originMesh.GetInstanceID();
            return CreateHashForMeshKey((ulong)transformId, (ulong)uriginMeshId, version, resolution, resolutionMode);
#endif
        }

        private ulong CreateHashForMeshKey(ulong transformsInstanceId,
                                ulong originMeshId,
                                ulong version,
                                int resolution,
                                ResolutionMode resolutionMode)
        { 
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;

            hash ^= 2UL;
            hash *= prime;

            hash ^= transformsInstanceId;
            hash *= prime;

            hash ^= originMeshId;
            hash *= prime;

            hash ^= version;
            hash *= prime;

            hash ^= (ulong)resolution;
            hash *= prime;

            hash ^= (ulong)resolutionMode;
            hash *= prime;

            return hash;
        }

        private ulong CreateHashForDataKey(string sceneName,
                                ulong originMeshId,
                                ulong version,
                                int resolution,
                                ResolutionMode resolutionMode)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;

            hash ^= 1UL;
            hash *= prime;

            hash ^= (ulong)sceneName.GetHashCode();
            hash *= prime;

            hash ^= originMeshId;
            hash *= prime;

            hash ^= version;
            hash *= prime;

            hash ^= (ulong)resolution;
            hash *= prime;

            hash ^= (ulong)resolutionMode;
            hash *= prime;

            return hash;
        }
    }
}