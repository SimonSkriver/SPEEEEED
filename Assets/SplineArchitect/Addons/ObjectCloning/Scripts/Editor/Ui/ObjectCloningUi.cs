// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: ObjectCloningUi.cs
//
// Author: Mikael Danielsson
// Date Created: 20-04-2025
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

using UnityEngine;

using SplineArchitect.Libraries;
using SplineArchitect.Utility;

namespace SplineArchitect.Ui
{
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    public class ObjectCloningUi
    {
        private static string[] cloneDistributionOptions = new string[] { "None", "Fit using scale", "Fit using spacing", "Corrected spacing"};
        private static string[] uvChannelOptions = new string[] { "UV1", "UV2", "UV3",  "UV4"};

        public static Rect CalcSplineObjectWindowSize(SplineObject so)
        {
            Rect rect = new Rect();
            rect.height = WindowBase.headerHeight + WindowBase.toolbarHeight + WindowBase.bottomHeight;
            rect.height += WindowBase.sectionHeight * 3;
            rect.height += WindowBase.itemHeight * 4;

            if (!EHandleCloningUi.PrefabsMinimized)
            {
                rect.height += WindowBase.itemHeight * so.originClones.Count;

                if(EHandleCloningUi.SelectedPrefabId > -1 && EHandleCloningUi.SelectedPrefabId < so.originClones.Count)
                {
                    OriginClone oc = so.originClones[EHandleCloningUi.SelectedPrefabId];

                    if(oc != null && oc.prefab == null)
                        rect.height += WindowBase.itemHeight * 1 + 20;
                    else
                    {
                        rect.height += WindowBase.itemHeight * 4 + 20;
                    }

                    if(oc != null && so.randomizePrefabClones)
                        rect.height += WindowBase.itemHeight * 1;
                }
            }

            if (!EHandleCloningUi.RanomizedMinimized)
            {
                rect.height += WindowBase.itemHeight * 6;
            }

            rect.width = 323;

            return rect;
        }

        public static void DrawSplineObjectWindow(SplineObject so)
        {
            Spline selectedSpline = EHandleSelection.selectedSpline;
            bool leftMouseUp = false;
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0) leftMouseUp = true;

            EUiUtility.CreateSection("OBJECT CLONING");

            GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
            GUILayout.Space(-1);
            EUiUtility.CreateLabelField("<b>Padding</b>", LibraryGUIStyle.textDefault, true, 100);

            GUILayout.FlexibleSpace();
            EUiUtility.CreateDelayedFloatFieldWithLabel("Start", so.cloneStartPadding, (value) =>
            {
                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                {
                    selected.cloneStartPadding = value;
                    selected.MarkCloneVersionDirty();
                    EHandleObjectCloning.UpdateCloneAmount(selected);
                }, "Changed start space", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
            }, leftMouseUp, 48, 31, true, true, "start padding all");
            GUILayout.Space(4);
            EUiUtility.CreateDelayedFloatFieldWithLabel("End", so.cloneEndPadding, (value) =>
            {
                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                {
                    selected.cloneEndPadding = value;
                    selected.MarkCloneVersionDirty();
                    EHandleObjectCloning.UpdateCloneAmount(selected);
                }, "Changed start space", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
            }, leftMouseUp, 48, 25, true, !so.cloneUseFixedAmount, "end padding all");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
            if (!so.cloneUseFixedAmount)
            {
                EUiUtility.CreateSliderAndDelayedInputField("Amount:", so.cloneAmount, (newValue, changeBySlider) => { }, 0, 500, leftMouseUp, 70, 58, true, false);
            }
            else
            {
                EUiUtility.CreateSliderAndDelayedInputField("Amount:", so.cloneAmount, (newValue, changeBySlider) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        if (newValue < 0) newValue = 0;
                        selected.cloneAmount = (int)newValue;
                        if (!changeBySlider) selected.MarkCloneVersionDirty();
                        EHandleObjectCloning.UpdateCloneAmount(selected);
                    }, "Changed clone amount", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 0, 100, leftMouseUp, 70, 58, true, true, true);
            }

            EUiUtility.CreateCheckbox(so.cloneUseFixedAmount, (value) =>
            {
                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                {
                    selected.cloneUseFixedAmount = value;

                    if(selected.cloneUseFixedAmount && selected.cloneDistributionMode == CloneDistributionMode.FIT_USING_SCALE) 
                        selected.cloneDistributionMode = CloneDistributionMode.NONE;

                    selected.MarkCloneVersionDirty();
                    EHandleObjectCloning.UpdateCloneAmount(selected);
                }, "Change fixed amount");
            }, 15);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
            EUiUtility.CreatePopupField("Distribution:", 110, (int)so.cloneDistributionMode, cloneDistributionOptions, (value) =>
            {
                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                {
                    selected.cloneDistributionMode = (CloneDistributionMode)value;
                    selected.MarkCloneVersionDirty();
                    EHandleObjectCloning.UpdateCloneAmount(selected);
                }, "Toggled clone distribution mode", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
            }, 77, true, true, false, false);

            GUILayout.FlexibleSpace();
            EUiUtility.CreateToggleField("Keep prefab link:", so.keepPrefabLink, (value) =>
            {
                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                {
                    selected.keepPrefabLink = value;
                    if(selected.cloningActive)
                    {
                        EHandleObjectCloning.Disable(selected);
                        EHandleObjectCloning.EnableUsingPrefabs(selected);
                    }
                }, "Toggled fitt", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
            }, true, true, 101, 15);
            GUILayout.EndHorizontal();

            EUiUtility.CreateSubSection("PREFABS", EHandleCloningUi.PrefabsMinimized, () =>
            {
                EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>
                {
                    undoState.prefabsMinimized = !undoState.prefabsMinimized;
                }, "Toggled prefabs section");
            });

            // Prefab
            if (!EHandleCloningUi.PrefabsMinimized)
            {
                OriginClone firstOriginClone = so.originClones.Count > 0 ? so.originClones[0] : null;

                if(so.originClones == null)
                    so.originClones = new List<OriginClone>();

                if(so.originClones.Count == 0)
                    so.originClones.Add(new OriginClone());

                for (int i = 0; i < so.originClones.Count; i++)
                {
                    OriginClone originClone = so.originClones[i];
                    bool isSelected = EHandleCloningUi.SelectedPrefabId == i;

                    GUIStyle backgroundStyleHeader = EUiUtility.GetBackgroundStyle();
                    if (isSelected) backgroundStyleHeader = LibraryGUIStyle.backgroundSelectedLayerHeader;

                    GUILayout.BeginHorizontal(backgroundStyleHeader);

                    EUiUtility.CreateObjectField("Prefab:", originClone.prefab, typeof(GameObject), (newPrefab) =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                        {
                            if (selected.originClones.Count > i)
                            {
                                selected.originClones[i].prefab = (GameObject)newPrefab;

                                if(selected.cloningActive)
                                {
                                    EHandleObjectCloning.Disable(selected);
                                    EHandleObjectCloning.EnableUsingPrefabs(selected);
                                }
                            }
                        }, "Set clone prefab", false , EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                    }, 170, 0, true);

                    GUILayout.Space(40);

                    if(i == 0)
                    {
                        EUiUtility.CreateButton(ButtonType.DEFAULT_GREEN, LibraryGUIContent.iconAdd, 22, 18, () =>
                        {
                            EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                            {
                                selected.originClones.Add(new OriginClone());

                                if (selected.originClones.Count == 1)
                                    selected.originClones.Add(new OriginClone());

                                if(selected.cloningActive)
                                {
                                    EHandleObjectCloning.Disable(selected);
                                    EHandleObjectCloning.EnableUsingPrefabs(selected);
                                }

                            }, "Added prefab field");
                        }, so.originClones.Count < 21);
                    }
                    else
                    {
                        EUiUtility.CreateButton(ButtonType.DEFAULT_RED, LibraryGUIContent.iconRemove, 22, 18, () =>
                        {
                            EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                            {
                                if (selected.originClones.Count > i)
                                {
                                    selected.originClones.RemoveAt(i);

                                    if(selected.cloningActive)
                                    {
                                        EHandleObjectCloning.Disable(selected);
                                        EHandleObjectCloning.EnableUsingPrefabs(selected);
                                    }
                                }
                            }, "Removed prefab field", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                        });
                    }

                    //Move down
                    EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.iconDownArrow, 22, 18, () =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                        {
                            if (selected.originClones.Count > i + 1)
                            {
                                OriginClone oc1 = selected.originClones[i];
                                OriginClone oc2 = selected.originClones[i + 1];
                                selected.originClones[i] = oc2;
                                selected.originClones[i + 1] = oc1;
                            }

                            if(EHandleCloningUi.SelectedPrefabId == i)
                            {
                                EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>{undoState.selectedPrefabId++;});
                            }
                            else if(EHandleCloningUi.SelectedPrefabId == i + 1)
                            {
                                EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>{undoState.selectedPrefabId--;});
                            }

                            if(selected.cloningActive)
                            {
                                EHandleObjectCloning.Disable(selected);
                                EHandleObjectCloning.EnableUsingPrefabs(selected);
                            }
                        }, "Moved prefab down", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                        GUI.FocusControl(null);
                    }, i < (so.originClones.Count - 1) && so.originClones.Count > 1);

                    //Move up
                    EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.iconUpArrow, 22, 18, () =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                        {
                            if (selected.originClones.Count > i)
                            {
                                OriginClone oc3 = selected.originClones[i];
                                OriginClone oc4 = selected.originClones[i - 1];
                                selected.originClones[i] = oc4;
                                selected.originClones[i - 1] = oc3;

                                if(EHandleCloningUi.SelectedPrefabId == i)
                                {
                                    EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>{undoState.selectedPrefabId--;});
                                }
                                else if(EHandleCloningUi.SelectedPrefabId == i - 1)
                                {
                                    EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>{undoState.selectedPrefabId++;});
                                }

                                if(selected.cloningActive)
                                {
                                    EHandleObjectCloning.Disable(selected);
                                    EHandleObjectCloning.EnableUsingPrefabs(selected);
                                }
                            }
                        }, "Moved prefab up", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                        GUI.FocusControl(null);
                    }, i > 0 && so.originClones.Count > 1);

                    if(isSelected)
                    {
                        GUILayout.FlexibleSpace();
                        EUiUtility.CreateButton(ButtonType.DEFAULT_GREEN, LibraryGUIContent.iconMinimize, 22, 18, () =>
                        {
                            EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>
                            {
                                undoState.selectedPrefabId = -1;
                            }, "Minimized prefab settings");
                            GUI.FocusControl(""); 
                        });

                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        EUiUtility.CreateButton(ButtonType.DEFAULT_GREEN, LibraryGUIContent.iconSelectLayer, 22, 18, () =>
                        {
                            EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>
                            {
                                undoState.selectedPrefabId = i;
                            }, "Selected prefab settings");
                            GUI.FocusControl(""); 
                        });
                    }

                    GUILayout.EndHorizontal();
                    if(isSelected)
                    {
                        EUiUtility.CreateHorizontalBlackLine();

                        EUiUtility.CreateLayerSection("GENERAL", 135, 50);

                        if(originClone.prefab == null)
                        {
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(10);
                            EUiUtility.CreateDelayedFloatFieldWithLabel("Spacing:", originClone.positionOffset.z, (value) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].positionOffset.z = value;
                                        selected.MarkCloneVersionDirty();
                                        EHandleObjectCloning.UpdateCloneAmount(selected);
                                    }
                                }, "Changed offset for clone prefab", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, leftMouseUp, 48, 52, true, true);
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(5);
                            EUiUtility.CreateXYZInputDelayedFields("<b>Offset</b>", originClone.positionOffset, (value, dif) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].positionOffset = value;
                                        selected.MarkCloneVersionDirty();
                                        EHandleObjectCloning.UpdateCloneAmount(selected);
                                    }
                                }, "Changed offset for clone prefab", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, 116, 10, 60, leftMouseUp, false, true);
                            GUILayout.EndHorizontal();
                            
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(5);
                            EUiUtility.CreateXYZInputDelayedFields("<b>Rotation</b>", originClone.rotation, (value, dif) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].rotation = value;
                                        selected.MarkCloneVersionDirty();
                                        EHandleObjectCloning.UpdateCloneAmount(selected);
                                    }
                                }, "Changed rotation for clone prefab", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, 116, 10, 60, leftMouseUp, false, true);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(5);
                            EUiUtility.CreateXYZInputDelayedFields("<b>Scale</b>", originClone.scale, (value, dif) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].scale = value;
                                        selected.MarkCloneVersionDirty();
                                        EHandleObjectCloning.UpdateCloneAmount(selected);
                                    }
                                }, "Changed scale for clone prefab", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, 116, 10, 60, leftMouseUp, false, true);
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(5);
                            EUiUtility.CreateToggleField("Deform:", originClone.deform, (value) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].deform = value;

                                        if(selected.cloningActive)
                                        {
                                            EHandleObjectCloning.Disable(selected);
                                            EHandleObjectCloning.EnableUsingPrefabs(selected);
                                        }
                                        selected.MarkCloneVersionDirty();
                                    }
                                }, "Toggle deform for clone", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, true, true, 52, 14);

                            GUILayout.FlexibleSpace();
                            EUiUtility.CreateLabelField("Incremental:", LibraryGUIStyle.textDefault, true, 77);
                            EUiUtility.CreatePopupField("", 50, EHandleUi.SelectedUvLayer, uvChannelOptions, (value) =>
                            {
                                EHandleUi.UpdateMenuStateRecordUndo((menuUndo) =>
                                {
                                    menuUndo.uvLayer = value;
                                }, "Selected uv layer");
                            }, -1, true, originClone.deform, true);
                            
                            bool uvIncrementalX;
                            if(EHandleUi.SelectedUvLayer == 1) uvIncrementalX = originClone.incrementUv2X;
                            else if(EHandleUi.SelectedUvLayer == 2) uvIncrementalX = originClone.incrementUv3X;
                            else if(EHandleUi.SelectedUvLayer == 3) uvIncrementalX = originClone.incrementUv4X;
                            else uvIncrementalX = originClone.incrementUvX;

                            EUiUtility.CreateToggleField("X", uvIncrementalX, (value) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        if(EHandleUi.SelectedUvLayer == 1) selected.originClones[i].incrementUv2X = value;
                                        else if(EHandleUi.SelectedUvLayer == 2) selected.originClones[i].incrementUv3X = value;
                                        else if(EHandleUi.SelectedUvLayer == 3) selected.originClones[i].incrementUv4X = value;
                                        else selected.originClones[i].incrementUvX = value;

                                        if(selected.cloningActive)
                                        {
                                            EHandleObjectCloning.UpdateCloneAmount(selected);
                                        }
                                    }
                                }, "Toggle deform for clone", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, originClone.deform, true, 12, 14);

                            bool uvIncrementalY;
                            if(EHandleUi.SelectedUvLayer == 1) uvIncrementalY = originClone.incrementUv2Y;
                            else if(EHandleUi.SelectedUvLayer == 2) uvIncrementalY = originClone.incrementUv3Y;
                            else if(EHandleUi.SelectedUvLayer == 3) uvIncrementalY = originClone.incrementUv4Y;
                            else uvIncrementalY = originClone.incrementUvY;

                            EUiUtility.CreateToggleField("Y", uvIncrementalY, (value) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        if(EHandleUi.SelectedUvLayer == 1) selected.originClones[i].incrementUv2Y = value;
                                        else if(EHandleUi.SelectedUvLayer == 2) selected.originClones[i].incrementUv3Y = value;
                                        else if(EHandleUi.SelectedUvLayer == 3) selected.originClones[i].incrementUv4Y = value;
                                        else selected.originClones[i].incrementUvY = value;

                                        if(selected.cloningActive)
                                        {
                                            EHandleObjectCloning.UpdateCloneAmount(selected);
                                        }
                                    }
                                }, "Toggle deform for clone", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, originClone.deform, true, 12, 14);
                            GUILayout.Space(9);
                            GUILayout.EndHorizontal();
                        }

                        if(so.randomizePrefabClones)
                        {
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
                            GUILayout.Space(5);
                            EUiUtility.CreateSliderAndDelayedInputField("Probability:", originClone.probability, (value, dif) =>
                            {
                                EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                                {
                                    if (selected.originClones.Count > i)
                                    {
                                        selected.originClones[i].probability = value;
                                        if(selected.cloningActive)
                                        {
                                            EHandleObjectCloning.Disable(selected);
                                            EHandleObjectCloning.EnableUsingPrefabs(selected);
                                        }
                                        selected.MarkCloneVersionDirty();
                                    }
                                }, "Changed probability for clone prefab", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                            }, 0, 1, leftMouseUp, 48, 73, true, so.randomizePrefabClones);
                            GUILayout.Space(10);
                            GUILayout.EndHorizontal();
                        }

                        if(i == so.originClones.Count - 1)
                        {
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection, GUILayout.Height(4));
                            GUILayout.Space(4);
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection, GUILayout.Height(3));
                            GUILayout.Space(3);
                            GUILayout.EndHorizontal();
                            EUiUtility.CreateHorizontalBlackLine();
                        }
                    }
                }
            }

            EUiUtility.CreateSubSection("RANDOMIZING", EHandleCloningUi.RanomizedMinimized, () =>
            {
                EHandleCloningUi.UpdateUndoStateRecordUndo((undoState) =>
                {
                    undoState.randomizedMinimized = !undoState.randomizedMinimized;
                }, "Toggled randomizing section");
            });

            if(!EHandleCloningUi.RanomizedMinimized)
            {
                GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
                EUiUtility.CreateToggleField("Randomize prefabs:", so.randomizePrefabClones, (value) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.randomizePrefabClones = value;

                        if(selected.cloningActive)
                        {
                            EHandleObjectCloning.Disable(selected);
                            EHandleObjectCloning.EnableUsingPrefabs(selected);
                        }
                    }, "Toggle randomize prefabs", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, true, true, 118, 14);

                GUILayout.FlexibleSpace();
                EUiUtility.CreateDelayedFloatFieldWithLabel("Seed:", so.cloneRandomizeSeed, (value) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.cloneRandomizeSeed = Mathf.RoundToInt(value);

                        if(selected.cloningActive)
                        {
                            EHandleObjectCloning.Disable(selected);
                            EHandleObjectCloning.EnableUsingPrefabs(selected);
                        }
                    }, "Change randomize clone seed", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, leftMouseUp, 48, 35, true);
                GUILayout.EndHorizontal();

                EUiUtility.CreateXYZFromToFields("Position", so.randomizedPositonFrom, so.randomizedPositonTo, (from, to) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.randomizedPositonFrom = from;
                        selected.randomizedPositonTo = to;

                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Change randomize position", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 58, 44);

                EUiUtility.CreateXYZFromToFields("Rotation", so.randomizedEulerFrom, so.randomizedEulerTo, (from, to) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.randomizedEulerFrom = from;
                        selected.randomizedEulerTo = to;

                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Change randomize rotation", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 58, 44);

                GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
                EUiUtility.CreateButtonToggle(ButtonType.DEFAULT, LibraryGUIContent.iconNotConstrained, LibraryGUIContent.iconConstrained, 20, 18, () =>
                {
                    bool newValue = !so.constrainedScale;
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.constrainedScale = newValue;
                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Toggle constrained scale", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, so.constrainedScale);
                GUILayout.Space(-5);
                if(so.constrainedScale)
                {
                    EUiUtility.CreateFromToInputField("<b>Scale</b>", so.randomizedScaleFrom, so.randomizedScaleTo, (from, to) =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                        {
                            selected.randomizedScaleFrom = from;
                            selected.randomizedScaleTo = to;

                            if(selected.cloningActive)
                            {
                                selected.MarkCloneVersionDirty();
                                EHandleObjectCloning.UpdateCloneAmount(selected);
                            }
                        }, "Change randomize scale", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                    }, 38, 5, 44, true);
                }
                else
                {
                    EUiUtility.CreateXYZFromToFields("Scale", so.randomizedXYZScaleFrom, so.randomizedXYZScaleTo, (from, to) =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                        {
                            selected.randomizedXYZScaleFrom = from;
                            selected.randomizedXYZScaleTo = to;

                            if(selected.cloningActive)
                            {
                                selected.MarkCloneVersionDirty();
                                EHandleObjectCloning.UpdateCloneAmount(selected);
                            }
                        }, "Change randomize scale", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                    }, 39, 44, false, true);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(EUiUtility.GetBackgroundStyle());
                EUiUtility.CreateSliderAndInputField("Mirror:", so.mirrorChance, (value, changeddBySlider) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.mirrorChance = value;

                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Change mirror chance", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 0, 1, 57, 34, 46, true);

                EUiUtility.CreateSliderAndInputField("Rotate 180:", so.rotation180Chance, (value, changeddBySlider) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.rotation180Chance = value;

                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Change rotate 180 chance", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 0, 1, 62, 34, 72, true);
                GUILayout.EndHorizontal();

                EUiUtility.CreateSliderAndInputField("Strength:", so.randomizationStrength, (value, changeddBySlider) =>
                {
                    EHandleSelection.UpdatedSelectedSplineObjectsRecordUndo((selected) =>
                    {
                        selected.randomizationStrength = value;

                        if(selected.cloningActive)
                        {
                            selected.MarkCloneVersionDirty();
                            EHandleObjectCloning.UpdateCloneAmount(selected);
                        }
                    }, "Change randomize scale", false, EHandleUndo.RecordType.REGISTER_COMPLETE_OBJECT);
                }, 0, 3, 200, 52, 61);
            }

            SplineObject cloneParent = EHandleObjectCloning.GetCloneParent(so);

            if (cloneParent != null && cloneParent.currentClones != null && cloneParent.currentClones.Contains(so))
            {
                GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundMainLayerButtons);
                GUILayout.FlexibleSpace();
                EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.textDisconnect, 76, 18, () =>
                {
                    EHandleUndo.RecordNow(cloneParent);
                    EHandleSelection.UpdatedSelectedSplineObjects((selected) =>
                    {
                        for(int i = 0; i < cloneParent.currentClones.Count; i++) 
                        {
                            if (selected == cloneParent.currentClones[i])
                            {
                                cloneParent.currentClones[i] = null;
                                break;
                            }
                        }

                        bool allIsNull = true;
                        for(int i = 0; i < cloneParent.currentClones.Count; i++) 
                        {
                            if (cloneParent.currentClones[i] != null)
                            {
                                allIsNull = false;
                                break;
                            }
                        }

                        if(allIsNull)
                        {
                            EHandleObjectCloning.DisconnectClonesAndDisable(cloneParent);
                        }
                    });

                    EHandleTool.ActivatePositionToolForSplineObject(so.SplineParent, so);
                    EHandleSceneView.RepaintCurrent();
                });
                GUILayout.EndHorizontal();
            }
            else
            {
                if (!so.cloningActive && (so.currentClones == null || so.currentClones.Count == 0))
                {
                    bool foundValidPrefab = false;

                    foreach(OriginClone oc2 in so.originClones)
                    {
                        if(oc2.prefab != null)
                        {
                            foundValidPrefab = true;
                            break;
                        }
                    }

                    GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundMainLayerButtons);
                    GUILayout.FlexibleSpace();

                    if (!foundValidPrefab)
                        EUiUtility.CreateInfoMessageIcon(LibraryGUIContent.warningMsgCantCloneWithNoPrefabs);
                    else if (so.Type != SplineObjectType.DEFORMATION)
                        EUiUtility.CreateInfoMessageIcon(LibraryGUIContent.warningMsgCanOnlyCloneWithTypeDeformation);

                    EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.textClone, 48, 18, () =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjects((selected) =>
                        {
                            if(selected.Type != SplineObjectType.DEFORMATION)
                                return;

                            EHandleObjectCloning.EnableUsingPrefabs(selected);
                        });
                    }, selectedSpline != null && foundValidPrefab && so.Type == SplineObjectType.DEFORMATION);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundMainLayerButtons);
                    GUILayout.FlexibleSpace();
                    EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.textDisconnectClones, 118, 18, () =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjects((selected) =>
                        {
                            EHandleObjectCloning.DisconnectClonesAndDisable(selected);
                        });
                    }, true);

                    EUiUtility.CreateButton(ButtonType.DEFAULT, LibraryGUIContent.textDeleteClones, 92, 18, () =>
                    {
                        EHandleSelection.UpdatedSelectedSplineObjects((selected) =>
                        {
                            EHandleObjectCloning.Disable(selected);
                            selected.MarkCloneVersionDirty();
                        });
                    }, true);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
