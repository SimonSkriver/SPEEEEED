// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: SplineObject_Cloning.cs
//
// Author: Mikael Danielsson
// Date Created: 21-04-2025
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

using UnityEngine;

namespace SplineArchitect
{
    public partial class SplineObject : MonoBehaviour
    {
#if UNITY_EDITOR

        //Stored data
        [HideInInspector, SerializeField] internal bool cloningActive;
        [HideInInspector, SerializeField] internal bool cloneUseFixedAmount;
        [HideInInspector, SerializeField] internal bool keepPrefabLink = true;
        [HideInInspector, SerializeField] internal int cloneAmount;
        [HideInInspector, SerializeField] internal float cloneStartPadding;
        [HideInInspector, SerializeField] internal float cloneEndPadding;
        [HideInInspector, SerializeField] internal CloneDistributionMode cloneDistributionMode = CloneDistributionMode.FIT_USING_SCALE;
        [HideInInspector, SerializeField] internal List<SplineObject> currentClones = new List<SplineObject>();
        [HideInInspector, SerializeField] internal List<OriginClone> originClones = new List<OriginClone>();
        // Randomize
        [HideInInspector, SerializeField] internal bool randomizePrefabClones;
        [HideInInspector, SerializeField] internal int cloneRandomizeSeed;
        [HideInInspector, SerializeField] internal Vector3 randomizedPositonFrom;
        [HideInInspector, SerializeField] internal Vector3 randomizedPositonTo;
        [HideInInspector, SerializeField] internal Vector3 randomizedEulerFrom;
        [HideInInspector, SerializeField] internal Vector3 randomizedEulerTo;
        [HideInInspector, SerializeField] internal Vector3 randomizedXYZScaleFrom;
        [HideInInspector, SerializeField] internal Vector3 randomizedXYZScaleTo;
        [HideInInspector, SerializeField] internal float randomizedScaleFrom;
        [HideInInspector, SerializeField] internal float randomizedScaleTo;
        [HideInInspector, SerializeField] internal float randomizationStrength = 1;
        [HideInInspector, SerializeField] internal float mirrorChance;
        [HideInInspector, SerializeField] internal float rotation180Chance;
        [HideInInspector, SerializeField] internal bool constrainedScale = true;

        internal void MarkCloneVersionDirty()
        {
            ScriptableObjects.SplineObjectUndoState undoState = GetUndoState();
            undoState.cloneVersion++;
        }

        internal bool IsCloneVersionDirty()
        {
            ScriptableObjects.SplineObjectUndoState undoState = GetUndoState();
            bool dirty = undoState.cloneVersion != undoState.oldCloneVersion;
            undoState.oldCloneVersion = undoState.cloneVersion;

            return dirty;
        }
#endif
    }
}
