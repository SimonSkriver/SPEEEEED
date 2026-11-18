// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: Population.cs
//
// Author: Mikael Danielsson
// Date Created: 07-02-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;

using SplineArchitect.Utility;

namespace SplineArchitect
{
    public class Population
    {
        // Data that can be changed by the user
        private float startPadding;
        private float endPadding;
        private float xOffset;
        private float yOffset;
        private float spacing;
        private int resolution;
        private ResolutionMode resolutionMode = ResolutionMode.EDGE_LOOPS_ALONG_Z;
        private bool autoResolution;
        private float resolutionBias = 1;
        private int minAutoResolution;
        private int maxAutoResolution;
        private bool skipTangents;
        private int maxInstances = 0;
        private Quaternion prefabRotationOffset = Quaternion.identity;
        private Vector3 scale = Vector3.one;
        private Bounds prefabBounds;
        private PopulationFillMode fillMode = PopulationFillMode.SNAP_LAST;

        // Read only data
        private bool updateOverTime;
        private GameObject prefab;
        private bool deform;
        private Transform parent;
        private bool worldPositionStays;

        private int version;
        private int oldVersion;
        private bool resolutionDirty;
        private float splineLength;
        private int warmupCounts;
        private int warmupCount;
        private bool invalid;
        private bool hasPrefabParts;

        private List<Component> components = new List<Component>(32);

        internal List<SplineObject> activeSet;
        internal List<SplineObject> deformingSet;

        public event Action<SplineObject, PopulationSpawnSource> afterInstanceSpawned;
        public event Action<SplineObject> afterInstanceDespawned;
        public event Action afterUpdate;

        public float StartPadding
        {
            get => startPadding;
            set
            {
                if (GeneralUtility.IsEqual(value, startPadding))
                    return;

                version++;
                startPadding = value;
            }
        }
        public float EndPadding
        {
            get => endPadding;
            set
            {
                if (GeneralUtility.IsEqual(value, endPadding))
                    return;

                version++;
                endPadding = value;
            }
        }
        public float XOffset
        {
            get => xOffset;
            set
            {
                if (GeneralUtility.IsEqual(value, xOffset))
                    return;

                version++;
                xOffset = value;
            }
        }
        public float YOffset
        {
            get => yOffset;
            set
            {
                if (GeneralUtility.IsEqual(value, yOffset))
                    return;

                version++;
                yOffset = value;
            }
        }
        public float Spacing
        {
            get => spacing;
            set
            {
                if (GeneralUtility.IsEqual(value, spacing))
                    return;

                version++;
                spacing = value;
            }
        }
        public int Resolution
        {
            get => resolution;
            set
            {
                if (GeneralUtility.IsEqual(value, resolution))
                    return;

                version++;
                resolutionDirty = true;
                resolution = value;
            }
        }
        public ResolutionMode ResolutionMode
        {
            get => resolutionMode;
            set
            {
                if (value == resolutionMode)
                    return;

                version++;
                resolutionDirty = true;
                resolutionMode = value;
            }
        }
        public bool AutoResolution
        {
            get => autoResolution;
            set
            {
                if (value == autoResolution)
                    return;

                version++;
                resolutionDirty = true;
                autoResolution = value;
            }
        }
        public float ResolutionBias
        {
            get => resolutionBias;
            set
            {
                if (GeneralUtility.IsEqual(value, resolutionBias))
                    return;

                version++;
                resolutionDirty = true;
                resolutionBias = value;
            }
        }
        /// <summary>
        /// Sets MinAutoResolution on all mesh containers in the population.
        /// The lowest resolution auto resolution can set. Only used when AutoResolution is enabled.
        /// Clamped between 0 and 32. Wins over MaxAutoResolution if it's greater. Default is 0.
        /// </summary>
        public int MinAutoResolution
        {
            get => minAutoResolution;
            set
            {
                value = Mathf.Clamp(value, 0, 32);
                if (value == minAutoResolution)
                    return;

                version++;
                resolutionDirty = true;
                minAutoResolution = value;
            }
        }
        /// <summary>
        /// Sets MaxAutoResolution on all mesh containers in the population.
        /// The highest resolution auto resolution can set. Only used when AutoResolution is enabled.
        /// Clamped between 0 and 32, where 0 means no limit. Default is 0.
        /// </summary>
        public int MaxAutoResolution
        {
            get => maxAutoResolution;
            set
            {
                value = Mathf.Clamp(value, 0, 32);
                if (value == maxAutoResolution)
                    return;

                version++;
                resolutionDirty = true;
                maxAutoResolution = value;
            }
        }
        public bool SkipTangents
        {
            get => skipTangents;
            set
            {
                if (value == skipTangents)
                    return;
                version++;
                skipTangents = value;
            }
        }   
        public int MaxInstances
        {
            get => maxInstances;
            set
            {
                if (value == maxInstances)
                    return;

                version++;
                maxInstances = value;
            }
        }
        public Quaternion PrefabRotationOffset
        {
            get => prefabRotationOffset;
            set
            {
                if (GeneralUtility.IsEqual(value, prefabRotationOffset))
                    return;

                version++;
                prefabRotationOffset = value;
            }
        }
        public Vector3 Scale
        {
            get => scale;
            set
            {
                if (GeneralUtility.IsEqual(value, scale))
                    return;

                version++;
                scale = value;
            }
        }
        public GameObject Prefab => prefab;
        public Bounds PrefabBounds
        {
            get => prefabBounds;
            set 
            {
                if (GeneralUtility.IsEqual(value, prefabBounds))
                    return;

                version++;
                prefabBounds = value;
            }
        }
        /// <summary>
        /// Determines how the population fills the spline.
        /// Keep the original layout, snap the last deformation, or fit using scale, spacing, or both.
        /// Fitting with both applies the same multiplier to object lengths and spacing.
        /// </summary>
        public PopulationFillMode FillMode
        {
            get => fillMode;
            set
            {
                if (value == fillMode)
                    return;
                version++;
                fillMode = value;
            }
        }
        public bool Deform => deform;
        public Transform Parent => parent;
        public bool UpdateOverTime => updateOverTime;
        public bool WorldPositionStays => worldPositionStays;
        internal bool Invalid => invalid;
        internal bool HasPrefabParts => hasPrefabParts;

        public Population(GameObject prefab, bool deform, bool updateOverTime = true, Transform parent = null, bool worldPositionStays = true)
        {
            Mesh mesh = null;
            components.Clear();
            bool firstBoundsSet = false;
            prefab.GetComponentsInChildren(components);

            foreach (Component c in components)
            {
                if (c == null)
                    continue;

                Mesh sharedMesh = null;

                MeshFilter mf = c as MeshFilter;
                MeshCollider mc = c as MeshCollider;
                if (mf != null && mf.sharedMesh != null)
                    sharedMesh = mf.sharedMesh;
                else if (mc != null && mc.sharedMesh != null)
                    sharedMesh = mc.sharedMesh;

                if (sharedMesh != null)
                {
                    if (!sharedMesh.isReadable && deform)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"[Spline Architect] Population is invalid! The mesh used by '{prefab.name}' does not have Read/Write enabled.");
#endif
                        invalid = true;
                    }

                    float4x4 matrix = float4x4.TRS(float3.zero, quaternion.identity, prefab.transform.localScale);
                    if (c.transform != prefab.transform)
                    {
                        matrix = math.mul(matrix, float4x4.TRS(c.transform.localPosition, c.transform.localRotation, c.transform.localScale));
                        Transform p = c.transform.parent;

                        for (int i = 25; i > 0; i--)
                        {
                            if (p == null || p == prefab.transform)
                                break;

                            matrix = math.mul(matrix, float4x4.TRS(p.transform.localPosition, p.transform.localRotation, c.transform.localScale));
                            p = p.parent;
                        }
                    }

                    Bounds transformedBounds = GeneralUtility.TransformBounds(sharedMesh.bounds, matrix);

                    if (!firstBoundsSet)
                    {
                        prefabBounds = transformedBounds;
                        firstBoundsSet = true;
                    }
                    else
                    {
                        prefabBounds.Encapsulate(transformedBounds);
                    }

                    if (prefab.transform != c.transform)
                        hasPrefabParts = true;

                    mesh = sharedMesh;
                }
            }

            if (mesh == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[Spline Architect] Population is invalid! Could not find valid mesh for {prefab.name}.");
#endif
                invalid = true;
            }

            activeSet = new List<SplineObject>();
            deformingSet = new List<SplineObject>();

            this.prefab = prefab;
            this.parent = parent;
            this.deform = deform;
            this.updateOverTime = updateOverTime;
            this.worldPositionStays = worldPositionStays;

            warmupCounts = 1;
            if (updateOverTime) warmupCounts = 2;
        }

        internal bool IsVersionDirty(float splineLength)
        {
            bool dirty = version != oldVersion;
            oldVersion = version;

            //Warmup period
            if(warmupCount < warmupCounts) dirty = true;
            warmupCount++;

            if (!GeneralUtility.IsEqual(this.splineLength, splineLength))
            {
                dirty = true;
                this.splineLength = splineLength;
            }

            return dirty;
        }

        internal bool IsResolutionDirty()
        {
            bool dirty = resolutionDirty;
            resolutionDirty = false;
            return dirty;
        }

        internal void Clear()
        {
            version = 0;
            oldVersion = 0;
            resolutionDirty = false;
            splineLength = 0;
            warmupCount = 0;
            warmupCounts = 1;
            if (updateOverTime) warmupCounts = 2;
            activeSet.Clear();
            deformingSet.Clear();
        }

        internal void InvokeAfterInstanceSpawned(SplineObject splineObject, PopulationSpawnSource populationSpawnSource)
        {
            afterInstanceSpawned?.Invoke(splineObject, populationSpawnSource);
        }

        internal void InvokeAfterInstanceDespawned(SplineObject splineObject)
        {
            afterInstanceDespawned?.Invoke(splineObject);
        }

        internal void InvokeAfterUpdate()
        {
            afterUpdate?.Invoke();
        }
    }
}
