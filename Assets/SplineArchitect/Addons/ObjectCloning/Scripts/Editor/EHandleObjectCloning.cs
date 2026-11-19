// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: EHandleObjectCloning.cs
//
// Author: Mikael Danielsson
// Date Created: 21-04-2025
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------
using System.Collections.Generic;

using UnityEditor;
using Unity.Mathematics;
using UnityEngine;

using SplineArchitect.CustomTools;
using SplineArchitect.Libraries;
using SplineArchitect.Ui;
using SplineArchitect.Utility;

namespace SplineArchitect
{
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    public class EHandleObjectCloning
    {
        const string name = "Object Cloning";
        const string version = "2.0.4";
        const float minSectionLength = 0.05f;

        public static Texture2D textureClone { get; private set; }
        public static Texture2D textureCloneActive { get; private set; }
        public static GUIContent iconClone { get; private set; }
        public static GUIContent iconCloneActive { get; private set; }
        public static GUIContent sOPositionWarningText { get; private set; }
        public static GUIContent sOTypeWarningText { get; private set; }

        private static List<OriginClone> originCloneLayout = new List<OriginClone>();
        private static List<MeshFilter> meshFilterContainer = new List<MeshFilter>();
        private static List<Transform> transformContainer = new List<Transform>();
        private static List<MeshCollider> meshColliderContainer = new List<MeshCollider>();
        private static HashSet<SplineObject> markedSplineObjects = new HashSet<SplineObject>();
        private static Dictionary<OriginClone, int> originCloneCount = new Dictionary<OriginClone, int>();


        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AftertAssemblyReload()
        {
            //Events
            EHandleEvents.beforeFirstUpdate += OnFirstUpdate;
            EHandleEvents.afterSegmentRemoved += AfterSegmentRemoved;
            EHandleEvents.afterSplineSplit += AfterSplineSplit;
            EHandleEvents.afterSplineChange += AfterSplineChange;
            EHandleEvents.afterSegmentMovement += AfterSegmentMovement;
            EHandleEvents.afterSegmentCreated += AfterSegmentCreated;
            EHandleEvents.afterSplineObjectParentChanged += AfterSplineObjectParentChanged;
            EHandleEvents.afterSplineOnbjectSetPositionInUi += AfterSplineOnbjectSetPositionInUi;
            EHandleEvents.afterControlPointSetPositionInUi += AfterControlPointSetPositionInUi;
            EHandleEvents.afterPositionToolDrag += AfterPositionToolDrag;
            EHandleEvents.beforeWindowExtendedGUI += BeforeWindowExtendedGUI;
            SceneView.beforeSceneGui += BeforeSceneGUI;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            //Addon display name
            WindowInfo.DisplayAddonName($"{name} {version}");

            EHandleCloningUi.AfterAssemblyReload();
        }

        private static void OnFirstUpdate()
        {
            string mainFolderPath = EHandleFolder.GetMainFolderPath();

            //Deform terrain
            textureClone = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainFolderPath}/Addons/ObjectCloning/Textures/cloneIcon.png");
            textureCloneActive = AssetDatabase.LoadAssetAtPath<Texture2D>($"{mainFolderPath}/Addons/ObjectCloning/Textures/cloneIcon_active.png");
            iconClone = new GUIContent(textureClone, "Object Cloning");
            iconCloneActive = new GUIContent(textureCloneActive, "Object Cloning");
            sOPositionWarningText = new GUIContent(LibraryTexture.iconWarningMsg, "Position locked because this SplineObject is a clone. Disconnect it in the Object Cloning submenu to unlock the position fields.");
            sOTypeWarningText = new GUIContent(LibraryTexture.iconWarningMsg, "Type is locked because cloning is currently enabled for this SplineObject. Disconnect or delete all clones to unlock the type setting.");
            WindowExtended.AddSubMenuSplineObject("objectCloning", iconClone, iconCloneActive, ObjectCloningUi.DrawSplineObjectWindow, ObjectCloningUi.CalcSplineObjectWindowSize);
        }

        private static void BeforeSceneGUI(SceneView sceneView)
        {
            // Upddated marked spline objects
            if (EUiUtility.MouseLeftUpIncludingIgnored())
            {
                Spline selectedSpline = EHandleSelection.selectedSpline;
                if(selectedSpline != null) selectedSpline.RebuildCache();

                foreach(SplineObject so in markedSplineObjects)
                {
                    if (so == null)
                        return;

                    UpdateCloneAmount(so);
                }

                markedSplineObjects.Clear();
            }

            SplineObject selectedSo = EHandleSelection.selectedSplineObject;

            if(selectedSo == null)
                return;

            SplineObject cloneParent = GetCloneParent(selectedSo);

            if(cloneParent == null || cloneParent.currentClones == null || !cloneParent.currentClones.Contains(selectedSo))
                return;

            PositionTool.locked = true;
            PositionTool.lockedWarningMsg = "[Spline Architect] Position locked because this SplineObject is a clone. Disconnect it in the Object Cloning submenu to unlock the position fields.";
        }

        private static void BeforeWindowExtendedGUI(Event e)
        {
            // Upddated marked spline objects
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                foreach(SplineObject so in markedSplineObjects)
                {
                    if (so == null)
                        return;

                    UpdateCloneAmount(so);
                }

                markedSplineObjects.Clear();
            }

            SplineObject selectedSo = EHandleSelection.selectedSplineObject;

            if(selectedSo == null)
                return;

            SplineObject cloneParent = GetCloneParent(selectedSo);

            if(cloneParent == null || cloneParent.currentClones == null || !cloneParent.currentClones.Contains(selectedSo))
                return;

            WindowExtended.disableSOPosition = true;
            WindowExtended.sOPositionWarningText = sOPositionWarningText;
        }

        private static void AfterPositionToolDrag()
        {
            if(PositionTool.activationType == PositionTool.ActivateType.SPLINE_OBJECT)
            {
                Mark(EHandleSelection.selectedSplineObject);    

                foreach(SplineObject so2 in EHandleSelection.selectedSplineObjects)
                {
                    Mark(so2);
                }

                void Mark(SplineObject so)
                {
                    if (so == null)
                        return;

                    if (so.Type != SplineObjectType.DEFORMATION)
                        return;

                    if (so.cloningActive)
                    {
                        if(markedSplineObjects.Contains(so))
                            return;

                        markedSplineObjects.Add(so);
                    }
                }
            }
        }

        private static void AfterSplineOnbjectSetPositionInUi(EventType eventType, SplineObject so)
        {
            if (so == null)
                return;

            if (so.Type != SplineObjectType.DEFORMATION)
                return;

            if (so.cloningActive)
            {
                if(eventType == EventType.KeyDown)
                {
                    UpdateCloneAmount(so);
                }
                else
                {
                    if(markedSplineObjects.Contains(so))
                        return;

                    markedSplineObjects.Add(so);
                }
            }
        }

        private static void AfterControlPointSetPositionInUi(EventType eventType, Segment segment)
        {
            AfterSegmentMovement(segment, ControlHandle.ANCHOR);
        }

        private static void AfterSegmentMovement(Segment segment, ControlHandle controlHandle)
        {
            if (segment == null)
                return;

            Spline spline = segment.SplineParent;

            if (spline == null)
                return;

            for (int i = spline.AllSplineObjectCount - 1; i >= 0; i--)
            {
                SplineObject so = spline.GetSplineObjectAtIndex(i);

                if (so == null)
                    continue;

                if (so.Type != SplineObjectType.DEFORMATION)
                    continue;

                if (so.cloningActive)
                {
                    if(markedSplineObjects.Contains(so))
                        continue;

                    markedSplineObjects.Add(so);
                }
            }
        }

        private static void AfterSegmentCreated(Segment segment)
        {
            AfterSegmentMovement(segment, ControlHandle.ANCHOR);
        }

        private static void AfterSplineSplit(Spline spline, Spline newSpline)
        {
            AfterSplineChange(spline, SplineChange.CUSTOM);
        }

        private static void AfterSplineChange(Spline spline, SplineChange splineChange)
        {
            if (spline == null)
                return;

            for (int i = 0; i < spline.AllSplineObjectCount; i++)
            {
                SplineObject so = spline.GetSplineObjectAtIndex(i);

                if (so == null)
                    continue;

                if (so.Type != SplineObjectType.DEFORMATION)
                    continue;

                if (!so.cloningActive)
                    continue;

                UpdateCloneAmount(so);
            }
        }

        private static void AfterSegmentRemoved(Spline spline)
        {
            if (spline == null)
                return;

            bool updatedSplineCache = false;

            for (int i = spline.AllSplineObjectCount - 1; i >= 0; i--)
            {
                if (i >= spline.AllSplineObjectCount)
                    continue;

                SplineObject so = spline.GetSplineObjectAtIndex(i);

                if (so == null)
                    continue;

                if (so.Type != SplineObjectType.DEFORMATION)
                    continue;

                if (!so.cloningActive)
                    continue;

                if(!updatedSplineCache)
                {
                    updatedSplineCache = true;
                    spline.RebuildCache();
                    spline.MarkSplineObjectsDirty();
                }

                UpdateCloneAmount(so);
            }
        }

        private static void OnUndoRedoPerformed()
        {
            Spline selectedSpline = EHandleSelection.selectedSpline;

            if (selectedSpline != null)
            {
                UpdateSpline(selectedSpline);

                // Updadte links
                for(int i = 0; i < selectedSpline.SegmentCount; i++)
                {
                    Segment s = selectedSpline.GetSegmentAtIndex(i);
                    for(int i2 = 0; i2 < s.LinkCount; i2++)
                    {
                        Segment s2 = s.GetLinkAtIndex(i2);
                        if(s2.splineParent == selectedSpline) continue;
                        UpdateSpline(s2.splineParent);
                    }
                }
            }

            SplineObject selectedSo = EHandleSelection.selectedSplineObject;

            if (selectedSo != null && selectedSo.SplineParent != null && selectedSpline != selectedSo.SplineParent)
                UpdateSpline(selectedSo.SplineParent);

            void UpdateSpline(Spline spline)
            {
                for (int i = 0; i < spline.AllSplineObjectCount; i++)
                {
                    SplineObject so = spline.GetSplineObjectAtIndex(i);

                    if (so == null)
                        continue;

                    if (so.SplineParent == null)
                        continue;

                    if (so.currentClones == null || so.currentClones.Count == 0)
                        continue;

                    if (so.originClones == null || so.originClones.Count == 0)
                        continue;

                    if (so.IsCloneVersionDirty())
                        UpdateCloneAmount(so);

                    if (so.SplineParent == null)
                        continue;

                    so.SplineParent.directSystemWorker.Add(so);
                    foreach (SplineObject so2 in so.currentClones) ForceUpdate(so2);
                }

                if (spline.directSystemWorker.HasWork())
                    spline.directSystemWorker.Complete();
            }

            void ForceUpdate(SplineObject so)
            {
                if (so == null)
                    return;

                SplineObject[] soChilds = so.transform.GetComponentsInChildren<SplineObject>();
                foreach (SplineObject soChild in soChilds)
                {
                    if (soChild == null)
                        continue;

                    if(soChild.SplineParent == null)
                        continue;

                    if (soChild.Type == SplineObjectType.DEFORMATION)
                    {
                        // Ugly fix, but during redo the mesh can become invisible.
                        soChild.SyncMeshContainers();
                        soChild.SyncInstanceMeshesFromCache();
                        soChild.RenderMeshInternal(true);
                    }

                    soChild.SplineParent.directSystemWorker.Add(soChild);
                }
            }
        }

        private static void AfterSplineObjectParentChanged(SplineObject so)
        {
            if (so == null)
                return;

            if (so.Type != SplineObjectType.DEFORMATION)
                return;

            if (!so.cloningActive)
                return;

            EActionDelayed.Add(() => {
                UpdateCloneAmount(so);
                EHandleDeformation.ProcessSplineObjects(so.SplineParent);
            }, 0, 0, EActionDelayed.ActionFlag.FRAMES | EActionDelayed.ActionFlag.LATE);
        }

        internal static void UpdateCloneAmount(SplineObject cloneParent)
        {
            if (!cloneParent.cloningActive)
                return;

            int newAmount = GetCloneAmount(cloneParent, cloneParent.originClones, originCloneLayout);
            int dif = newAmount - cloneParent.currentClones.Count;

            if (dif > 0)
            {
                CreateClones(cloneParent, dif, originCloneLayout);
            }
            else if (dif < 0)
            {
                DeleteClones(cloneParent, Mathf.Abs(dif));
            }

            FittClones(cloneParent, originCloneLayout);
            SetAdditionalSettings(cloneParent);
        }

        internal static void SetAdditionalSettings(SplineObject cloneParent)
        {
            originCloneLayout.Clear();
            foreach(OriginClone oc in cloneParent.originClones)
            {
                if(oc.prefab != null) originCloneLayout.Add(oc);
            }

            originCloneCount.Clear();
            for (int i = 0; i < cloneParent.currentClones.Count; i++)
            {
                SplineObject clone = cloneParent.currentClones[i];

                if(clone == null)
                    continue;

                OriginClone originClone = originCloneLayout[i % originCloneLayout.Count];

                if(!originCloneCount.ContainsKey(originClone)) originCloneCount.Add(originClone, 0);
                else originCloneCount[originClone]++;

                EHandleUndo.RecordNow(clone);
                
                // Uv related
                if(clone.MeshContainerCount > 0 && originClone.deform)
                {
                    MeshContainer mc = clone.GetMeshContainerAtIndex(0);

                    // UV
                    Vector2 oldUvOffset = mc.GetUvOffsetAtIndex(0);
                    if(originClone.incrementUvX) mc.SetUvOffsetAtIndex(0, new Vector2(originCloneCount[originClone], oldUvOffset.y));
                    if(originClone.incrementUvY) mc.SetUvOffsetAtIndex(0, new Vector2(oldUvOffset.x, originCloneCount[originClone]));

                    // UV2
                    oldUvOffset = mc.GetUvOffsetAtIndex(1);
                    if(originClone.incrementUv2X) mc.SetUvOffsetAtIndex(1, new Vector2(originCloneCount[originClone], oldUvOffset.y));
                    if(originClone.incrementUv2Y) mc.SetUvOffsetAtIndex(1, new Vector2(oldUvOffset.x, originCloneCount[originClone]));

                    // UV3
                    oldUvOffset = mc.GetUvOffsetAtIndex(2);
                    if(originClone.incrementUv3X) mc.SetUvOffsetAtIndex(2, new Vector2(originCloneCount[originClone], oldUvOffset.y));
                    if(originClone.incrementUv3Y) mc.SetUvOffsetAtIndex(2, new Vector2(oldUvOffset.x, originCloneCount[originClone]));

                    // UV4
                    oldUvOffset = mc.GetUvOffsetAtIndex(3);
                    if(originClone.incrementUv4X) mc.SetUvOffsetAtIndex(3, new Vector2(originCloneCount[originClone], oldUvOffset.y));
                    if(originClone.incrementUv4Y) mc.SetUvOffsetAtIndex(3, new Vector2(oldUvOffset.x, originCloneCount[originClone]));
                }
            }
        }

        private static void CreateClones(SplineObject cloneParent, int amount, List<OriginClone> originCloneLayout)
        {
            //Amount check menu
            if (amount > 999)
            {
                bool clicksYes = EditorUtility.DisplayDialog("Spline Architect Warning",
                                                            $"You are about to clone {amount} GameObject(s). Do you want to continue?",
                                                            "Yes",
                                                            "No"
                );

                if (!clicksYes)
                {
                    DeleteClones(cloneParent, cloneParent.currentClones.Count);
                    cloneParent.cloningActive = false;
                    return;
                }
            }

            int totalAmount = cloneParent.currentClones.Count + amount;
            int initialCloneCount = cloneParent.currentClones.Count;

            int cloneIndex = 0;

            for (int i = 0; i < originCloneLayout.Count; i++)
            {
                OriginClone originClone = originCloneLayout[i];

                if(originClone == null || originClone.prefab == null || originClone.prefab == cloneParent.gameObject)
                    continue;

                if (cloneIndex < initialCloneCount)
                {
                    cloneIndex++;
                    continue;
                }

                if (cloneIndex >= totalAmount)
                    break;

                //Create clone
                cloneParent.editorDisableOnChildrenChanged = true;
                GameObject goClone = Object.Instantiate(originClone.prefab, cloneParent.transform);
                cloneParent.editorDisableOnChildrenChanged = false;
                
                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originClone.prefab);
                GameObject prefab = string.IsNullOrEmpty(prefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if(prefab == null)
                    prefab = PrefabUtility.GetCorrespondingObjectFromSource(originClone.prefab);

                if (prefab != null && cloneParent.keepPrefabLink)
                {
                    ConvertToPrefabInstanceSettings settings = new();
                    settings.componentsNotMatchedBecomesOverride = true;
                    settings.objectMatchMode = ObjectMatchMode.ByHierarchy;
                    settings.recordPropertyOverridesOfMatches = true;
                    settings.logInfo = false;
                    settings.changeRootNameToAssetName = false;
                    PrefabUtility.ConvertToPrefabInstance(goClone, prefab, settings, InteractionMode.AutomatedAction);
                }

                goClone.name = $"{goClone.name} {cloneIndex}";
                EHandleUndo.RegisterCreatedObject(goClone, "Created clones");

                SplineObject soClone = AddSplineObject(goClone, false, originClone.deform);

                EHandleUndo.RecordNow(soClone);
                if (!originClone.deform) soClone.Type = SplineObjectType.FOLLOWER;
                soClone.transform.localScale = originClone.prefab.transform.localScale;
#if UNITY_2022
                EHandleUndo.RecordNow(cloneParent, null, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
#else
                EHandleUndo.RecordNow(cloneParent);
#endif
                cloneParent.currentClones.Add(soClone);
                cloneIndex++;

                if(soClone.Type == SplineObjectType.DEFORMATION)
                {
                    transformContainer.Clear();
                    goClone.transform.GetComponentsInChildren(transformContainer);
                    foreach (Transform child in transformContainer)
                    {
                        if(child == goClone.transform)
                            continue;

                        AddSplineObject(child.gameObject, true, true);
                    }
                }
            }

            EHandleUndo.RecordNow(cloneParent);
            cloneParent.cloneAmount = totalAmount;

            SplineObject AddSplineObject(GameObject go, bool isChild, bool deform)
            {
                SplineObject so = go.GetComponent<SplineObject>();
                if(so == null) so = EHandleUndo.AddComponent<SplineObject>(go);
                else so.SyncParentData();

                EHandleUndo.RecordNow(so);
                if (deform) so.Type = SplineObjectType.DEFORMATION;
                else so.Type = SplineObjectType.FOLLOWER;

                SplineObject source = null;
                Mesh originMesh = GetOriginMesh(so);
                for (int i = 0; i < cloneParent.currentClones.Count; i++)
                {
                    SplineObject candidate = cloneParent.currentClones[i];
                    if (candidate == null || candidate == so)
                        continue;

                    // Keep the first clone as the fallback until a mesh/type match is found.
                    if (source == null) source = candidate;
                    if (originMesh != null && candidate.Type == so.Type && GetOriginMesh(candidate) == originMesh)
                    {
                        source = candidate;
                        break;
                    }
                }

                if (source != null) 
                    CopySplineObjectSettings(source, so);

                if(isChild)
                {
                    so.localSplinePosition = so.transform.localPosition;
                    so.localSplineRotation = so.transform.localRotation;
                }

                return so;

                Mesh GetOriginMesh(SplineObject splineObject)
                {
                    if (splineObject.Type == SplineObjectType.FOLLOWER)
                    {
                        MeshFilter meshFilter = splineObject.GetComponent<MeshFilter>();
                        if (meshFilter == null) meshFilter = splineObject.GetComponentInChildren<MeshFilter>(true);
                        return meshFilter != null ? meshFilter.sharedMesh : null;
                    }

                    return splineObject.MeshContainerCount > 0 ? splineObject.GetMeshContainerAtIndex(0).GetOriginMesh() : null;
                }
            }

            void CopySplineObjectSettings(SplineObject source, SplineObject target)
            {
                target.AlignToEnd = source.AlignToEnd;
                target.SkipTangents = source.SkipTangents;
                target.ProjectToSurface = source.ProjectToSurface;
                target.MirrorDeformation = source.MirrorDeformation;
                target.NormalType = source.NormalType;
                target.SnapSettings = source.SnapSettings;
                target.LockPosition = source.LockPosition;
                target.ProjectToSurfaceMask = source.ProjectToSurfaceMask;
                target.componentMode = source.componentMode;
                target.meshMode = source.meshMode;

                if (target.Type == SplineObjectType.FOLLOWER) target.RotationMode = source.RotationMode;

                for (int i = 0; i < target.MeshContainerCount && i < source.MeshContainerCount; i++)
                {
                    MeshContainer targetMesh = target.GetMeshContainerAtIndex(i);
                    MeshContainer sourceMesh = source.GetMeshContainerAtIndex(i);

                    targetMesh.Resolution = sourceMesh.Resolution;
                    targetMesh.ResolutionMode = sourceMesh.ResolutionMode;
                    targetMesh.AutoResolution = sourceMesh.AutoResolution;
                    targetMesh.AutoBias = sourceMesh.AutoBias;
                    targetMesh.MinAutoResolution = sourceMesh.MinAutoResolution;
                    targetMesh.MaxAutoResolution = sourceMesh.MaxAutoResolution;

                    for (int uv = 0; uv < 4; uv++)
                        targetMesh.SetUvOffsetAtIndex(uv, sourceMesh.GetUvOffsetAtIndex(uv));
                }
            }
        }

        private static void DeleteClones(SplineObject cloneParent, int amountToDelete)
        {
            if (cloneParent.currentClones == null)
                return;

            if(amountToDelete == 0)
                return;

            EHandleUndo.RecordNow(cloneParent, "Deleted clones", EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
            if(!cloneParent.cloneUseFixedAmount) cloneParent.cloneAmount = cloneParent.currentClones.Count - amountToDelete;
            for (int i = 0; i < amountToDelete; i++)
            {
                int index = cloneParent.currentClones.Count - 1;
                SplineObject soChild = cloneParent.currentClones[index];

                cloneParent.currentClones.RemoveAt(index);

                if(soChild != null)
                    EHandleUndo.DestroyObjectImmediate(soChild.gameObject);
            }
        }

        private static void FittClones(SplineObject cloneParent, List<OriginClone> originCloneLayout)
        {
            if (cloneParent.currentClones == null || cloneParent.currentClones.Count == 0 || originCloneLayout == null || originCloneLayout.Count == 0)
                return;

            float availableLength = Mathf.Max(0, cloneParent.SplineParent.Length - cloneParent.splinePosition.z);
            float rangeStart = cloneParent.cloneStartPadding;
            float rangeEnd = cloneParent.cloneEndPadding;
            float rangeLength = Mathf.Max(0, availableLength - (rangeEnd + rangeStart));
            double totalPrefabLength = 0;
            double totalFixedLength = 0;
            bool scaleToFit = cloneParent.cloneDistributionMode == CloneDistributionMode.FIT_USING_SCALE && !cloneParent.cloneUseFixedAmount;
            bool spacingToFit = cloneParent.cloneDistributionMode == CloneDistributionMode.FIT_USING_SPACING && !cloneParent.cloneUseFixedAmount;
            int prefabCount = 0;
            int firstPrefabIndex = -1;
            int lastPrefabIndex = -1;
            double leadingPositionOffsetZ = 0;

            double accumulatedPositionOffsetZ = 0;
            double finalPositionOffsetZ = 0;
            for (int i = 0; i < originCloneLayout.Count; i++)
            {
                OriginClone originClone = originCloneLayout[i];

                if (originClone == null)
                    continue;

                if (originClone.prefab != null)
                {
                    if (prefabCount == 0)
                    {
                        firstPrefabIndex = i;
                        leadingPositionOffsetZ = accumulatedPositionOffsetZ;
                    }
                    lastPrefabIndex = i;
                    prefabCount++;
                    totalPrefabLength += GetSectionLength(originClone);

                    if (scaleToFit)
                        totalFixedLength += GetSectionLength(originClone, 0);

                    finalPositionOffsetZ = accumulatedPositionOffsetZ;
                }

                accumulatedPositionOffsetZ += ClampPositionOffsetZ(originClone, originClone.positionOffset.z, GetSectionLength(originClone));
            }

            if (totalPrefabLength <= 0)
                return;

            float fitScale = 1;
            float spacingScale = 1;
            float extraSpacing = 0;

            if (scaleToFit)
            {
                // Keep the configured gaps exact and fit only the object lengths.
                double scalableLength = totalPrefabLength - totalFixedLength;

                if (scalableLength > 0)
                {
                    double availableScalableLength = rangeLength - totalFixedLength - finalPositionOffsetZ;
                    fitScale = (float)System.Math.Max(0, availableScalableLength / scalableLength);
                }
            }
            else if (spacingToFit && prefabCount > 1)
            {
                // Preserve relative gap sizes, including empty spacer entries between prefabs.
                double totalSpacing = 0;
                for (int i = firstPrefabIndex; i < lastPrefabIndex; i++)
                {
                    OriginClone originClone = originCloneLayout[i];
                    if (originClone != null)
                        totalSpacing += Mathf.Max(0, originClone.positionOffset.z);
                }

                double availableSpacing = System.Math.Max(0, rangeLength - totalPrefabLength - leadingPositionOffsetZ);
                if (totalSpacing > 0)
                    spacingScale = (float)(availableSpacing / totalSpacing);
                else
                    extraSpacing = (float)(availableSpacing / (prefabCount - 1));
            }

            double zPosition = rangeStart;
            accumulatedPositionOffsetZ = 0;
            Vector3 correctedCursorPosition = Vector3.zero;
            bool hasCorrectedCursorPosition = false;

            int cloneIndex = 0;
            for (int i = 0; i < originCloneLayout.Count; i++)
            {
                OriginClone originClone = originCloneLayout[i];

                if(originClone == null)
                    continue;

                float positionOffsetZ = GetPositionOffsetZ(originClone, i);

                if (originClone.prefab == null)
                {
                    if (cloneParent.cloneDistributionMode == CloneDistributionMode.CORRECTED_SPACING && hasCorrectedCursorPosition && !GeneralUtility.IsZero(positionOffsetZ))
                    {
                        float correctedSpacingEndZ = GetCorrectedZPosition(cloneParent.SplineParent, correctedCursorPosition, positionOffsetZ, 25);
                        accumulatedPositionOffsetZ += correctedSpacingEndZ - correctedCursorPosition.z;
                        correctedCursorPosition.z = correctedSpacingEndZ;
                    }
                    else
                    {
                        accumulatedPositionOffsetZ += positionOffsetZ;
                    }

                    continue;
                }

                SplineObject clone = cloneIndex < cloneParent.currentClones.Count ? cloneParent.currentClones[cloneIndex] : null;
                cloneIndex++;

                if(clone != null)
                {
                    EHandleUndo.RecordNow(clone);
                    EHandleUndo.RecordNow(clone.transform);
                }

                Bounds prefabBounds = GetSectionBounds(originClone, fitScale);
                positionOffsetZ = ClampPositionOffsetZ(originClone, positionOffsetZ, prefabBounds.size.z);
                Vector3 cloneScale = Vector3.Scale(originClone.prefab.transform.localScale, originClone.scale);
                Quaternion cloneRotation = Quaternion.Euler(originClone.rotation);
                SplineObject prefabSplineObject = originClone.prefab.GetComponent<SplineObject>();

                if (prefabSplineObject != null)
                    cloneRotation = prefabSplineObject.localSplineRotation * cloneRotation;

                if(clone != null)
                {
                    clone.transform.localScale = Vector3.Scale(cloneScale, GetFitScaleMultiplier(cloneRotation, fitScale));
                    clone.localSplineRotation = cloneRotation;
                }

                if(clone != null)
                {
                    Vector3 splinePosition = Vector3.zero;
                    splinePosition.x += originClone.positionOffset.x;
                    splinePosition.y += originClone.positionOffset.y;
                    splinePosition.z = (float)(zPosition - prefabBounds.min.z + accumulatedPositionOffsetZ);
                    clone.localSplinePosition = splinePosition;

                    if (cloneParent.cloneDistributionMode == CloneDistributionMode.CORRECTED_SPACING)
                    {
                        Vector3 boundsStartPosition = clone.splinePosition;
                        boundsStartPosition.z += prefabBounds.min.z;

                        float correctedEndZ = GetCorrectedZPosition(cloneParent.SplineParent, boundsStartPosition, prefabBounds.size.z, 25);

                        zPosition += correctedEndZ - boundsStartPosition.z;

                        correctedCursorPosition = boundsStartPosition;
                        correctedCursorPosition.z = correctedEndZ;
                        hasCorrectedCursorPosition = true;

                        if (!GeneralUtility.IsZero(positionOffsetZ))
                        {
                            float correctedSpacingEndZ = GetCorrectedZPosition(cloneParent.SplineParent, correctedCursorPosition, positionOffsetZ, 25);
                            accumulatedPositionOffsetZ += correctedSpacingEndZ - correctedCursorPosition.z;
                            correctedCursorPosition.z = correctedSpacingEndZ;
                        }
                    }
                    else
                    {
                        zPosition += prefabBounds.size.z;
                        accumulatedPositionOffsetZ += positionOffsetZ;
                    }
                }
                else
                {
                    zPosition += prefabBounds.size.z;
                    accumulatedPositionOffsetZ += positionOffsetZ;

                    hasCorrectedCursorPosition = false;
                }
            }

            // Randomization pass
            System.Random transformRandom = new System.Random(cloneParent.cloneRandomizeSeed);
            for (int i = 0; i < cloneParent.currentClones.Count; i++)
            {
                SplineObject clone = cloneParent.currentClones[i];

                Vector3 randomizedPosition = GetRandomVector(transformRandom, cloneParent.randomizedPositonFrom, cloneParent.randomizedPositonTo, cloneParent.randomizationStrength);
                Vector3 randomizedEuler = GetRandomVector(transformRandom, cloneParent.randomizedEulerFrom, cloneParent.randomizedEulerTo, cloneParent.randomizationStrength);
                Vector3 randomizedScale = GetRandomVector(transformRandom, cloneParent.randomizedXYZScaleFrom, cloneParent.randomizedXYZScaleTo, cloneParent.randomizationStrength);
                float randomizedScaleConstrained = Mathf.Lerp(cloneParent.randomizedScaleFrom, cloneParent.randomizedScaleTo, (float)transformRandom.NextDouble()) * cloneParent.randomizationStrength;

                if (clone != null)
                {
                    clone.localSplinePosition += randomizedPosition;
                    clone.localSplineRotation *= Quaternion.Euler(randomizedEuler);
                    if(cloneParent.constrainedScale)
                        clone.transform.localScale += new Vector3(randomizedScaleConstrained, randomizedScaleConstrained, randomizedScaleConstrained);
                    else
                        clone.transform.localScale += randomizedScale;

                    SnapSettings snapSettings = clone.SnapSettings;
                    snapSettings.startSnapDistance = 0;
                    snapSettings.endSnapDistance = 0;
                    snapSettings.snapMode = SnapMode.NONE;
                    if (scaleToFit && cloneParent.cloneEndPadding == 0f && i == cloneParent.currentClones.Count - 1)
                    {
                        // Close residual seams at intersections when no end gap is requested.
                        float endSnapDistance = 0.1f;
                        if (clone.MeshContainerCount > 0)
                        {
                            Mesh originMesh = clone.GetMeshContainerAtIndex(0).GetOriginMesh();
                            if (originMesh != null)
                                endSnapDistance = originMesh.bounds.extents.z;
                        }

                        snapSettings.endSnapDistance = endSnapDistance;
                        snapSettings.startSnapOffset = 0;
                        snapSettings.endSnapOffset = 0;
                        snapSettings.snapMode = SnapMode.CONTROL_POINT_END;
                    }
                    clone.SnapSettings = snapSettings;
                }
            }

            float mirrorChance = Mathf.Clamp01(cloneParent.mirrorChance) * cloneParent.randomizationStrength;
            float rotation180Chance = Mathf.Clamp01(cloneParent.rotation180Chance) * cloneParent.randomizationStrength;

            for (int i = 0; i < cloneParent.currentClones.Count; i++)
            {
                SplineObject clone = cloneParent.currentClones[i];
                bool mirror = transformRandom.NextDouble() < mirrorChance;
                bool mirrorXAxis = transformRandom.NextDouble() < 0.5;
                bool rotate180 = transformRandom.NextDouble() < rotation180Chance;

                if(clone != null)
                {
                    if(mirror)
                    {
                        Vector3 scale = clone.transform.localScale;

                        if(mirrorXAxis) scale.x *= -1;
                        else scale.z *= -1;

                        clone.transform.localScale = scale;
                    }

                    if(rotate180)
                        clone.localSplineRotation *= Quaternion.Euler(0, 180, 0);
                }
            }

            float GetPositionOffsetZ(OriginClone originClone, int layoutIndex)
            {
                float offset = originClone.positionOffset.z;
                if (spacingToFit && layoutIndex >= firstPrefabIndex && layoutIndex < lastPrefabIndex)
                {
                    offset = Mathf.Max(0, offset) * spacingScale;
                    if (originClone.prefab != null)
                        offset += extraSpacing;
                }

                return offset;
            }

            Vector3 GetRandomVector(System.Random random, Vector3 from, Vector3 to, float strength)
            {
                return new Vector3(Mathf.Lerp(from.x, to.x, (float)random.NextDouble()),
                                   Mathf.Lerp(from.y, to.y, (float)random.NextDouble()),
                                   Mathf.Lerp(from.z, to.z, (float)random.NextDouble())) * strength;
            }

            float GetCorrectedZPosition(Spline spline, Vector3 splinePosition, float length, int resolution)
            {
                if (resolution <= 0)
                    throw new System.ArgumentOutOfRangeException(nameof(resolution));

                float targetLength = Mathf.Abs(length);
                float direction = Mathf.Sign(length);
                float stepSize = targetLength / resolution * direction;
                int iterations = resolution * 5;

                float worldLength = 0;
                Vector3 sp = splinePosition;
                Vector3 a = spline.SplinePositionToWorldPosition(sp);

                for (int i = 0; i < iterations; i++)
                {
                    float previousZ = sp.z;

                    sp.z += stepSize;

                    Vector3 b = spline.SplinePositionToWorldPosition(sp);
                    float stepWorldLength = Vector3.Distance(a, b);

                    if (worldLength + stepWorldLength >= targetLength)
                    {
                        float remainingLength = targetLength - worldLength;

                        if (stepWorldLength > 0.0001f)
                        {
                            float t = remainingLength / stepWorldLength;
                            return Mathf.Lerp(previousZ, sp.z, t);
                        }

                        return previousZ;
                    }

                    worldLength += stepWorldLength;
                    a = b;
                }

                return sp.z;
            }
        }

        private static int GetCloneAmount(SplineObject cloneParent, List<OriginClone> originClones, List<OriginClone> layout)
        {
            layout.Clear();

            if (cloneParent == null || cloneParent.SplineParent == null || originClones == null || originClones.Count == 0)
                return 0;

            bool hasPrefab = false;
            double totalProbability = 0;

            foreach(OriginClone oc in originClones)
            {
                if(oc == null)
                    continue;

                if(oc.prefab != null && oc.prefab != cloneParent.gameObject)
                {
                    hasPrefab = true;
                }

                if(cloneParent.randomizePrefabClones && oc.probability > 0)
                {
                    totalProbability += Mathf.Clamp01(oc.probability);
                }
            }

            if(!hasPrefab || (cloneParent.randomizePrefabClones && GeneralUtility.IsZero(totalProbability, 0.0001f)))
                return 0;

            int amount = 0;
            System.Random random = new System.Random(cloneParent.cloneRandomizeSeed);

            if (cloneParent.cloneUseFixedAmount)
            {
                int fixedAmount = Mathf.Max(0, cloneParent.cloneAmount);

                while(amount < fixedAmount && layout.Count < 25000)
                {
                    OriginClone oc = GetNextOriginClone(layout.Count);
                    layout.Add(oc);

                    if(oc != null && oc.prefab != null)
                        amount++;
                }

                return amount;
            }

            float availableLength = Mathf.Max(0, cloneParent.SplineParent.Length - cloneParent.splinePosition.z);
            float rangeStart = cloneParent.cloneStartPadding;
            float rangeEnd = cloneParent.cloneEndPadding;
            float cloneLength = Mathf.Max(0, availableLength - (rangeEnd + rangeStart));
            float calcLength = 0;
            double totalPrefabLength = 0;
            double leadingPositionOffsetZ = 0;
            OriginClone originClone = GetNextOriginClone(0);

            while (cloneLength > calcLength && layout.Count < 25000)
            {
                float transformedSectionLength = GetSectionLength(originClone);

                // Spacing fitting keeps base scales, so do not create a section that cannot fit even with zero gaps.
                if (cloneParent.cloneDistributionMode == CloneDistributionMode.FIT_USING_SPACING &&
                    originClone != null && originClone.prefab != null &&
                    totalPrefabLength + transformedSectionLength + leadingPositionOffsetZ > cloneLength)
                    break;

                totalPrefabLength += transformedSectionLength;
                calcLength += transformedSectionLength;
                layout.Add(originClone);

                if(originClone != null && originClone.prefab != null)
                    amount++;

                OriginClone nextOriginClone = GetNextOriginClone(layout.Count);
                float nextTransformedSectionLength = GetSectionLength(nextOriginClone);
                float positionOffsetZ = originClone != null ? originClone.positionOffset.z : 0;
                positionOffsetZ = ClampPositionOffsetZ(originClone, positionOffsetZ, transformedSectionLength);

                if (calcLength + positionOffsetZ + nextTransformedSectionLength * 0.5f > cloneLength)
                    break;

                if (amount == 0)
                    leadingPositionOffsetZ += positionOffsetZ;

                calcLength += positionOffsetZ;
                originClone = nextOriginClone;
            }

            return amount;

            OriginClone GetNextOriginClone(int index)
            {
                if(cloneParent.randomizePrefabClones)
                {
                    if(totalProbability > 0)
                    {
                        double probability = random.NextDouble() * totalProbability;
                        double cumulativeProbability = 0;

                        foreach(OriginClone oc in originClones)
                        {
                            if(oc == null || oc.probability <= 0)
                                continue;

                            cumulativeProbability += Mathf.Clamp01(oc.probability);
                            if(probability < cumulativeProbability)
                                return oc;
                        }
                    }

                    int prefabIndex = random.Next(0, originClones.Count);
                    return originClones[prefabIndex];
                }

                return originClones[index % originClones.Count];
            }
        }

        // A prefab section plus its z offset can never take up less than minSectionLength along the spline.
        private static float ClampPositionOffsetZ(OriginClone originClone, float positionOffsetZ, float sectionLength)
        {
            if(originClone == null || originClone.prefab == null)
                return positionOffsetZ;

            return Mathf.Max(positionOffsetZ, minSectionLength - sectionLength);
        }

        private static float GetSectionLength(OriginClone originClone, float fitScaleMultiplier = 1)
        {
            if(originClone == null || originClone.prefab == null)
                return 0;

            float sectionLength = GetSectionBounds(originClone, fitScaleMultiplier).size.z;
            return sectionLength;
        }

        private static Bounds GetSectionBounds(OriginClone originClone, float fitScaleMultiplier = 1)
        {
            if(originClone == null || originClone.prefab == null)
                return new Bounds(Vector3.zero, Vector3.forward);

            Bounds bounds = new Bounds();
            bool boundsSet = false;
            float4x4 prefabWorldToLocal = (float4x4)originClone.prefab.transform.worldToLocalMatrix;
            Vector3 cloneScale = Vector3.Scale(originClone.prefab.transform.localScale, originClone.scale);
            Quaternion cloneRotation = Quaternion.Euler(originClone.rotation);
            SplineObject prefabSplineObject = originClone.prefab.GetComponent<SplineObject>();

            if (prefabSplineObject != null)
                cloneRotation = prefabSplineObject.localSplineRotation * cloneRotation;

            cloneScale = Vector3.Scale(cloneScale, GetFitScaleMultiplier(cloneRotation, fitScaleMultiplier));

            float4x4 originTransform = float4x4.TRS(float3.zero, cloneRotation, cloneScale);

            meshFilterContainer.Clear();
            originClone.prefab.GetComponentsInChildren(meshFilterContainer);

            foreach(MeshFilter mf in meshFilterContainer)
            {
                if (mf == null || mf.sharedMesh == null)
                    continue;

                float4x4 meshToPrefab = math.mul(prefabWorldToLocal, (float4x4)mf.transform.localToWorldMatrix);
                float4x4 meshToClone = math.mul(originTransform, meshToPrefab);

                Mesh originMesh = ESplineObjectUtility.GetOriginMeshFromMeshNameId(mf.sharedMesh);
                Mesh boundsMesh = originMesh != null ? originMesh : mf.sharedMesh;
                Bounds meshBounds = GeneralUtility.TransformBounds(boundsMesh.bounds, meshToClone);

                if (!boundsSet)
                {
                    boundsSet = true;
                    bounds = meshBounds;
                }
                else
                {
                    bounds.Encapsulate(meshBounds);
                }
            }

            meshColliderContainer.Clear();
            originClone.prefab.GetComponentsInChildren(meshColliderContainer);

            foreach (MeshCollider mc in meshColliderContainer)
            {
                if (mc == null || mc.sharedMesh == null)
                    continue;

                float4x4 meshToPrefab = math.mul(prefabWorldToLocal, (float4x4)mc.transform.localToWorldMatrix);
                float4x4 meshToClone = math.mul(originTransform, meshToPrefab);

                Mesh originMesh = ESplineObjectUtility.GetOriginMeshFromMeshNameId(mc.sharedMesh);
                Mesh boundsMesh = originMesh != null ? originMesh : mc.sharedMesh;
                Bounds meshBounds = GeneralUtility.TransformBounds(boundsMesh.bounds, meshToClone);

                if (!boundsSet)
                {
                    boundsSet = true;
                    bounds = meshBounds;
                }
                else
                {
                    bounds.Encapsulate(meshBounds);
                }
            }

            if (!boundsSet)
                return new Bounds(Vector3.zero, Vector3.forward);

            if (bounds.size.z < 0.025f && !GeneralUtility.IsZero(fitScaleMultiplier))
                bounds.size = new Vector3(bounds.size.x, bounds.size.y, 1);

            return bounds;
        }

        private static Vector3 GetFitScaleMultiplier(Quaternion rotation, float fitScale)
        {
            Vector3 splineForward = Quaternion.Inverse(rotation) * Vector3.forward;
            Vector3 scaleMultiplier = Vector3.one;

            if (!GeneralUtility.IsZero(splineForward.x)) scaleMultiplier.x = fitScale;
            if (!GeneralUtility.IsZero(splineForward.y)) scaleMultiplier.y = fitScale;
            if (!GeneralUtility.IsZero(splineForward.z)) scaleMultiplier.z = fitScale;

            return scaleMultiplier;
        }

        internal static SplineObject GetCloneParent(SplineObject so)
        {
            SplineObject parent = so.SoParent;

            if (parent == null)
                return null;

            for (int i = 0; i < 25; i++)
            {
                if (parent == null)
                    break;

                if (parent.cloningActive && parent.currentClones != null)
                {
                    return parent;
                }

                parent = parent.SoParent;
            }

            return null;
        }

        internal static void EnableUsingPrefabs(SplineObject cloneParent)
        {
            if (cloneParent.currentClones == null)
                cloneParent.currentClones = new List<SplineObject>();

            cloneParent.currentClones.Clear();

            EHandleUndo.RecordNow(cloneParent);
            cloneParent.cloningActive = true;
            if(!GeneralUtility.IsEqual(cloneParent.transform.localScale, Vector3.one))
            {
                Debug.LogWarning($"[Spline Architect] Clone parent '{cloneParent.name}' did not have a scale of (1,1,1). Scale has been reset to (1,1,1).");
                cloneParent.transform.localScale = Vector3.one;
            }

            UpdateCloneAmount(cloneParent);
        }

        internal static void Disable(SplineObject cloneParent)
        {
            DeleteClones(cloneParent, cloneParent.currentClones.Count);
            EHandleUndo.RecordNow(cloneParent);
            cloneParent.cloningActive = false;
        }

        internal static void DisconnectClonesAndDisable(SplineObject cloneParent)
        {
            EHandleUndo.RecordNow(cloneParent);
            if(cloneParent.currentClones != null)
                cloneParent.currentClones.Clear();

            cloneParent.cloningActive = false;
        }
    }
}
