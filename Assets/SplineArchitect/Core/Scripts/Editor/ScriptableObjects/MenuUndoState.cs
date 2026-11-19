// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: MenuUndoState.cs
//
// Author: Mikael Danielsson
// Date Created: 10-07-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace SplineArchitect.ScriptableObjects
{
    internal partial class MenuUndoState : ScriptableObject
    {
        // Spline
        [SerializeField] internal string activeSplineMenu = "deformation";
        [SerializeField] internal string activeAnchorMenu = "general";
        [SerializeField] internal bool schedulingMinimized = true;
        [SerializeField] internal bool renderingMinimized = true;
        [SerializeField] internal bool linksMinimized = true;
        [SerializeField] internal int selectedNoiseLayerIndex;

        // Spline Object
        [SerializeField] internal bool snapSettingsMinimized = true;
        [SerializeField] internal bool meshSettingsMinimized = true;
        [SerializeField] internal bool advancedSettingsMinimized = true;
        [SerializeField] internal string activeSplineObjectMenu = "general";

        // Mesh Container
        [SerializeField] internal int uvLayer;
    }
}
