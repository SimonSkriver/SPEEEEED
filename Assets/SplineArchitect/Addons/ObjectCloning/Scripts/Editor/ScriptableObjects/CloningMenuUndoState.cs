// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: CloningMenuUndoState.cs
//
// Author: Mikael Danielsson
// Date Created: 20-09-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace SplineArchitect.ScriptableObjects
{
    internal class CloningMenuUndoState : ScriptableObject
    {
        // SplineObject
        [SerializeField] internal bool prefabsMinimized = true;
        [SerializeField] internal bool randomizedMinimized = true;
        [SerializeField] internal int selectedPrefabId;
    }
}
