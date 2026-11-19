// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: EHandleCloningUi.cs
//
// Author: Mikael Danielsson
// Date Created: 20-09-2026
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;

using UnityEngine;
using UnityEditor;

using SplineArchitect.ScriptableObjects;
using SplineArchitect.Utility;

namespace SplineArchitect
{
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    internal static class EHandleCloningUi
    {
        private static CloningMenuUndoState cloningMenuUndoState;

        internal static bool PrefabsMinimized
        {
            get
            {
                if (cloningMenuUndoState == null)
                    return true;

                return cloningMenuUndoState.prefabsMinimized;
            }
        }

        internal static bool RanomizedMinimized
        {
            get
            {
                if (cloningMenuUndoState == null)
                    return true;

                return cloningMenuUndoState.randomizedMinimized;
            }
        }

        internal static int SelectedPrefabId
        {
            get
            {
                if (cloningMenuUndoState == null)
                    return -1;

                return cloningMenuUndoState.selectedPrefabId;
            }
        }

        internal static void AfterAssemblyReload()
        {
            string SelectionUndoStateKey = "SplineArchitect_cloningMenuUndoState";
            cloningMenuUndoState = EGeneralUtility.GetObjectFromSessionState(SelectionUndoStateKey) as CloningMenuUndoState;

            if (cloningMenuUndoState != null)
                return;

            cloningMenuUndoState = ScriptableObject.CreateInstance<CloningMenuUndoState>();
            cloningMenuUndoState.hideFlags = HideFlags.HideAndDontSave;
            EGeneralUtility.SetObjectSessionKey(SelectionUndoStateKey, cloningMenuUndoState);
        }

        internal static void UpdateUndoStateRecordUndo(Action<CloningMenuUndoState> onUpdate, string recordName = null, EHandleUndo.RecordType recordType = EHandleUndo.RecordType.RECORD_OBJECT)
        {
            EHandleUndo.RecordNow(cloningMenuUndoState, recordName, recordType);
            onUpdate.Invoke(cloningMenuUndoState);
        }

        internal static void UpdateUndoState(Action<CloningMenuUndoState> onUpdate)
        {
            onUpdate.Invoke(cloningMenuUndoState);
        }
    }
}
