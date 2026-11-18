// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: Spline_Population.cs
//
// Author: Mikael Danielsson
// Date Created: 15-01-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;

using SplineArchitect.Utility;

namespace SplineArchitect
{
    public partial class Spline : MonoBehaviour
    {
        private const float minPopulationSectionLength = 0.05f;
        private const float fitSnapDistance = 0.01f;

        [NonSerialized] internal bool finalizePopulateUsingPoolJobSafe;
        [NonSerialized] internal HashSet<Population> populations = new HashSet<Population>();
        [NonSerialized] private Dictionary<GameObject, List<SplineObject>> populationPoolDeformations = new Dictionary<GameObject, List<SplineObject>>();
        [NonSerialized] private Dictionary<GameObject, List<SplineObject>> populationPoolFollowers = new Dictionary<GameObject, List<SplineObject>>();

        [NonSerialized] private List<SplineObject> splineObjectContainer = new List<SplineObject>();
        [NonSerialized] private List<Population> populationContainer = new List<Population>();
        [NonSerialized] private List<SplineObject> splineObjectContainerSnap = new List<SplineObject>();

        public void AddPopulation(Population population)
        {
            if (populations.Contains(population))
            {
#if UNITY_EDITOR
                Debug.LogError("[Spline Architect] Population is already added to this spline.");
#endif
                return;
            }

            if(population.Invalid)
            {
#if UNITY_EDITOR
                Debug.LogError("[Spline Architect] Population is invalid, could not add it to the spline.");
#endif
                return;
            }

            if (population.HasPrefabParts)
            {
#if UNITY_EDITOR
                Debug.LogError("[Spline Architect] Prefab children used in population are only supported during baking.");
#endif
                return;
            }

            population.Clear();
            populations.Add(population);
        }

        public void RemovePopulation(Population population, PopulationRemoveOption populationRemoveOption)
        {
            if (populationRemoveOption != PopulationRemoveOption.DO_NOTHING)
            {
                // Handle deforming set
                if (population.UpdateOverTime)
                {
                    if (jobState != JobState.IDLE)
                        CompleteOngoingWorkers();

                    for (int i = population.deformingSet.Count - 1; i >= 0; i--)
                    {
                        SplineObject so = population.deformingSet[i];
                        RemoveSplineObject(so);

                        if (populationRemoveOption == PopulationRemoveOption.DISABLE_AND_POOL)
                        {
                            List<SplineObject> pool = GetPopulationPool(population.Prefab, population.Deform);
                            so.gameObject.SetActive(false);
                            pool.Add(so);
                        }
                        else 
                        {
                            Destroy(so.gameObject);
                        }

                        population.deformingSet.RemoveAt(i);
                        population.InvokeAfterInstanceDespawned(so);
                    }
                }

                // Handle active set
                for (int i = population.activeSet.Count - 1; i >= 0; i--)
                {
                    SplineObject so = population.activeSet[i];

                    if (!population.UpdateOverTime)
                        RemoveSplineObject(so);

                    if (populationRemoveOption == PopulationRemoveOption.DISABLE_AND_POOL)
                    {
                        List<SplineObject> pool = GetPopulationPool(population.Prefab, population.Deform);
                        so.gameObject.SetActive(false);
                        pool.Add(so);
                    }
                    else
                    {
                        Destroy(so.gameObject);
                    }

                    population.activeSet.RemoveAt(i);
                    population.InvokeAfterInstanceDespawned(so);
                }
            }

            populations.Remove(population);
        }

        public bool ContainsPopulation(Population population)
        {
            return populations.Contains(population);
        }

        public void ClearPopulations(PopulationRemoveOption populationRemoveOption)
        {
            populationContainer.Clear();
            populationContainer.AddRange(populations);

            for (int i = populationContainer.Count - 1; i >= 0; i--)
            {
                RemovePopulation(populationContainer[i], populationRemoveOption);
            }

            populationContainer.Clear();
            populations.Clear();
        }

        /// <summary>
        /// Bakes (instantiates) the given Population into SplineObjects. 
        /// Baked objects are not managed by the Population system
        /// and will not update when the spline or Population settings change.
        /// </summary>
        public void BakePopulation(Population population, 
                                   ICollection<SplineObject> resultSplineObjects = null)
        {
            GameObject prefab = population.Prefab;
            Transform parent = population.Parent;
            Quaternion prefabRotationOffset = population.PrefabRotationOffset;
            float startPadding = population.StartPadding;
            float xOffset = population.XOffset;
            float yOffset = population.YOffset;
            bool skipTangents = population.SkipTangents;
            bool deform = population.Deform;
            GetPopulationLayout(population, out int amount, out float splineLength, out float soLength,
                                out float alignOffset, out Vector3 fittedScale, out PopulationFillMode fillMode);

            splineObjectContainer.Clear();
            for (int i = 0; i < amount; i++)
            {
                SplineObject soClone = null;
                GameObject goClone = Instantiate(prefab);
                if (deform)
                {
                    soClone = CreateDeformation(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                    CreateDeformationsForChildren(goClone, splineObjectContainer);
                }
                else
                {
                    soClone = CreateFollower(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                }

#if UNITY_EDITOR
                if (soClone == null)
                    continue;
#endif
                splineObjectContainer.Add(soClone);
                soClone.localSplinePosition = new Vector3(xOffset, yOffset, startPadding + alignOffset + (soLength * i));
                ApplyPopulationResolution(population, soClone);
                soClone.transform.localScale = fittedScale;
                soClone.SkipTangents = skipTangents;
                population.InvokeAfterInstanceSpawned(soClone, PopulationSpawnSource.CREATED);
            }

            SplineObject last = splineObjectContainer.Count > 0 ? splineObjectContainer[splineObjectContainer.Count - 1] : null;

            //End snapping
            if (ShouldSnapLast(fillMode) && last != null)
            {
                SnapLastInPopulation(population, last, soLength, splineLength, fillMode);
            }

            foreach (SplineObject soChild in splineObjectContainer)
            {
                soChild.enabled = false;
                directSystemWorker.Add(soChild);
                if(resultSplineObjects != null)
                    resultSplineObjects.Add(soChild);
            }

            directSystemWorker.Complete();
        }

        internal List<SplineObject> GetPopulationPool(GameObject prefab, bool deformation)
        {
            List<SplineObject> newPool = null;

            if (deformation)
            {
                if (populationPoolDeformations.ContainsKey(prefab))
                    return populationPoolDeformations[prefab];

                newPool = new List<SplineObject>();
                populationPoolDeformations.Add(prefab, newPool);
            }
            else
            {
                if (populationPoolFollowers.ContainsKey(prefab))
                    return populationPoolFollowers[prefab];

                newPool = new List<SplineObject>();
                populationPoolFollowers.Add(prefab, newPool);
            }

            return newPool;
        }

        internal void PopulateUsingPoolJobSafe(Population population)
        {
            GameObject prefab = population.Prefab;
            Transform parent = population.Parent;
            Quaternion prefabRotationOffset = population.PrefabRotationOffset;
            float startPadding = population.StartPadding;
            float xOffset = population.XOffset;
            float yOffset = population.YOffset;
            bool skipTangents = population.SkipTangents;
            bool deform = population.Deform;
            List<SplineObject> pool = GetPopulationPool(prefab, deform);

            // Move active set to pool
            for (int i = population.activeSet.Count - 1; i >= 0; i--)
            {
                SplineObject so = population.activeSet[i];
                pool.Add(so);
                so.RenderMesh(false);
                population.activeSet.RemoveAt(i);
                population.InvokeAfterInstanceDespawned(so);
            }

            // Render or move to pool
            for (int i = population.deformingSet.Count - 1; i >= 0; i--)
            {
                SplineObject so = population.deformingSet[i];

                if (finalizePopulateUsingPoolJobSafe)
                {
                    pool.Add(so);
                    so.RenderMesh(false);
                }
                else
                {
                    population.activeSet.Add(so);
                    so.RenderMesh(true);
                }

                RemoveSplineObject(so);
                population.deformingSet.RemoveAt(i);
            }

            GetPopulationLayout(population, out int amount, out float splineLength, out float soLength,
                                out float alignOffset, out Vector3 fittedScale, out PopulationFillMode fillMode);

            // Create new set
            for (int i = 0; i < amount; i++)
            {
                SplineObject soClone = null;
                PopulationSpawnSource populationSpawnSource = PopulationSpawnSource.POOLED;

                if (pool.Count > 0)
                {
                    soClone = pool[0];
                    pool.RemoveAt(0);
                    soClone.gameObject.SetActive(true);
                    soClone.transform.parent = parent == null ? transform : parent;
                }

                if (soClone == null)
                {
                    GameObject goClone = Instantiate(prefab);
                    if (deform)
                    {
                        soClone = CreateDeformation(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                        soClone.EnsureMeshesInitialized();
                    }
                    else soClone = CreateFollower(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                    populationSpawnSource = PopulationSpawnSource.CREATED;

                }

                soClone.localSplinePosition = new Vector3(xOffset, yOffset, startPadding + alignOffset + (soLength * i));
                soClone.transform.localScale = fittedScale;
                soClone.RenderMesh(finalizePopulateUsingPoolJobSafe ? true : false);
                soClone.SkipTangents = skipTangents;
                ApplyPopulationResolution(population, soClone);

                population.deformingSet.Add(soClone);
                AddSplineObject(soClone);
                soClone.MarkVersionDirty();
                population.InvokeAfterInstanceSpawned(soClone, populationSpawnSource);
            }

            int lastId = -1;
            float zMax = -99999;

            //End snapping
            for (int i = population.deformingSet.Count - 1; i >= 0; i--)
            {
                SplineObject so = population.deformingSet[i];

                if (so.localSplinePosition.z > zMax)
                {
                    zMax = so.localSplinePosition.z;
                    lastId = i;
                }

                ClearPopulationSnap(population, so);
            }

            if (ShouldSnapLast(fillMode) && lastId != -1)
            {
                SplineObject last = population.deformingSet[lastId];
                SnapLastInPopulation(population, last, soLength, splineLength, fillMode);
            }
        }

        internal void PopulateUsingPool(Population population)
        {
            GameObject prefab = population.Prefab;
            Transform parent = population.Parent;
            Quaternion prefabRotationOffset = population.PrefabRotationOffset;
            float startPadding = population.StartPadding;
            float xOffset = population.XOffset;
            float yOffset = population.YOffset;
            bool skipTangents = population.SkipTangents;
            bool deform = population.Deform;
            List<SplineObject> pool = GetPopulationPool(prefab, deform);
            GetPopulationLayout(population, out int amount, out float splineLength, out float soLength,
                                out float alignOffset, out Vector3 fittedScale, out PopulationFillMode fillMode);

            // Update existing spline objects if scale, padding or offset changes.
            bool check1 = population.activeSet.Count > 0 && (!GeneralUtility.IsEqual(population.activeSet[0].localSplinePosition.z, alignOffset + startPadding) ||
                                                             !GeneralUtility.IsEqual(population.activeSet[0].localSplinePosition.x, xOffset) ||
                                                             !GeneralUtility.IsEqual(population.activeSet[0].localSplinePosition.y, yOffset) ||
                                                             !GeneralUtility.IsEqual(population.activeSet[0].transform.localScale, fittedScale));
            bool check2 = population.activeSet.Count > 1 && !GeneralUtility.IsEqual(population.activeSet[1].localSplinePosition.z - population.activeSet[0].localSplinePosition.z, soLength);
            if (check1 || check2)
            {
                for (int i = 0; i < population.activeSet.Count; i++)
                {
                    SplineObject so = population.activeSet[i];

                    // Apply scale
                    if(!GeneralUtility.IsEqual(fittedScale, so.transform.localScale))
                    {
                        so.transform.localScale = fittedScale;
                        so.MarkVersionDirty();
                    }

                    // apply position
                    so.localSplinePosition = new Vector3(xOffset, yOffset, startPadding + alignOffset + (soLength * i));
                }
            }

            // Update existing spline objects if resolution settings changes.
            if (population.IsResolutionDirty())
            {
                for (int i = 0; i < population.activeSet.Count; i++)
                {
                    SplineObject so = population.activeSet[i];

                    if (ApplyPopulationResolution(population, so))
                        so.MarkVersionDirty();
                }
            }

            //Get dif
            int dif = amount - population.activeSet.Count;

            //Add
            if (dif > 0)
            {
                for (int i = population.activeSet.Count; i < amount; i++)
                {
                    SplineObject soClone = null;
                    PopulationSpawnSource populationSpawnSource = PopulationSpawnSource.POOLED;

                    if (pool.Count > 0)
                    {
                        soClone = pool[0];
                        pool.RemoveAt(0);
                        soClone.gameObject.SetActive(true);
                        soClone.RenderMesh(true);
                        soClone.transform.parent = parent == null ? transform : parent;
                    }

                    if (soClone == null)
                    {
                        GameObject goClone = Instantiate(prefab);
                        if (deform) soClone = CreateDeformation(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                        else soClone = CreateFollower(goClone, Vector3.zero, prefabRotationOffset, parent, population.WorldPositionStays);
                        populationSpawnSource = PopulationSpawnSource.CREATED;
                    }

#if UNITY_EDITOR
                    if (soClone == null)
                        continue;
#endif
                    soClone.transform.localScale = fittedScale;
                    soClone.localSplinePosition = new Vector3(xOffset,
                                                              yOffset,
                                                              startPadding + alignOffset + (soLength * i));
                    
                    soClone.SkipTangents = skipTangents;
                    ApplyPopulationResolution(population, soClone);
                    soClone.MarkVersionDirty();
                    population.activeSet.Add(soClone);
                    population.InvokeAfterInstanceSpawned(soClone, populationSpawnSource);
                }
            }
            //Remove
            else if (dif < 0)
            {
                for (int i = population.activeSet.Count - 1; i >= amount; i--)
                {   
                    SplineObject so = population.activeSet[i];
                    population.activeSet.RemoveAt(i);
                    pool.Add(so);
                    so.gameObject.SetActive(false);
                    so.RenderMesh(false);
                    population.InvokeAfterInstanceDespawned(so);
                }
            }

            //End snapping
            for (int i = 0; i < population.activeSet.Count; i++)
            {
                SplineObject so = population.activeSet[i];

                if (ShouldSnapLast(fillMode) && i == population.activeSet.Count - 1)
                {
                    SnapLastInPopulation(population, so, soLength, splineLength, fillMode);
                }
                else
                {
                    ClearPopulationSnap(population, so);
                }
            }
        }

        private void GetPopulationLayout(Population population, out int amount, out float splineLength, out float soLength,
                                         out float alignOffset, out Vector3 fittedScale, out PopulationFillMode fillMode)
        {
            Vector3 scale = population.Scale;
            Bounds prefabBounds = population.PrefabBounds;
            // Prefab bounds already include the prefab scale, the population scale is applied on top of it.
            prefabBounds.center = Vector3.Scale(prefabBounds.center, scale);
            prefabBounds.extents = Vector3.Scale(prefabBounds.extents, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            float prefabLength = prefabBounds.size.z;
            // A section plus its spacing can never take up less than minPopulationSectionLength along the spline.
            float spacing = Mathf.Max(population.Spacing, minPopulationSectionLength - prefabLength);
            float zScaleMultiplier = 1f;

            splineLength = length;
            soLength = prefabLength + spacing;
            alignOffset = -prefabBounds.min.z;
            fittedScale = Vector3.Scale(population.Prefab.transform.localScale, scale);
            fillMode = population.FillMode;

            // Ignore the final segment when its anchor duplicates the previous anchor.
            bool skipLastSegment = segments.Count > 1 &&
                                   GeneralUtility.IsEqual(segments[segments.Count - 1].anchor,
                                                          segments[segments.Count - 2].anchor);
            if (skipLastSegment)
                splineLength -= segments[segments.Count - 2].length;

            float availableLength = Mathf.Max(0f, splineLength - population.StartPadding - population.EndPadding);
            amount = soLength > 0f ? Mathf.Max(1, Mathf.FloorToInt((availableLength + soLength / 2f + spacing / 2f) / soLength)) : 1;
            if (population.MaxInstances > 0)
                amount = Mathf.Min(amount, population.MaxInstances);

            if (isInvalidShape || (skipLastSegment && segments.Count == 2))
                amount = 0;

            if (amount == 0 || prefabLength <= 0f)
                return;

            // A single deforming section cannot snap independently at both ends.
            if (amount == 1 && fillMode == PopulationFillMode.SNAP_LAST && population.Deform)
                fillMode = PopulationFillMode.FIT_USING_SCALE;

            if(fillMode == PopulationFillMode.FIT_USING_SCALE)
            {
                zScaleMultiplier = Mathf.Max(0f, availableLength - spacing * (amount - 1)) / (prefabLength * amount);
            }
            else if(fillMode == PopulationFillMode.FIT_USING_SPACING)
            {
                // Keep the base scale and avoid negative gaps by reducing the count when needed.
                amount = Mathf.Min(amount, Mathf.FloorToInt(availableLength / prefabLength));
                if (amount == 0)
                    return;

                // With one section, the remaining space is a trailing gap.
                spacing = Mathf.Max(0f, availableLength - prefabLength * amount) / Mathf.Max(1, amount - 1);
            }

            soLength = Mathf.Max(minPopulationSectionLength, prefabLength * zScaleMultiplier + spacing);
            alignOffset *= zScaleMultiplier;
            fittedScale.z *= zScaleMultiplier;
        }

        // Returns true if any mesh container setting changed.
        private bool ApplyPopulationResolution(Population population, SplineObject so)
        {
            bool changed = false;

            for (int i = 0; i < so.MeshContainerCount; i++)
            {
                MeshContainer mc = so.GetMeshContainerAtIndex(i);
                int oldResolution = mc.Resolution;
                ResolutionMode oldResolutionMode = mc.ResolutionMode;
                bool oldAutoResolution = mc.AutoResolution;
                float oldAutoBias = mc.AutoBias;
                int oldMinAutoResolution = mc.MinAutoResolution;
                int oldMaxAutoResolution = mc.MaxAutoResolution;

                mc.ResolutionMode = population.ResolutionMode;
                mc.AutoResolution = population.AutoResolution;
                mc.AutoBias = population.ResolutionBias;
                mc.MinAutoResolution = population.MinAutoResolution;
                mc.MaxAutoResolution = population.MaxAutoResolution;

                // Auto resolution sets the resolution during deformation.
                if (!population.AutoResolution)
                    mc.Resolution = population.Resolution;

                // Compared after assigning since the mesh container clamps and rounds some values.
                if (oldResolution != mc.Resolution ||
                    oldResolutionMode != mc.ResolutionMode ||
                    oldAutoResolution != mc.AutoResolution ||
                    !GeneralUtility.IsEqual(oldAutoBias, mc.AutoBias) ||
                    oldMinAutoResolution != mc.MinAutoResolution ||
                    oldMaxAutoResolution != mc.MaxAutoResolution)
                    changed = true;
            }

            return changed;
        }

        private void SnapLastInPopulation(Population population, SplineObject last, float soLength, float splineLength, PopulationFillMode fillMode)
        {
            if (last.MeshContainerCount > 0)
            {
                Snap(last);
            }

            if (last.transform.childCount > 0)
            {
                splineObjectContainerSnap.Clear();
                last.gameObject.GetComponentsInChildren(splineObjectContainerSnap);

                for (int i = 0; i < splineObjectContainerSnap.Count; i++)
                {
                    SplineObject soChild = splineObjectContainerSnap[i];

                    if (soChild == null || soChild.MeshContainerCount == 0)
                        continue;

                    MeshContainer mc = soChild.GetMeshContainerAtIndex(0);
                    Bounds bounds = mc.GetOriginMesh().bounds;
                    bounds = GeneralUtility.TransformBoundsToWorldSpace(bounds, soChild.transform);
                    bounds = GeneralUtility.TransformBoundsToLocalSpace(bounds, last.transform);
                    float partBoundsEnd = bounds.center.z + bounds.extents.z;
                    float wholeBoundsEnd = population.PrefabBounds.center.z + population.PrefabBounds.extents.z;

                    float partsEnd = partBoundsEnd + soChild.splinePosition.z;
                    float wholeEnd = splineLength - population.EndPadding - 0.001f;

                    if (partBoundsEnd >= (wholeBoundsEnd - 0.001f) || partsEnd >= (wholeEnd - 0.001f))
                        Snap(soChild);
                }
            }

            void Snap(SplineObject so)
            {
                // Control point end is exact, spline point can leave tiny gaps (e.g. at road intersections). So only use spline point when the end is padded.
                SnapMode snapMode = GeneralUtility.IsZero(population.EndPadding) ? SnapMode.CONTROL_POINT_END : SnapMode.SPLINE_POINT;
                float snapTargetPoint = splineLength - population.EndPadding;

                // When the last segment is skipped (its anchor duplicates the previous anchor) the population ends at the second last control point.
                // Control point end would snap to the last control point and include the skipped segment's length.
                if (snapMode == SnapMode.CONTROL_POINT_END && segments.Count > 2 &&
                    GeneralUtility.IsEqual(segments[segments.Count - 1].anchor, segments[segments.Count - 2].anchor))
                {
                    snapMode = SnapMode.SPLINE_POINT;
                    snapTargetPoint = segments[segments.Count - 2].zPosition;
                }

                // Fit modes already end at the target, the snap only removes precision errors.
                float snapDistance = fillMode == PopulationFillMode.SNAP_LAST ? soLength : fitSnapDistance;
                SetPopulationSnap(so, snapMode, snapDistance, snapTargetPoint);
            }
        }

        private bool ShouldSnapLast(PopulationFillMode fillMode)
        {
            return fillMode == PopulationFillMode.SNAP_LAST ||
                   fillMode == PopulationFillMode.FIT_USING_SCALE ||
                   fillMode == PopulationFillMode.FIT_USING_SPACING;
        }

        private void ClearPopulationSnap(Population population, SplineObject so)
        {
            SetPopulationSnap(so, SnapMode.NONE, 0, 0);

            // Parts of a previous last instance can still be snapped.
            if (!population.HasPrefabParts || so.transform.childCount == 0)
                return;

            splineObjectContainerSnap.Clear();
            so.gameObject.GetComponentsInChildren(splineObjectContainerSnap);

            for (int i = 0; i < splineObjectContainerSnap.Count; i++)
                SetPopulationSnap(splineObjectContainerSnap[i], SnapMode.NONE, 0, 0);
        }

        private void SetPopulationSnap(SplineObject so, SnapMode snapMode, float endSnapDistance, float snapTargetPoint)
        {
            SnapSettings snapSettings = so.SnapSettings;
            snapSettings.snapMode = snapMode;
            snapSettings.startSnapDistance = 0;
            snapSettings.endSnapDistance = endSnapDistance;
            snapSettings.startSnapOffset = 0;
            snapSettings.endSnapOffset = 0;
            snapSettings.snapTargetPoint = snapTargetPoint;
            so.SnapSettings = snapSettings;
        }
    }
}
