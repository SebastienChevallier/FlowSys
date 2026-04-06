using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    internal static class MissionObjectIdEditorUtility
    {
        internal static List<string> GetAvailableObjectIds()
        {
            MissionObjectRegistrar[] registrars = Resources.FindObjectsOfTypeAll<MissionObjectRegistrar>();
            List<string> ids = new List<string>();

            foreach (MissionObjectRegistrar registrar in registrars)
            {
                if (registrar == null)
                    continue;

                if (EditorUtility.IsPersistent(registrar))
                    continue;

                if (registrar.gameObject.scene.IsValid() == false)
                    continue;

                if (string.IsNullOrWhiteSpace(registrar.objectId))
                    continue;

                ids.Add(registrar.objectId.Trim());
            }

            return ids
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
        }

        internal static void DrawObjectIdPopup(string label, string currentValue, Action<string> onValueChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            string displayText = string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue;
            if (GUILayout.Button(displayText, EditorStyles.popup))
            {
                ShowObjectIdMenu(currentValue, onValueChanged);
            }

            EditorGUILayout.EndHorizontal();
        }

        internal static void DrawObjectIdPopup(Rect rect, SerializedProperty property, GUIContent label)
        {
            Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect buttonRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, label);

            string currentValue = property != null ? property.stringValue : string.Empty;
            string displayText = string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue;
            if (GUI.Button(buttonRect, displayText, EditorStyles.popup))
            {
                ShowObjectIdMenu(currentValue, value =>
                {
                    if (property == null)
                        return;

                    property.stringValue = value;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
        }

        private static void ShowObjectIdMenu(string currentValue, Action<string> onValueChanged)
        {
            GenericMenu menu = new GenericMenu();
            List<string> ids = GetAvailableObjectIds();

            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(currentValue), () => onValueChanged?.Invoke(string.Empty));

            if (ids.Count > 0)
            {
                menu.AddSeparator(string.Empty);
                foreach (string id in ids)
                {
                    string capturedId = id;
                    menu.AddItem(new GUIContent(capturedId), string.Equals(currentValue, capturedId, StringComparison.Ordinal), () => onValueChanged?.Invoke(capturedId));
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("No MissionObjectRegistrar found in open scenes"));
            }

            menu.ShowAsContext();
        }
    }
}
