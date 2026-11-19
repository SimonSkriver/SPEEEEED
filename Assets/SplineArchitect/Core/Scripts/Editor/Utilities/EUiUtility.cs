// -----------------------------------------------------------------------------
// SplineArchitect
// Filename: EUiUtility.cs
//
// Author: Mikael Danielsson
// Date Created: 16-02-2023
// (C) 2023 Mikael Danielsson. All rights reserved.
// -----------------------------------------------------------------------------

using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using SplineArchitect.CustomTools;
using SplineArchitect.Libraries;
using SplineArchitect.Ui;

namespace SplineArchitect.Utility
{
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    public class EUiUtility
    {
        private static int backgroundStyleCounter = 0;
        private static float pendingFloat;
        private static float pendingFloat2;
        private static Vector2 pendingVector2;
        private static Vector3 pendingVector3;
        private static Vector3 pendingVector4;
        private static bool isPending;
        private static string realtimeDragControlName;
        private static int minMaxSliderControlerId = -1;
        private static string horizontalSliderId = "";

        public static void CreateSection(string title)
        {
            GUILayout.BeginVertical();
            CreateHorizontalBlackLine();
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundSection);
            CreateLabelField($"<b>{title}</b>", LibraryGUIStyle.textSection, true);
            GUILayout.EndHorizontal();
            CreateHorizontalBlackLine();
            GUILayout.EndVertical();
        }

        public static void CreateSubSection(string title, bool minimized, 
                                                          Action actionOnPress)
        {
            CreateHorizontalBlackLine();
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundSubSection);
            CreateLabelField($"<b>{title}</b>", LibraryGUIStyle.textSubSection, true);
            GUILayout.Space(100);
            CreateButton(ButtonType.SUB_MENU2, minimized ?
                                               LibraryGUIContent.iconMaximizeBlack :
                                               LibraryGUIContent.iconMinimizeBlack, 20, 14, () =>
            {
                actionOnPress.Invoke();
            });
            GUILayout.EndHorizontal();
            CreateHorizontalBlackLine();
        }

        public static void CreateSubSection(string title)
        {
            CreateHorizontalBlackLine();
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundSubSection);
            CreateLabelField($"<b>{title}</b>", LibraryGUIStyle.textSubSection, true);
            GUILayout.EndHorizontal();
            CreateHorizontalBlackLine();
        }

        public static void CreateLayerSection(string title, bool minimized, 
                                                            float spaceLeft, 
                                                            float labelWidth, 
                                                            float spaceRight, 
                                                            Action actionOnPress)
        {
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
            GUILayout.Space(spaceLeft);
            CreateLabelField($"<b>{title}</b>", LibraryGUIStyle.textLayerSection, true, labelWidth);
            GUILayout.Space(spaceRight);
            CreateButton(ButtonType.DEFAULT_WHITE, minimized ? LibraryGUIContent.iconMaximizeBlack :
                                                                LibraryGUIContent.iconMinimizeBlack, 18, 12, () =>
                                                                {
                                                                    actionOnPress.Invoke();
                                                                });
            GUILayout.EndHorizontal();
            CreateHorizontalSubHeader2Line();
        }

        public static void CreateLayerSection(string title, float spaceLeft, float labelWidth)
        {
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
            GUILayout.Space(spaceLeft);
            CreateLabelField($"<b>{title}</b>", LibraryGUIStyle.textLayerSection, true, labelWidth);
            GUILayout.EndHorizontal();
            CreateHorizontalSubHeader2Line();
        }

        public static void CreateColorField(string label, 
                                            Color color, 
                                            Action<Color> onChange, 
                                            float width = -1, 
                                            float labelWidth = -1, 
                                            bool skipGroup = false)
        {
            Color oldColor = color;

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());

            if (labelWidth == -1) GUILayout.Label(label, LibraryGUIStyle.textDefault);
            else GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            if (width == -1) color = EditorGUILayout.ColorField(color);
            else color = EditorGUILayout.ColorField(color, GUILayout.Width(width));

            if (!skipGroup) GUILayout.EndHorizontal();

            if (!GeneralUtility.IsEqual(oldColor, color))
            {
                onChange.Invoke(color);
            }
        }

        public static void CreateCheckbox(bool value, Action<bool> onChange, float width = 0)
        {
            bool oldValue = value;
            if(width == 0)
                value = EditorGUILayout.Toggle(value);
            else
                value = EditorGUILayout.Toggle(value, GUILayout.Width(width));

            if (oldValue != value)
                onChange.Invoke(value);
        }

        public static void CreateButtonToggle(ButtonType buttonType, 
                                              GUIContent icon, 
                                              GUIContent iconActive, 
                                              float width, 
                                              float height, 
                                              Action onPress, 
                                              bool active, 
                                              bool enable = true)
        {
            GUIStyle buttonStyle = null;
            GUIStyle buttonActiveStyle = null;

            if(buttonType == ButtonType.SUB_MENU)
            {
                buttonStyle = LibraryGUIStyle.buttonSubMenu;
                buttonActiveStyle = LibraryGUIStyle.buttonSubMenuActive;
            }
            else if (buttonType == ButtonType.DEFAULT_GREEN)
            {
                buttonStyle = LibraryGUIStyle.buttonDefaultGreen;
                buttonActiveStyle = LibraryGUIStyle.buttonDefaultActive;
            }
            else if (buttonType == ButtonType.DEFAULT_RED)
            {
                buttonStyle = LibraryGUIStyle.buttonDefaultRed;
                buttonActiveStyle = LibraryGUIStyle.buttonDefaultActive;
            }
            else if(buttonType == ButtonType.DEFAULT_MIDDLE_LEFT)
            {
                buttonStyle = LibraryGUIStyle.buttonDefaultMiddleLeft;
                buttonActiveStyle = LibraryGUIStyle.buttonDefaultMiddleLeftActive;
            }
            else
            {
                buttonStyle = LibraryGUIStyle.buttonDefault;
                buttonActiveStyle = LibraryGUIStyle.buttonDefaultActive;
            }

            GUI.enabled = enable;
            if (GUILayout.Button(active ? iconActive : icon, active ? buttonActiveStyle : buttonStyle, GUILayout.Width(width), GUILayout.Height(height)))
                onPress.Invoke();
            GUI.enabled = true;
        }

        public static void CreateButton(ButtonType buttonType, 
                                        GUIContent icon, 
                                        float width, 
                                        float height, 
                                        Action onPress, 
                                        bool enable = true)
        {
            GUIStyle buttonStyle = null;

            if (buttonType == ButtonType.SUB_MENU)
                buttonStyle = LibraryGUIStyle.buttonSubMenu;
            else if (buttonType == ButtonType.DEFAULT_RED)
                buttonStyle = LibraryGUIStyle.buttonDefaultRed;
            else if (buttonType == ButtonType.DEFAULT_WHITE)
                buttonStyle = LibraryGUIStyle.buttonDefaultWhite;
            else if (buttonType == ButtonType.SUB_MENU2)
                buttonStyle = LibraryGUIStyle.buttonSubMenu2;
            else if (buttonType == ButtonType.DEFAULT_GREEN)
                buttonStyle = LibraryGUIStyle.buttonDefaultGreen;
            else if (buttonType == ButtonType.DEFAULT_MIDDLE_LEFT)
                buttonStyle = LibraryGUIStyle.buttonDefaultMiddleLeft;
            else if (buttonType == ButtonType.DEFAULT_ACTIVE)
                buttonStyle = LibraryGUIStyle.buttonDefaultActive;
            else
                buttonStyle = LibraryGUIStyle.buttonDefault;

            GUI.enabled = enable;
            if (GUILayout.Button(icon, buttonStyle, GUILayout.Width(width), GUILayout.Height(height)))
            {
                onPress.Invoke();
            }
            GUI.enabled = true;
        }

        public static void CreateObjectField(string label, 
                                             UnityEngine.Object obj, 
                                             Type typeOf, 
                                             Action<UnityEngine.Object> actionOnChange, 
                                             float width = 0, 
                                             float labelWidth = -1, 
                                             bool skipGroup = false, 
                                             bool blackText = false,
                                             bool skipLabel = false)
        {
            if(!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            if(!skipLabel)
            {
                if (labelWidth == -1)
                    GUILayout.Label(label, blackText ? LibraryGUIStyle.textDefaultBlack : LibraryGUIStyle.textDefault);
                else
                    GUILayout.Label(label, blackText ? LibraryGUIStyle.textDefaultBlack : LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            }
            GUI.SetNextControlName(label);
            UnityEngine.Object newO;

            if (width == 0)
                newO = EditorGUILayout.ObjectField(obj, typeOf, true);
            else
                newO = EditorGUILayout.ObjectField(obj, typeOf, true, GUILayout.Width(width));

            if (!skipGroup) GUILayout.EndHorizontal();

            if (obj != newO) actionOnChange.Invoke(newO);

            if (GUI.GetNameOfFocusedControl() == label && Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete)
                Event.current.Use();
        }

        public static void CreateXYZInputFields(string label, 
                                                Vector3 currentValue, 
                                                Action<Vector3, Vector3> actionOnValueChange, 
                                                float labelWidth, 
                                                float xyzLabelWidth, 
                                                float inputFieldWidth, 
                                                bool disable = false, 
                                                bool skipGroup = false)
        {
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Label($"<b>{label}</b>", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            Vector3 oldValue = currentValue;

            currentValue.x = Mathf.Round(currentValue.x * 100) / 100;
            currentValue.y = Mathf.Round(currentValue.y * 100) / 100;
            currentValue.z = Mathf.Round(currentValue.z * 100) / 100;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            EditorGUIUtility.labelWidth = xyzLabelWidth;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.white;
            EditorStyles.label.focused.textColor = Color.white;
            GUI.enabled = !disable;
            GUI.SetNextControlName("field_x");
            currentValue.x = EditorGUILayout.FloatField("X", currentValue.x, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUILayout.Space(2);
            GUI.SetNextControlName("field_y");
            currentValue.y = EditorGUILayout.FloatField("Y", currentValue.y, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUILayout.Space(2);
            GUI.SetNextControlName("field_z");
            currentValue.z = EditorGUILayout.FloatField("Z", currentValue.z, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUI.enabled = true;

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;

            if (!skipGroup) GUILayout.EndHorizontal();

            currentValue.x = Mathf.Round(currentValue.x * 100) / 100;
            currentValue.y = Mathf.Round(currentValue.y * 100) / 100;
            currentValue.z = Mathf.Round(currentValue.z * 100) / 100;

            oldValue.x = Mathf.Round(oldValue.x * 100) / 100;
            oldValue.y = Mathf.Round(oldValue.y * 100) / 100;
            oldValue.z = Mathf.Round(oldValue.z * 100) / 100;

            if (PositionTool.activePart == PositionTool.Part.NONE && (GUI.GetNameOfFocusedControl() == "field_x" ||
                                                                            GUI.GetNameOfFocusedControl() == "field_y" ||
                                                                            GUI.GetNameOfFocusedControl() == "field_z"))
            {
                if (!GeneralUtility.IsEqual(oldValue, currentValue))
                {
                    actionOnValueChange.Invoke(currentValue, oldValue - currentValue);
                }
            }
        }

        public static void CreateXYInputDelayedFields(string label,
                                        Vector2 currentValue,
                                        Action<Vector2, Vector2> actionOnValueChange,
                                        float labelWidth,
                                        float xyLabelWidth,
                                        float inputFieldWidth,
                                        bool leftMouseUp,
                                        bool disable = false,
                                        bool skipGroup = false,
                                        bool realtimeLabelDrag = false)
        {
            Vector2 newValue;
            Vector2 fieldValue = currentValue;
            bool pendigDif = false;
            Rect lastRect = new Rect();
            GUIStyle guiStyleX = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle guiStyleY = LibraryGUIStyle.textFieldDelayedNoWidth;
            string xyControler = $"{label} fields";

            if (isPending && GUI.GetNameOfFocusedControl().Contains(label) && !GeneralUtility.IsEqual(pendingVector2, currentValue))
            {
                if (GUI.GetNameOfFocusedControl() == $"{label} field_x")
                    guiStyleX = LibraryGUIStyle.textFieldPendingNoWidth;

                if (GUI.GetNameOfFocusedControl() == $"{label} field_y")
                    guiStyleY = LibraryGUIStyle.textFieldPendingNoWidth;

                fieldValue = pendingVector2;
                pendigDif = true;
            }

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Label($"<b>{label}</b>", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            Vector2 oldValue = currentValue;

            currentValue.x = Mathf.Round(currentValue.x * 100) / 100;
            currentValue.y = Mathf.Round(currentValue.y * 100) / 100;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            EditorGUIUtility.labelWidth = xyLabelWidth;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.white;
            EditorStyles.label.focused.textColor = Color.white;
            GUI.enabled = !disable;
            GUI.SetNextControlName($"{label} field_x");
            newValue.x = EditorGUILayout.FloatField("X", fieldValue.x, guiStyleX, GUILayout.Width(inputFieldWidth));
            if(GUI.GetNameOfFocusedControl() == $"{label} field_x") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(2);
            GUI.SetNextControlName($"{label} field_y");
            newValue.y = EditorGUILayout.FloatField("Y", fieldValue.y, guiStyleY, GUILayout.Width(inputFieldWidth));
            if (GUI.GetNameOfFocusedControl() == $"{label} field_y") lastRect = GUILayoutUtility.GetLastRect();

            if (pendigDif && EditorGUIUtility.editingTextField)
            {
                lastRect = new Rect(new Vector2(lastRect.position.x + Mathf.RoundToInt(lastRect.size.x / 2) - 8, lastRect.position.y - lastRect.size.y + 10), new Vector2(45, 30));
                GUI.Label(lastRect, "Enter", LibraryGUIStyle.pressEnter);
            }

            GUI.enabled = true;

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;

            if (!skipGroup) GUILayout.EndHorizontal();

            newValue.x = Mathf.Round(newValue.x * 100) / 100;
            newValue.y = Mathf.Round(newValue.y * 100) / 100;

            oldValue.x = Mathf.Round(oldValue.x * 100) / 100;
            oldValue.y = Mathf.Round(oldValue.y * 100) / 100;

            if (PositionTool.activePart == PositionTool.Part.NONE && GUI.GetNameOfFocusedControl().Contains(label))
            {
                Event e = Event.current;
                //Dragging on the X or Y label gives the field both hot and keyboard control without editing the text.
                bool labelDragging = GUIUtility.hotControl != 0 && GUIUtility.hotControl == GUIUtility.keyboardControl && !EditorGUIUtility.editingTextField;
                bool realtimeDragEnded = realtimeDragControlName == xyControler && leftMouseUp && !EditorGUIUtility.editingTextField;
                if (realtimeDragEnded) realtimeDragControlName = null;

                if (realtimeLabelDrag && labelDragging && !GeneralUtility.IsEqual(oldValue, newValue))
                {
                    isPending = false;
                    realtimeDragControlName = xyControler;
                    actionOnValueChange.Invoke(newValue, oldValue - newValue);
                    WindowBase.RepaintAll();
                }
                else if (!GeneralUtility.IsEqual(oldValue, newValue) &&
                    (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (!EditorGUIUtility.editingTextField && leftMouseUp)))
                {
                    actionOnValueChange.Invoke(newValue, oldValue - newValue);
                    GUI.FocusControl(null);
                    WindowBase.RepaintAll();
                }
                //Realtime label drag done, the value is already applied.
                else if (realtimeDragEnded)
                {
                    isPending = false;
                    GUI.FocusControl(null);
                }
                else
                {
                    isPending = true;
                    pendingVector2 = newValue;
                }
            }
        }

        public static void CreateXYZInputDelayedFields(string label,
                                        Vector3 currentValue,
                                        Action<Vector3, Vector3> actionOnValueChange,
                                        float labelWidth,
                                        float xyzLabelWidth,
                                        float inputFieldWidth,
                                        bool leftMouseUp,
                                        bool disable = false,
                                        bool skipGroup = false,
                                        bool realtimeLabelDrag = false)
        {
            Vector3 newValue;
            Vector3 fieldValue = currentValue;
            bool pendigDif = false;
            Rect lastRect = new Rect();
            GUIStyle guiStyleX = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle guiStyleY = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle guiStyleZ = LibraryGUIStyle.textFieldDelayedNoWidth;
            string xyzControler = $"{label} fields";

            if (isPending && GUI.GetNameOfFocusedControl().Contains(label) && !GeneralUtility.IsEqual(pendingVector3, currentValue))
            {
                if (GUI.GetNameOfFocusedControl() == $"{label} field_x")
                    guiStyleX = LibraryGUIStyle.textFieldPendingNoWidth;

                if (GUI.GetNameOfFocusedControl() == $"{label} field_y")
                    guiStyleY = LibraryGUIStyle.textFieldPendingNoWidth;

                if (GUI.GetNameOfFocusedControl() == $"{label} field_z")
                    guiStyleZ = LibraryGUIStyle.textFieldPendingNoWidth;

                fieldValue = pendingVector3;
                pendigDif = true;
            }

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Label($"<b>{label}</b>", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            Vector3 oldValue = currentValue;

            currentValue.x = Mathf.Round(currentValue.x * 100) / 100;
            currentValue.y = Mathf.Round(currentValue.y * 100) / 100;
            currentValue.z = Mathf.Round(currentValue.z * 100) / 100;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            EditorGUIUtility.labelWidth = xyzLabelWidth;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.white;
            EditorStyles.label.focused.textColor = Color.white;
            GUI.enabled = !disable;
            GUI.SetNextControlName($"{label} field_x");
            newValue.x = EditorGUILayout.FloatField("X", fieldValue.x, guiStyleX, GUILayout.Width(inputFieldWidth));
            if(GUI.GetNameOfFocusedControl() == $"{label} field_x") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(2);
            GUI.SetNextControlName($"{label} field_y");
            newValue.y = EditorGUILayout.FloatField("Y", fieldValue.y, guiStyleY, GUILayout.Width(inputFieldWidth));
            if (GUI.GetNameOfFocusedControl() == $"{label} field_y") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(2);
            GUI.SetNextControlName($"{label} field_z");
            newValue.z = EditorGUILayout.FloatField("Z", fieldValue.z, guiStyleZ, GUILayout.Width(inputFieldWidth));
            if (GUI.GetNameOfFocusedControl() == $"{label} field_z") lastRect = GUILayoutUtility.GetLastRect();

            if (pendigDif && EditorGUIUtility.editingTextField)
            {
                lastRect = new Rect(new Vector2(lastRect.position.x + Mathf.RoundToInt(lastRect.size.x / 2) - 8, lastRect.position.y - lastRect.size.y + 10), new Vector2(45, 30));
                GUI.Label(lastRect, "Enter", LibraryGUIStyle.pressEnter);
            }

            GUI.enabled = true;

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;

            if (!skipGroup) GUILayout.EndHorizontal();

            newValue.x = Mathf.Round(newValue.x * 100) / 100;
            newValue.y = Mathf.Round(newValue.y * 100) / 100;
            newValue.z = Mathf.Round(newValue.z * 100) / 100;

            oldValue.x = Mathf.Round(oldValue.x * 100) / 100;
            oldValue.y = Mathf.Round(oldValue.y * 100) / 100;
            oldValue.z = Mathf.Round(oldValue.z * 100) / 100;

            if (PositionTool.activePart == PositionTool.Part.NONE && GUI.GetNameOfFocusedControl().Contains(label))
            {
                Event e = Event.current;
                //Dragging on the X, Y or Z label gives the field both hot and keyboard control without editing the text.
                bool labelDragging = GUIUtility.hotControl != 0 && GUIUtility.hotControl == GUIUtility.keyboardControl && !EditorGUIUtility.editingTextField;
                bool realtimeDragEnded = realtimeDragControlName == xyzControler && leftMouseUp && !EditorGUIUtility.editingTextField;
                if (realtimeDragEnded) realtimeDragControlName = null;

                if (realtimeLabelDrag && labelDragging && !GeneralUtility.IsEqual(oldValue, newValue))
                {
                    isPending = false;
                    realtimeDragControlName = xyzControler;
                    actionOnValueChange.Invoke(newValue, oldValue - newValue);
                    WindowBase.RepaintAll();
                }
                else if (!GeneralUtility.IsEqual(oldValue, newValue) &&
                    (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (!EditorGUIUtility.editingTextField && leftMouseUp)))
                {
                    actionOnValueChange.Invoke(newValue, oldValue - newValue);
                    GUI.FocusControl(null);
                    WindowBase.RepaintAll();
                }
                //Realtime label drag done, the value is already applied.
                else if (realtimeDragEnded)
                {
                    isPending = false;
                    GUI.FocusControl(null);
                }
                else
                {
                    isPending = true;
                    pendingVector3 = newValue;
                }
            }
        }

        public static void CreateXYZFromToFields(string label,
                                         Vector3 currentFrom,
                                         Vector3 currentTo,
                                         Action<Vector3, Vector3> actionOnValueChange,
                                         float labelWidth,
                                         float inputFieldWidth,
                                         bool disable = false,
                                         bool skipGroup = false)
        {
            Vector3 newFromValue;
            Vector3 newToValue;

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Label($"<b>{label}</b>", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            Vector3 oldFromValue = currentFrom;
            currentFrom.x = Mathf.Round(currentFrom.x * 100) / 100;
            currentFrom.y = Mathf.Round(currentFrom.y * 100) / 100;
            currentFrom.z = Mathf.Round(currentFrom.z * 100) / 100;

            Vector3 oldToValue = currentTo;
            currentTo.x = Mathf.Round(currentTo.x * 100) / 100;
            currentTo.y = Mathf.Round(currentTo.y * 100) / 100;
            currentTo.z = Mathf.Round(currentTo.z * 100) / 100;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.white;
            EditorStyles.label.focused.textColor = Color.white;
            GUI.enabled = !disable;

            // X
            GUI.SetNextControlName($"{label} from_field_x");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.x = EditorGUILayout.FloatField("X", currentFrom.x, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_x");
            EditorGUIUtility.labelWidth = 4;
            newToValue.x = EditorGUILayout.FloatField("-", currentTo.x, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth - 4));

            GUILayout.Space(3);

            // Y
            GUI.SetNextControlName($"{label} from_field_y");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.y = EditorGUILayout.FloatField("Y", currentFrom.y, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_y");
            EditorGUIUtility.labelWidth = 4;
            newToValue.y = EditorGUILayout.FloatField("-", currentTo.y, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth - 4));

            GUILayout.Space(3);

            // Z
            GUI.SetNextControlName($"{label} from_field_z");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.z = EditorGUILayout.FloatField("Z", currentFrom.z, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth));
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_z");
            EditorGUIUtility.labelWidth = 4;
            newToValue.z = EditorGUILayout.FloatField("-", currentTo.z, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(inputFieldWidth - 4));

            GUI.enabled = true;
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;

            if (!skipGroup) GUILayout.EndHorizontal();

            newFromValue.x = Mathf.Round(newFromValue.x * 100) / 100;
            newFromValue.y = Mathf.Round(newFromValue.y * 100) / 100;
            newFromValue.z = Mathf.Round(newFromValue.z * 100) / 100;
            oldFromValue.x = Mathf.Round(oldFromValue.x * 100) / 100;
            oldFromValue.y = Mathf.Round(oldFromValue.y * 100) / 100;
            oldFromValue.z = Mathf.Round(oldFromValue.z * 100) / 100;

            newToValue.x = Mathf.Round(newToValue.x * 100) / 100;
            newToValue.y = Mathf.Round(newToValue.y * 100) / 100;
            newToValue.z = Mathf.Round(newToValue.z * 100) / 100;
            oldToValue.x = Mathf.Round(oldToValue.x * 100) / 100;
            oldToValue.y = Mathf.Round(oldToValue.y * 100) / 100;
            oldToValue.z = Mathf.Round(oldToValue.z * 100) / 100;

            if (PositionTool.activePart == PositionTool.Part.NONE && GUI.GetNameOfFocusedControl().Contains(label))
            {
                if (!GeneralUtility.IsEqual(oldFromValue, newFromValue) || !GeneralUtility.IsEqual(oldToValue, newToValue))
                {
                    actionOnValueChange.Invoke(newFromValue, newToValue);
                }
            }
        }

        public static void CreateXYZFromToDelayedFields(string label,
                                         Vector3 currentFrom,
                                         Vector3 currentTo,
                                         Action<Vector3, Vector3> actionOnValueChange,
                                        float labelWidth,
                                        float inputFieldWidth,
                                        bool leftMouseUp,
                                        bool disable = false,
                                        bool skipGroup = false)
        {
            Vector3 newFromValue;
            Vector3 fromFieldValue = currentFrom;
            GUIStyle fromGuiStyleX = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle fromGuiStyleY = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle fromGuiStyleZ = LibraryGUIStyle.textFieldDelayedNoWidth;

            Vector3 newToValue;
            Vector3 toFieldValue = currentTo;
            GUIStyle toGuiStyleX = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle toGuiStyleY = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle toGuiStyleZ = LibraryGUIStyle.textFieldDelayedNoWidth;

            bool pendigDif = false;
            Rect lastRect = new Rect();

            if (isPending && GUI.GetNameOfFocusedControl().Contains(label) && !GeneralUtility.IsEqual(pendingVector3, currentFrom))
            {
                fromFieldValue = pendingVector3;
                pendigDif = true;
            }
            if (isPending && GUI.GetNameOfFocusedControl().Contains(label) && !GeneralUtility.IsEqual(pendingVector4, currentTo))
            {
                toFieldValue = pendingVector4;
                pendigDif = true;
            }

            if(pendigDif)
            {
                if (GUI.GetNameOfFocusedControl() == $"{label} from_field_x") fromGuiStyleX = LibraryGUIStyle.textFieldPendingNoWidth;
                if (GUI.GetNameOfFocusedControl() == $"{label} from_field_y") fromGuiStyleY = LibraryGUIStyle.textFieldPendingNoWidth;
                if (GUI.GetNameOfFocusedControl() == $"{label} from_field_z") fromGuiStyleZ = LibraryGUIStyle.textFieldPendingNoWidth;
                if (GUI.GetNameOfFocusedControl() == $"{label} to_field_x") toGuiStyleX = LibraryGUIStyle.textFieldPendingNoWidth;
                if (GUI.GetNameOfFocusedControl() == $"{label} to_field_y") toGuiStyleY = LibraryGUIStyle.textFieldPendingNoWidth;
                if (GUI.GetNameOfFocusedControl() == $"{label} to_field_z") toGuiStyleZ = LibraryGUIStyle.textFieldPendingNoWidth;
            }

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Label($"<b>{label}</b>", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            Vector3 oldFromValue = currentFrom;
            currentFrom.x = Mathf.Round(currentFrom.x * 100) / 100;
            currentFrom.y = Mathf.Round(currentFrom.y * 100) / 100;
            currentFrom.z = Mathf.Round(currentFrom.z * 100) / 100;

            Vector3 oldToValue = currentTo;
            currentTo.x = Mathf.Round(currentTo.x * 100) / 100;
            currentTo.y = Mathf.Round(currentTo.y * 100) / 100;
            currentTo.z = Mathf.Round(currentTo.z * 100) / 100;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.white;
            EditorStyles.label.focused.textColor = Color.white;
            GUI.enabled = !disable;

            // X
            GUI.SetNextControlName($"{label} from_field_x");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.x = EditorGUILayout.FloatField("X", fromFieldValue.x, fromGuiStyleX, GUILayout.Width(inputFieldWidth));
            if(GUI.GetNameOfFocusedControl() == $"{label} from_field_x") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_x");
            EditorGUIUtility.labelWidth = 4;
            newToValue.x = EditorGUILayout.FloatField("-", toFieldValue.x, toGuiStyleX, GUILayout.Width(inputFieldWidth - 4));
            if(GUI.GetNameOfFocusedControl() == $"{label} to_field_x") lastRect = GUILayoutUtility.GetLastRect();

            GUILayout.Space(3);

            // Y
            GUI.SetNextControlName($"{label} from_field_y");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.y = EditorGUILayout.FloatField("Y", fromFieldValue.y, fromGuiStyleY, GUILayout.Width(inputFieldWidth));
            if(GUI.GetNameOfFocusedControl() == $"{label} from_field_y") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_y");
            EditorGUIUtility.labelWidth = 4;
            newToValue.y = EditorGUILayout.FloatField("-", toFieldValue.y, toGuiStyleY, GUILayout.Width(inputFieldWidth - 4));
            if(GUI.GetNameOfFocusedControl() == $"{label} to_field_y") lastRect = GUILayoutUtility.GetLastRect();
        
            GUILayout.Space(3);

            // Z
            GUI.SetNextControlName($"{label} from_field_z");
            EditorGUIUtility.labelWidth = 8;
            newFromValue.z = EditorGUILayout.FloatField("Z", fromFieldValue.z, fromGuiStyleZ, GUILayout.Width(inputFieldWidth));
            if(GUI.GetNameOfFocusedControl() == $"{label} from_field_z") lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(-3);
            GUI.SetNextControlName($"{label} to_field_z");
            EditorGUIUtility.labelWidth = 4;
            newToValue.z = EditorGUILayout.FloatField("-", toFieldValue.z, toGuiStyleZ, GUILayout.Width(inputFieldWidth - 4));
            if(GUI.GetNameOfFocusedControl() == $"{label} to_field_z") lastRect = GUILayoutUtility.GetLastRect();

            if (pendigDif && EditorGUIUtility.editingTextField)
            {
                lastRect = new Rect(new Vector2(lastRect.position.x + Mathf.RoundToInt(lastRect.size.x / 2) - 8, lastRect.position.y - lastRect.size.y + 10), new Vector2(45, 30));
                GUI.Label(lastRect, "Enter", LibraryGUIStyle.pressEnter);
            }

            GUI.enabled = true;
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;

            if (!skipGroup) GUILayout.EndHorizontal();

            newFromValue.x = Mathf.Round(newFromValue.x * 100) / 100;
            newFromValue.y = Mathf.Round(newFromValue.y * 100) / 100;
            newFromValue.z = Mathf.Round(newFromValue.z * 100) / 100;
            oldFromValue.x = Mathf.Round(oldFromValue.x * 100) / 100;
            oldFromValue.y = Mathf.Round(oldFromValue.y * 100) / 100;
            oldFromValue.z = Mathf.Round(oldFromValue.z * 100) / 100;

            newToValue.x = Mathf.Round(newToValue.x * 100) / 100;
            newToValue.y = Mathf.Round(newToValue.y * 100) / 100;
            newToValue.z = Mathf.Round(newToValue.z * 100) / 100;
            oldToValue.x = Mathf.Round(oldToValue.x * 100) / 100;
            oldToValue.y = Mathf.Round(oldToValue.y * 100) / 100;
            oldToValue.z = Mathf.Round(oldToValue.z * 100) / 100;

            if (PositionTool.activePart == PositionTool.Part.NONE && GUI.GetNameOfFocusedControl().Contains(label))
            {
                Event e = Event.current;

                if ((!GeneralUtility.IsEqual(oldFromValue, newFromValue) || !GeneralUtility.IsEqual(oldToValue, newToValue)) && 
                    (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (!EditorGUIUtility.editingTextField && leftMouseUp)))
                {
                    actionOnValueChange.Invoke(newFromValue, newToValue);
                    GUI.FocusControl(null);
                    WindowBase.RepaintAll();
                }
                else
                {
                    isPending = true;
                    pendingVector3 = newFromValue;
                    pendingVector4 = newToValue;
                }
            }
        }

        public static void CreateFromToInputField(string label, 
                                                  float currentValueFrom, 
                                                  float currentValueTo, 
                                                  Action<float, float> actionOnValueChange, 
                                                  float width = 58, 
                                                  float paddingLeft = 6, 
                                                  float labelWidth = 58, 
                                                  bool skipGroup = false, 
                                                  bool verySmall = false)
        {
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Space(paddingLeft);

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            int oldFontSize = EditorStyles.label.fontSize;
            bool oldRichText = EditorStyles.label.richText;

            EditorStyles.label.normal.textColor = LibraryGUIStyle.textNoWdith.normal.textColor;
            EditorStyles.label.hover.textColor = LibraryGUIStyle.textNoWdith.normal.textColor;
            EditorStyles.label.focused.textColor = LibraryGUIStyle.textNoWdith.normal.textColor;
            EditorStyles.label.fontSize = LibraryGUIStyle.textNoWdith.fontSize;
            EditorStyles.label.richText = LibraryGUIStyle.textNoWdith.richText;

            GUIStyle fieldStyle = verySmall ? LibraryGUIStyle.textFieldVerySmall : LibraryGUIStyle.textFieldSmall;
            EditorGUIUtility.labelWidth = labelWidth;
            float newValueFrom = EditorGUILayout.FloatField(label, currentValueFrom, fieldStyle, GUILayout.Width(width + labelWidth));
            EditorGUIUtility.labelWidth = 6;
            float newValueTo = EditorGUILayout.FloatField("-", currentValueTo, fieldStyle, GUILayout.Width(width + EditorGUIUtility.labelWidth));

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;
            EditorStyles.label.fontSize = oldFontSize;
            EditorStyles.label.richText = oldRichText;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (GeneralUtility.IsEqual(newValueFrom, currentValueFrom) && GeneralUtility.IsEqual(newValueTo, currentValueTo))
                return;
            
            actionOnValueChange.Invoke(newValueFrom, newValueTo);
        }

        public static void CreateSliderAndInputField(string label, 
                                                     float currentValue, 
                                                     Action<float, bool> actionOnValueChange, 
                                                     float sliderLeftValue, 
                                                     float sliderRightValue, 
                                                     float sliderWidth, 
                                                     float textFieldWidth, 
                                                     float labelWidth = 0, 
                                                     bool skipGroup = false, 
                                                     bool enable = true)
        {
            bool changeBySlider = false;

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUI.enabled = enable;
            if (labelWidth == 0)
                GUILayout.Label(label, LibraryGUIStyle.textDefault);
            else
                GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            float oldValue = currentValue;
            float newValue = GUILayout.HorizontalSlider(currentValue, sliderLeftValue, sliderRightValue, GUILayout.Width(sliderWidth));

            //If slider changed value, stop focusing field
            if (!GeneralUtility.IsEqual(oldValue, newValue))
            {
                changeBySlider = true;
                GUI.FocusControl(null);
            }

            else newValue = EditorGUILayout.FloatField(newValue, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(textFieldWidth));

            GUI.enabled = true;
            if (!skipGroup) GUILayout.EndHorizontal();

            newValue = Mathf.Round(newValue * 100) / 100;
            if (GeneralUtility.IsEqual(newValue, oldValue))
                return;

            actionOnValueChange.Invoke(newValue, changeBySlider);
        }

        public static void CreateSliderAndDelayedInputField(string label,
                                                             float currentValue,
                                                             Action<float, bool> actionOnValueChange,
                                                             float sliderLeftValue,
                                                             float sliderRightValue,
                                                             bool leftMouseUp,
                                                             float textFieldWidth,
                                                             float labelWidth = 0,
                                                             bool skipGroup = false,
                                                             bool enable = true,
                                                             bool roundToInt = false,
                                                             bool realtimeSlider = false)
        {
            float fieldValue = currentValue;
            bool pendingDif = false;
            GUIStyle guiStyle = LibraryGUIStyle.textFieldDelayedNoWidth;
            bool changeBySlider = false;

            // Update pendding values
            if (isPending && !GeneralUtility.IsEqual(pendingFloat, currentValue) 
                && (GUI.GetNameOfFocusedControl() == label || horizontalSliderId == label))
            {
                guiStyle = LibraryGUIStyle.textFieldPendingNoWidth;
                fieldValue = pendingFloat;
                pendingDif = true;
            }

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUI.enabled = enable;

            // Label
            if (labelWidth == 0) GUILayout.Label(label, LibraryGUIStyle.textDefault);
            else GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            // Slider
            float newValue = GUILayout.HorizontalSlider(fieldValue, sliderLeftValue, sliderRightValue);

            //If slider changed value, stop focusing field
            if (!GeneralUtility.IsEqual(fieldValue, newValue))
            {
                changeBySlider = true;
                horizontalSliderId = label;
                GUI.FocusControl(null);
            }

            GUI.SetNextControlName(label);
            newValue = EditorGUILayout.FloatField(newValue, guiStyle, GUILayout.Width(textFieldWidth));

            // Enter info box
            if (pendingDif && EditorGUIUtility.editingTextField)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                rect = new Rect(new Vector2(rect.position.x + rect.size.x - (textFieldWidth / 2) - 14, rect.position.y - rect.size.y + 10), new Vector2(45, 30));
                GUI.Label(rect, "Enter", LibraryGUIStyle.pressEnter);
            }

            GUI.enabled = true;
            if (!skipGroup) GUILayout.EndHorizontal();

            Event e = Event.current;
            newValue = Mathf.Round(newValue * 100) / 100;
            if(roundToInt) newValue = Mathf.Round(newValue);

            bool realtimeDragEnded = realtimeDragControlName == label && leftMouseUp;
            if (realtimeDragEnded) realtimeDragControlName = null;

            //Slider is dragged, the value is applied in realtime. Invoked as not changed by slider becouse the value should be fully applied.
            if (realtimeSlider && changeBySlider && GUIUtility.hotControl != 0 && !GeneralUtility.IsEqual(newValue, currentValue))
            {
                isPending = false;
                realtimeDragControlName = label;
                actionOnValueChange.Invoke(newValue, false);
                WindowBase.RepaintAll();
            }
            else if (!GeneralUtility.IsEqual(newValue, currentValue) && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (leftMouseUp && !EditorGUIUtility.editingTextField)))
            {
                isPending = false;
                actionOnValueChange.Invoke(newValue, changeBySlider);
                GUI.FocusControl(null);
                WindowBase.RepaintAll();
                horizontalSliderId = "";
            }
            //Realtime slider drag done, the value is already applied.
            else if (realtimeDragEnded)
            {
                isPending = false;
                horizontalSliderId = "";
            }
            else if (GUI.GetNameOfFocusedControl() == label || horizontalSliderId == label)
            {
                isPending = true;
                pendingFloat = newValue;
            }
        }

        public static void CreateLabelField(string label, GUIStyle guiStyle, 
                                                          bool skipGroup = false, 
                                                          float labelWidth = -1)
        {
            if(!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            if(labelWidth == -1) GUILayout.Label(label, guiStyle);
            else GUILayout.Label(label, guiStyle, GUILayout.Width(labelWidth));
            if (!skipGroup) GUILayout.EndHorizontal();
        }

        public static void CreateMaskField(string label,
                                                   LayerMask currentValue,
                                                   string[] options,
                                                   Action<LayerMask> actionOnValueChange,
                                                   bool enable = true,
                                                   bool skipGroup = false,
                                                   float labelWidth = 0,
                                                   float width = 0)
        {
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUI.enabled = enable;

            if (labelWidth == 0) GUILayout.Label(label, LibraryGUIStyle.textDefault);
            else GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            int newMask = EditorGUILayout.MaskField(currentValue.value, options, LibraryGUIStyle.popUpFieldSmallText, GUILayout.Width(width));

            GUI.enabled = true;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (newMask != currentValue.value)
            {
                actionOnValueChange.Invoke(newMask);
            }
        }

        public static void CreateToggleField(string label, 
                                             bool currentValue, 
                                             Action<bool> actionOnValueChange, 
                                             bool enable = true, 
                                             bool skipGroup = false, 
                                             float labelWidth = 0, 
                                             float width = 0)
        {
            bool oldValue = currentValue;
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUI.enabled = enable;
            if(label.Length > 0)
            {
                if (labelWidth == 0) GUILayout.Label(label, LibraryGUIStyle.textDefault);
                else GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            }
            if(width == 0) currentValue = EditorGUILayout.Toggle(currentValue);
            else currentValue = EditorGUILayout.Toggle(currentValue, GUILayout.Width(width));

            GUI.enabled = true;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (currentValue == oldValue)
                return;

            actionOnValueChange.Invoke(currentValue);
        }

        public static void CreateToggleXYZField(string label, 
                                                Vector3Int currentValue, 
                                                Action<Vector3Int> actionOnValueChange, 
                                                float paddingLeft = 6, 
                                                bool skipGroup = false)
        {
            Vector3Int oldValue = currentValue;

            //Follow axels
            if(!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUILayout.Space(paddingLeft);
            GUILayout.Label(label, LibraryGUIStyle.textNoWdith);
            GUILayout.Label("X", LibraryGUIStyle.specificX);
            currentValue.x = EditorGUILayout.Toggle(currentValue.x != 0) ? 1 : 0;

            GUILayout.Label("Y", LibraryGUIStyle.specificYZ);
            currentValue.y = EditorGUILayout.Toggle(currentValue.y != 0) ? 1 : 0;

            GUILayout.Label("Z", LibraryGUIStyle.specificYZ);
            currentValue.z = EditorGUILayout.Toggle(currentValue.z != 0) ? 1 : 0;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (currentValue == oldValue)
                return;

            actionOnValueChange.Invoke(currentValue);
        }

        //True while a value is changed by dragging on the label of a field with realtimeLabelDrag enabled or a slider with realtimeSlider enabled.
        public static bool IsRealtimeDragging()
        {
            return realtimeDragControlName != null && GUIUtility.hotControl != 0;
        }

        public static void CreateDelayedFloatFieldWithLabel(string label,
                                                            float currentValue,
                                                            Action<float> actionOnValueChange,
                                                            bool leftMouseUp,
                                                            float width = -1,
                                                            float labelWidth = -1,
                                                            bool skipGroup = false,
                                                            bool enable = true,
                                                            string controllerName = null,
                                                            bool realtimeLabelDrag = false)
        {
            float newValue;
            float fieldValue = currentValue;
            bool pendigDif = false;
            GUIStyle guiStyle = LibraryGUIStyle.textFieldDelayedNoWidth;

            string cName = controllerName == null ? label : controllerName;

            if (isPending && GUI.GetNameOfFocusedControl() == cName && !GeneralUtility.IsEqual(pendingFloat, currentValue))
            {
                guiStyle = LibraryGUIStyle.textFieldPendingNoWidth;
                fieldValue = pendingFloat;
                pendigDif = true;
            }

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            int oldFontSize = EditorStyles.label.fontSize;
            bool oldRichText = EditorStyles.label.richText;
            bool oldEnabled = GUI.enabled;

            if (labelWidth != -1) EditorGUIUtility.labelWidth = labelWidth;
            EditorStyles.label.normal.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.hover.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.focused.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.fontSize = LibraryGUIStyle.textDefault.fontSize;
            EditorStyles.label.richText = LibraryGUIStyle.textDefault.richText;
            GUI.enabled = oldEnabled && enable;

            GUI.SetNextControlName(cName);
            if (width == -1) newValue = EditorGUILayout.FloatField(label, fieldValue, guiStyle);
            else newValue = EditorGUILayout.FloatField(label, fieldValue, guiStyle, GUILayout.Width(width + EditorGUIUtility.labelWidth));

            if (pendigDif && EditorGUIUtility.editingTextField)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.xMin += EditorGUIUtility.labelWidth;
                rect = new Rect(new Vector2(rect.position.x + Mathf.RoundToInt(rect.size.x / 2) - 14, rect.position.y - rect.size.y + 10), new Vector2(45, 30));
                GUI.Label(rect, "Enter", LibraryGUIStyle.pressEnter);
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;
            EditorStyles.label.fontSize = oldFontSize;
            EditorStyles.label.richText = oldRichText;
            GUI.enabled = oldEnabled;
            if (!skipGroup) GUILayout.EndHorizontal();

            Event e = Event.current;
            bool focused = GUI.GetNameOfFocusedControl() == cName;
            //Dragging on the label gives the field both hot and keyboard control without editing the text.
            bool labelDragging = GUIUtility.hotControl != 0 && GUIUtility.hotControl == GUIUtility.keyboardControl && !EditorGUIUtility.editingTextField;

            if (realtimeLabelDrag && focused && labelDragging && !GeneralUtility.IsEqual(newValue, currentValue))
            {
                isPending = false;
                realtimeDragControlName = cName;
                actionOnValueChange.Invoke(newValue);
                WindowBase.RepaintAll();
            }
            //Realtime label drag done, the value is already applied.
            else if (realtimeDragControlName == cName && focused && leftMouseUp && !EditorGUIUtility.editingTextField)
            {
                isPending = false;
                realtimeDragControlName = null;
                GUI.FocusControl(null);
            }
            else if (focused && !GeneralUtility.IsEqual(newValue, currentValue) &&
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (!EditorGUIUtility.editingTextField && leftMouseUp)))
            {
                isPending = false;
                actionOnValueChange.Invoke(newValue);
                GUI.FocusControl(null);
                WindowBase.RepaintAll();
            }
            else if (focused)
            {
                isPending = true;
                pendingFloat = newValue;
            }
        }

        public static void CreateFloatFieldWithLabel(string label,
                                             float currentValue,
                                             Action<float> actionOnValueChange,
                                             float width = -1,
                                             float labelWidth = -1,
                                             bool skipGroup = false,
                                             bool enable = true)
        {
            float newValue;

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            Color oldNormal = EditorStyles.label.normal.textColor;
            Color oldHover = EditorStyles.label.hover.textColor;
            Color oldFocused = EditorStyles.label.focused.textColor;
            int oldFontSize = EditorStyles.label.fontSize;
            bool oldRichText = EditorStyles.label.richText;
            bool oldEnabled = GUI.enabled;

            if (labelWidth != -1) EditorGUIUtility.labelWidth = labelWidth;
            EditorStyles.label.normal.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.hover.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.focused.textColor = LibraryGUIStyle.textDefault.normal.textColor;
            EditorStyles.label.fontSize = LibraryGUIStyle.textDefault.fontSize;
            EditorStyles.label.richText = LibraryGUIStyle.textDefault.richText;
            GUI.enabled = oldEnabled && enable;

            if (width == -1) newValue = EditorGUILayout.FloatField(label, currentValue, LibraryGUIStyle.textFieldNoWidth);
            else newValue = EditorGUILayout.FloatField(label, currentValue, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(width + EditorGUIUtility.labelWidth));

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorStyles.label.normal.textColor = oldNormal;
            EditorStyles.label.hover.textColor = oldHover;
            EditorStyles.label.focused.textColor = oldFocused;
            EditorStyles.label.fontSize = oldFontSize;
            EditorStyles.label.richText = oldRichText;
            GUI.enabled = oldEnabled;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (GeneralUtility.IsEqual(newValue, currentValue))
                return;

            actionOnValueChange.Invoke(newValue);
        }

        public static void CreateFloatField(float currentValue, Action<float> actionOnValueChange, 
                                                                float width = -1, 
                                                                bool skipGroup = false)
        {
            float newValue;
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            if (width == -1) newValue = EditorGUILayout.FloatField(currentValue, LibraryGUIStyle.textFieldNoWidth);
            else newValue = EditorGUILayout.FloatField(currentValue, LibraryGUIStyle.textFieldNoWidth, GUILayout.Width(width));
            if (!skipGroup) GUILayout.EndHorizontal();

            if (GeneralUtility.IsEqual(newValue, currentValue))
                return;

            actionOnValueChange.Invoke(newValue);
        }

        public static void CreateDelayedFloatField(string controlName, float currentValue,
                                                                       Action<float> actionOnValueChange,
                                                                       bool leftMouseUp,
                                                                       float width = -1,
                                                                       bool skipGroup = false,
                                                                       bool realtimeLabelDrag = false)
        {
            CreateDelayedFloatFieldWithLabel(" ", currentValue, actionOnValueChange,
                leftMouseUp, width, 20, skipGroup, true, controlName, realtimeLabelDrag);
        }

        public static void CreatePopupField(string label, 
                                            float width, 
                                            int currentType, 
                                            string[] options, 
                                            Action<int> actionOnValueChange, 
                                            float labelWidth = -1, 
                                            bool skipGroup = false, 
                                            bool enable = true, 
                                            bool skipLabel = false, 
                                            bool blackText = false)
        {
            int oldType = currentType;

            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            GUI.enabled = enable;
            if(!skipLabel)
            {
                if(labelWidth == -1)
                    GUILayout.Label(label, blackText ? LibraryGUIStyle.textDefaultBlack : LibraryGUIStyle.textDefault);
                else
                    GUILayout.Label(label, blackText ? LibraryGUIStyle.textDefaultBlack : LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            }
            currentType = EditorGUILayout.Popup(currentType, options, LibraryGUIStyle.popUpFieldSmallText, GUILayout.Width(width));
            GUI.enabled = true;
            if (!skipGroup) GUILayout.EndHorizontal();

            if (currentType == oldType)
                return;

            actionOnValueChange.Invoke(currentType);
        }

        public static void CreateMinMaxSlider(string label, 
                                              ref float minValue, 
                                              ref float maxValue, 
                                              float minLimit, 
                                              float maxLimit, 
                                              float labelWidth, 
                                              float sliderWidth = -1, 
                                              bool skipGroup = false)
        {
            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());
            string startValue = Mathf.Round(minValue * 100).ToString();
            string endValue = Mathf.Round(maxValue * 100).ToString();

            GUILayout.Label($"{label} {startValue}", LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));
            if(sliderWidth == -1)
                EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            else
                EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit, GUILayout.Width(sliderWidth));
            GUILayout.Label(endValue, LibraryGUIStyle.textDefault, GUILayout.Width(30));
            if (!skipGroup) GUILayout.EndHorizontal();
        }

        public static void CreateMinMaxSliderDelayedFields(string label,
                                                           int index,
                                                      ref float minValue,
                                                      ref float maxValue,
                                                      float minLimit,
                                                      float maxLimit,
                                                      Action<bool> actionOnValueChange,
                                                      float fieldWidth,
                                                      bool leftMouseUp,
                                                      bool skipGroup = false,
                                                      bool skipLabel = true,
                                                      float labelWidth = 0,
                                                      bool realtimeSlider = false,
                                                      bool enable = false)
        {
            float fieldMinValue = minValue * 100;
            float fieldMaxValue = maxValue * 100;
            GUIStyle guiStyleMin = LibraryGUIStyle.textFieldDelayedNoWidth;
            GUIStyle guiStyleMax = LibraryGUIStyle.textFieldDelayedNoWidth;
            bool changedBySlider = false;
            bool pendingDif = false;
            Rect lastRect = new Rect();

            string minFieldControler = $"{label} {index} min field";
            string maxFieldControler = $"{label} {index} max field";
            string sliderControler = $"{label} {index} slider";

            // Update pendding values
            if (isPending)
            {
                if (!GeneralUtility.IsEqual(pendingFloat, minValue) && (GUI.GetNameOfFocusedControl() == minFieldControler || minMaxSliderControlerId == index))
                {
                    guiStyleMin = LibraryGUIStyle.textFieldPendingNoWidth;
                    fieldMinValue = pendingFloat * 100;
                    pendingDif = true;
                }

                if (!GeneralUtility.IsEqual(pendingFloat2, maxValue) && (GUI.GetNameOfFocusedControl() == maxFieldControler || minMaxSliderControlerId == index))
                {
                    guiStyleMax = LibraryGUIStyle.textFieldPendingNoWidth;
                    fieldMaxValue = pendingFloat2 * 100;
                    pendingDif = true;
                }
            }


            if (!skipGroup) GUILayout.BeginHorizontal(GetBackgroundStyle());

            if(!skipLabel) GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            GUI.enabled = enable;
            GUI.SetNextControlName(minFieldControler);
            fieldMinValue = EditorGUILayout.FloatField(fieldMinValue, guiStyleMin, GUILayout.Width(fieldWidth));
            fieldMinValue = fieldMinValue / 100;
            if (!GeneralUtility.IsEqual(fieldMinValue, minValue)) lastRect = GUILayoutUtility.GetLastRect();

            fieldMaxValue = fieldMaxValue / 100;
            float oldMax = fieldMaxValue;
            float oldMin = fieldMinValue;
            EditorGUILayout.MinMaxSlider(ref fieldMinValue, ref fieldMaxValue, minLimit, maxLimit);


            if (!GeneralUtility.IsEqual(oldMin, fieldMinValue) || !GeneralUtility.IsEqual(oldMax, fieldMaxValue))
            {
                changedBySlider = true;
                GUI.FocusControl(null);
                minMaxSliderControlerId = index;
            }

            GUI.SetNextControlName(maxFieldControler);
            fieldMaxValue = fieldMaxValue * 100;
            fieldMaxValue = EditorGUILayout.FloatField(fieldMaxValue, guiStyleMax, GUILayout.Width(fieldWidth));
            fieldMaxValue = fieldMaxValue / 100;

            if (!GeneralUtility.IsEqual(fieldMaxValue, maxValue)) lastRect = GUILayoutUtility.GetLastRect();
            GUI.enabled = true;

            // Enter info box
            if (pendingDif && EditorGUIUtility.editingTextField)
            {
                lastRect = new Rect(new Vector2(lastRect.position.x + lastRect.size.x - (fieldWidth / 2) - 14, lastRect.position.y - lastRect.size.y + 10), new Vector2(45, 30));
                GUI.Label(lastRect, "Enter", LibraryGUIStyle.pressEnter);
            }

            if (!skipGroup) GUILayout.EndHorizontal();

            Event e = Event.current;
            bool foundChange = !GeneralUtility.IsEqual(fieldMinValue, minValue) || !GeneralUtility.IsEqual(fieldMaxValue, maxValue);

            bool realtimeDragEnded = realtimeDragControlName == sliderControler && leftMouseUp;
            if (realtimeDragEnded) realtimeDragControlName = null;

            //Slider is dragged, the values are applied in realtime. Invoked as not changed by slider becouse the values should be fully applied.
            if (realtimeSlider && changedBySlider && GUIUtility.hotControl != 0 && foundChange)
            {
                isPending = false;
                realtimeDragControlName = sliderControler;
                minValue = Mathf.Round(fieldMinValue * 10000) / 10000;
                maxValue = Mathf.Round(fieldMaxValue * 10000) / 10000;
                actionOnValueChange.Invoke(false);
                WindowBase.RepaintAll();
            }
            else if (foundChange && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || (leftMouseUp && !EditorGUIUtility.editingTextField)))
            {
                isPending = false;
                minValue = Mathf.Round(fieldMinValue * 10000) / 10000;
                maxValue = Mathf.Round(fieldMaxValue * 10000) / 10000;
                actionOnValueChange.Invoke(changedBySlider);
                GUI.FocusControl(null);
                WindowBase.RepaintAll();
                minMaxSliderControlerId = -1;
            }
            //Realtime slider drag done, the values are already applied.
            else if (realtimeDragEnded)
            {
                isPending = false;
                minMaxSliderControlerId = -1;
            }
            else if (GUI.GetNameOfFocusedControl() == minFieldControler || GUI.GetNameOfFocusedControl() == maxFieldControler || changedBySlider)
            {
                isPending = true;
                pendingFloat = fieldMinValue;
                pendingFloat2 = fieldMaxValue;
            }
        }

        public static void CreateCurveField(string label, 
                                            AnimationCurve curve, 
                                            AnimationCurve curveContainer, 
                                            Action<AnimationCurve> actionOnValueChange, 
                                            float width, float labelWidth = 0, 
                                            bool skipGroup = false, bool enable = true)
        {
            if (!skipGroup)
                GUILayout.BeginHorizontal(GetBackgroundStyle());

            if (labelWidth == 0)
                GUILayout.Label(label, LibraryGUIStyle.textDefault);
            else if (labelWidth > 0)
                GUILayout.Label(label, LibraryGUIStyle.textDefault, GUILayout.Width(labelWidth));

            GUI.enabled = enable;

            EditorGUI.BeginChangeCheck();

            if (EHandleUndo.UndoTriggered() || curveContainer.length == 0)
            {
                curveContainer.CopyFrom(curve);
                RefreshOpenCurveEditorWindow();
            }

            EditorGUILayout.CurveField(curveContainer, GUILayout.Width(width));

            GUI.enabled = true;

            if (!skipGroup)
                GUILayout.EndHorizontal();

            if (!EditorGUI.EndChangeCheck())
                return;
            actionOnValueChange.Invoke(curveContainer);
            RefreshOpenCurveEditorWindow();

            void RefreshOpenCurveEditorWindow()
            {
                EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

                for (int i = 0; i < windows.Length; i++)
                {
                    EditorWindow window = windows[i];

                    if (window == null)
                        continue;

                    Type type = window.GetType();

                    if (type.FullName != "UnityEditor.CurveEditorWindow")
                        continue;

                    MethodInfo refreshShownCurvesMethod = GetMethodNoParameters(type, "RefreshShownCurves");

                    if (refreshShownCurvesMethod == null)
                    {
                        Debug.LogError("[Spline Architect] Could not find RefreshShownCurves method, API has changed!");
                        return;
                    }

                    refreshShownCurvesMethod.Invoke(window, null);
                    window.Repaint();
                }

                MethodInfo GetMethodNoParameters(Type type, string methodName)
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    for (int i = 0; i < methods.Length; i++)
                    {
                        if (methods[i].Name != methodName)
                            continue;

                        if (methods[i].GetParameters().Length == 0)
                            return methods[i];
                    }

                    return null;
                }
            }
        }

        public static void CreateErrorWarningMessageIcon(GUIContent guiContent, bool moveBack = false)
        {
            GUILayout.Label(guiContent, LibraryGUIStyle.infoIcon);
            if (moveBack) GUILayout.Space(-6);
        }

        public static void CreateInfoMessageIcon(GUIContent guiContent, bool moveBack = false)
        {
            if(EGlobalSettings.GetInfoIconsVisibility())
            {
                GUILayout.Label(guiContent, LibraryGUIStyle.infoIcon);
                if(moveBack) GUILayout.Space(-6);
            }
        }

        public static void CreateHorizontalYellowLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineYellow, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalBlackLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineBlack, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalWhiteLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineWhite, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalGreyLine80()
        {
            GUILayout.Box("", LibraryGUIStyle.lineGrey80, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalGreyLine30()
        {
            GUILayout.Box("", LibraryGUIStyle.lineGrey30, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalGreyLine20()
        {
            GUILayout.Box("", LibraryGUIStyle.lineGrey20, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalGreyLine40()
        {
            GUILayout.Box("", LibraryGUIStyle.lineGrey40, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void CreateHorizontalSubHeader2Line()
        {
            GUILayout.BeginHorizontal(LibraryGUIStyle.backgroundLayerSection);
            GUILayout.Space(10);
            GUILayout.Box("", LibraryGUIStyle.lineGrey30, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }

        public static void CreateVerticalYellowLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineYellow, GUILayout.Width(1), GUILayout.ExpandHeight(true));
        }

        public static void CreateVerticalBlackLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineBlack, GUILayout.Width(1), GUILayout.ExpandHeight(true));
        }

        public static void CreateVerticalWhiteLine()
        {
            GUILayout.Box("", LibraryGUIStyle.lineWhite, GUILayout.Width(1), GUILayout.ExpandHeight(true));
        }

        public static void CreateVerticalGreyLine80()
        {
            GUILayout.Box("", LibraryGUIStyle.lineGrey80, GUILayout.Width(1), GUILayout.ExpandHeight(true));
        }

        public static void CreateSeparator()
        {
            GUILayout.Box("", LibraryGUIStyle.separatorWhite);
        }

        public static void CreateSpaceWidth(float width)
        {
            GUILayout.Label(LibraryTexture.empty, GUILayout.Width(width));
        }

        public static GUIStyle GetBackgroundStyle(bool keepOld = false)
        {
            if(!keepOld)
                backgroundStyleCounter++;

            if (backgroundStyleCounter % 2 == 0)
                return LibraryGUIStyle.backgroundItem1;
            else
                return LibraryGUIStyle.backgroundItem2;
        }

        public static void ResetGetBackgroundStyleId()
        {
            backgroundStyleCounter = 1;
        }

        public static Vector2 GetWindowAnchorPosition(SceneView sceneView, 
                                                      Rect buttonWorldBound, 
                                                      bool isRow)
        {
            Rect win = sceneView.position;
            Rect wb = buttonWorldBound;
            if (isRow) return new Vector2(win.x + wb.x, win.y + wb.yMax + 5);
            else return new Vector2(win.x + wb.xMax + 5, win.y + wb.y);
        }

        public static bool MouseLeftUpIncludingIgnored()
        {
            Event e = Event.current;
            return (e.type == EventType.MouseUp || (e.rawType == EventType.MouseUp && e.type == EventType.Ignore)) && e.button == 0;
        }
    }
}
