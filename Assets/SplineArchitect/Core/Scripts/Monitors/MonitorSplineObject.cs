// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: MonitorSplineObject.cs
//
// Author: Mikael Danielsson
// Date Created: 30-01-2023
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;

using SplineArchitect.Utility;

namespace SplineArchitect.Monitor
{
    internal class MonitorSplineObject
    {
        internal const int dataUsage = 32 + 8 + 12 + 12 + 16 + 12 + 12 + 12 + 1 + 1;

        private SplineObject so;
        private Vector3 localScale;
        private Vector3 localSplinePosition;
        private Vector3 position;
        private Quaternion rotation;
        private Quaternion localSplineRotation;
        private Vector3 combinedParentPositionInSplineSpace;
        private Quaternion combinedParentRotationInSplineSpace;
        private Vector3 combinedParentLocalScale;
        private bool mirrored;
        private NormalType normalType;
        private SplineObjectType type;
        private UvSettings[] uvSettingPerLongest;
        private UvSettings[] uvSettingPerLongestZ;
        private UvSettings[] uvSettingPerSubdivide;

        internal MonitorSplineObject(SplineObject so)
        {
            this.so = so;
            UpdatePosRotSplineSpace();
            UpdateCombinedParentPosRotScaleChange();
            UpdateTransform();
            UpdateUvSettings();
            mirrored = so.MirrorDeformation;
            normalType = so.NormalType;
            type = so.Type;
            
#if UNITY_EDITOR
            componentCount = so.gameObject.GetComponentCount();
            childCount = so.transform.childCount;
            isStatic = so.gameObject.isStatic;
#endif
        }

        internal bool PosRotSplineSpaceChange()
        {
            bool foundChange = false;
            if (PosSplineSpaceChange()) foundChange = true;
            if (RotSplineSpaceChange()) foundChange = true;

            return foundChange;

            bool PosSplineSpaceChange()
            {
                bool foundChange = false;
                if (!GeneralUtility.IsEqualFast(localSplinePosition, so.localSplinePosition)) foundChange = true;

                localSplinePosition = so.localSplinePosition;

                return foundChange;
            }

            bool RotSplineSpaceChange()
            {
                bool foundChange = false;
                if (!GeneralUtility.IsEqualFast(localSplineRotation, so.localSplineRotation)) foundChange = true;

                localSplineRotation = so.localSplineRotation;

                return foundChange;
            }
        }

        internal bool CombinedParentPosRotScaleChange()
        {
            bool foundChange = false;
            SplineObject currAco = so;

            Vector3 pos = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.zero;

            for (int i = 0; i < 25; i++)
            {
                currAco = currAco.SoParent;
                if (currAco == null)
                    break;

                pos += currAco.localSplinePosition;
                rotation = rotation * currAco.localSplineRotation;
                scale += currAco.transform.localScale;
            }

            if (!GeneralUtility.IsEqualFast(pos, combinedParentPositionInSplineSpace)) foundChange = true;
            if (!GeneralUtility.IsEqualFast(rotation, combinedParentRotationInSplineSpace)) foundChange = true;
            if (!GeneralUtility.IsEqualFast(scale, combinedParentLocalScale)) foundChange = true;

            combinedParentPositionInSplineSpace = pos;
            combinedParentRotationInSplineSpace = rotation;
            combinedParentLocalScale = scale;

            return foundChange;
        }

        internal bool MirrorChange()
        {
            bool foundChange = false;
            if (so.MirrorDeformation != mirrored)
                foundChange = true;

            mirrored = so.MirrorDeformation;

            return foundChange;
        }

        internal bool NormalChange()
        {
            bool foundChange = false;
            if (so.NormalType != normalType)
                foundChange = true;

            normalType = so.NormalType;

            return foundChange;
        }

        internal bool SplineObjectTypeChange(out SplineObjectType oldType)
        {
            bool foundChange = false;
            if (so.Type != type)
                foundChange = true;

            oldType = type;
            type = so.Type;

            return foundChange;
        }

        internal bool TransformPosRotChange()
        {
            bool foundChange = false;
            if (TransformPosChange()) foundChange = true;
            if (TransformRotChange()) foundChange = true;

            return foundChange;

            bool TransformPosChange()
            {
                bool foundChange = false;
                if (!GeneralUtility.IsEqual(position, so.transform.position)) foundChange = true;

                position = so.transform.position;

                return foundChange;
            }

            bool TransformRotChange()
            {
                bool foundChange = false;
                if (!GeneralUtility.IsEqual(rotation.eulerAngles, so.transform.rotation.eulerAngles)) foundChange = true;

                rotation = so.transform.rotation;

                return foundChange;
            }
        }

        internal bool TransformScaleChange()
        {
            bool foundChange = false;
            if (!GeneralUtility.IsEqualFast(localScale, so.transform.localScale)) foundChange = true;

            localScale = so.transform.localScale;

            return foundChange;
        }

        internal bool UvSettingsChange(out bool uvChanged, out bool uv2Changed, out bool uv3Changed, out bool uv4Changed)
        {
            bool foundChange = false;
            uvChanged = false;
            uv2Changed = false;
            uv3Changed = false;
            uv4Changed = false;

            if(so.MeshContainerCount == 0)
                return false;

            MeshContainer mc = so.GetMeshContainerAtIndex(0);

            if(!mc.MeshComponentIsMeshFilter())
                return false;

            UvSettings currentUvSettings = mc.GetUvSettings();
            UvSettings[] settings;

            if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_ALL_AXELS)
                settings = uvSettingPerLongest;
            else if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_Z_AXEL)
                settings = uvSettingPerLongestZ;
            else
                settings = uvSettingPerSubdivide;

            if(settings == null)
            {
                if(GeneralUtility.IsZeroFast(currentUvSettings.uvOffset) && GeneralUtility.IsZeroFast(currentUvSettings.uv2Offset) &&
                   GeneralUtility.IsZeroFast(currentUvSettings.uv3Offset) && GeneralUtility.IsZeroFast(currentUvSettings.uv4Offset))
                {
                    return false;
                }

                settings = new UvSettings[33];

                if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_ALL_AXELS)
                    uvSettingPerLongest = settings;
                else if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_Z_AXEL)
                    uvSettingPerLongestZ = settings;
                else
                    uvSettingPerSubdivide = settings;
            }

            ref UvSettings cached = ref settings[mc.Resolution];

            if (!GeneralUtility.IsEqualFast(currentUvSettings.uvOffset, cached.uvOffset))
            {
                foundChange = true;
                uvChanged = true;
                cached.uvOffset = currentUvSettings.uvOffset;
            }

            if (!GeneralUtility.IsEqualFast(currentUvSettings.uv2Offset, cached.uv2Offset))
            {
                foundChange = true;
                uv2Changed = true;
                cached.uv2Offset = currentUvSettings.uv2Offset;
            }
            if (!GeneralUtility.IsEqualFast(currentUvSettings.uv3Offset, cached.uv3Offset))
            {
                foundChange = true;
                uv3Changed = true;
                cached.uv3Offset = currentUvSettings.uv3Offset;
            }

            if (!GeneralUtility.IsEqualFast(currentUvSettings.uv4Offset, cached.uv4Offset))
            {
                foundChange = true;
                uv4Changed = true;
                cached.uv4Offset = currentUvSettings.uv4Offset;
            }

            return foundChange;
        }

        internal void UpdatePosRotSplineSpace()
        {
            localSplinePosition = so.localSplinePosition;
            localSplineRotation = so.localSplineRotation;
        }

        internal void UpdateCombinedParentPosRotScaleChange()
        {
            SplineObject currAco = so;
            Vector3 pos = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.zero;

            for (int i = 0; i < 25; i++)
            {
                currAco = currAco.SoParent;
                if (currAco == null)
                    break;

                pos += currAco.localSplinePosition;
                rotation = rotation * currAco.localSplineRotation;
                scale += currAco.transform.localScale;
            }

            combinedParentPositionInSplineSpace = pos;
            combinedParentRotationInSplineSpace = rotation;
            combinedParentLocalScale = scale;
        }

        internal void UpdateTransform()
        {
            localScale = so.transform.localScale;
            position = so.transform.position;
            rotation = so.transform.rotation;
        }

        private void UpdateUvSettings()
        {
            if(so.MeshContainerCount > 0)
            {
                MeshContainer mc = so.GetMeshContainerAtIndex(0);
                if(mc.MeshComponentIsMeshFilter())
                {
                    UvSettings uvSetting = mc.GetUvSettings();
                    bool foundNoneZero = false;

                    if (!GeneralUtility.IsZeroFast(uvSetting.uvOffset)) foundNoneZero = true;
                    else if (!GeneralUtility.IsZeroFast(uvSetting.uv2Offset)) foundNoneZero = true;
                    else if (!GeneralUtility.IsZeroFast(uvSetting.uv3Offset)) foundNoneZero = true;
                    else if (!GeneralUtility.IsZeroFast(uvSetting.uv4Offset)) foundNoneZero = true;

                    if(foundNoneZero)
                    {
                        if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_ALL_AXELS)
                        {
                            if(uvSettingPerLongest == null)
                                uvSettingPerLongest = new UvSettings[33];

                            uvSettingPerLongest[mc.Resolution] = mc.GetUvSettings();
                        }
                        else if(mc.ResolutionMode == ResolutionMode.SUBDIVIDE_Z_AXEL)
                        {
                            if(uvSettingPerLongestZ == null)
                                uvSettingPerLongestZ = new UvSettings[33];

                            uvSettingPerLongestZ[mc.Resolution] = mc.GetUvSettings();
                        }
                        else
                        {
                            if(uvSettingPerSubdivide == null)
                                uvSettingPerSubdivide = new UvSettings[33];

                            uvSettingPerSubdivide[mc.Resolution] = mc.GetUvSettings();
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        private int childCount;
        private int componentCount;
        private bool isStatic;

        internal bool EditorStaticChange()
        {
            bool foundChange = false;
            if (so.gameObject.isStatic != isStatic)
            {
                foundChange = true;
            }

            isStatic = so.gameObject.isStatic;

            return foundChange;
        }

        internal bool EditorComponentCountChange()
        {
            bool foundChange = false;
            if (so.gameObject.GetComponentCount() != componentCount)
            {
                foundChange = true;
            }

            componentCount = so.gameObject.GetComponentCount();

            return foundChange;
        }

        internal bool EditorChildCountChange(out int dif)
        {
            dif = 0;

            bool foundChange = false;
            if (so.transform.childCount != childCount)
            {
                dif = so.transform.childCount - childCount;
                foundChange = true;
            }

            childCount = so.transform.childCount;

            return foundChange;
        }
#endif
    }
}
