using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GAME.MissionSystem
{
    public static class MissionTransformReferenceUtility
    {
        public static List<MissionTransformReference> FindAllTransformReferences()
        {
            return UnityEngine.Object.FindObjectsOfType<MissionTransformReference>()
                .OrderBy(r => r.referenceId)
                .ToList();
        }

        public static string[] GetAllTransformReferenceIds()
        {
            var references = FindAllTransformReferences();
            return references
                .Where(r => !string.IsNullOrEmpty(r.referenceId))
                .Select(r => r.referenceId)
                .ToArray();
        }

        public static MissionTransformReference FindTransformReferenceById(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId))
                return null;

            var references = FindAllTransformReferences();
            return references.FirstOrDefault(r => r.referenceId == referenceId);
        }

        public static void DrawTransformReferenceSelector(SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            string currentValue = property.stringValue;
            string[] allIds = GetAllTransformReferenceIds();

            if (allIds.Length == 0)
            {
                EditorGUILayout.LabelField(label, new GUIContent("(No MissionTransformReference in scene)"));
                EditorGUILayout.EndHorizontal();
                return;
            }

            int currentIndex = System.Array.IndexOf(allIds, currentValue);
            if (currentIndex < 0 && !string.IsNullOrEmpty(currentValue))
            {
                var tempList = new List<string>(allIds);
                tempList.Insert(0, currentValue + " (Missing)");
                allIds = tempList.ToArray();
                currentIndex = 0;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup(label.text, currentIndex, allIds);
            if (newIndex != currentIndex)
            {
                string selectedId = allIds[newIndex];
                if (selectedId.EndsWith(" (Missing)"))
                {
                    selectedId = selectedId.Replace(" (Missing)", "");
                }
                property.stringValue = selectedId;
            }

            if (GUILayout.Button("→", GUILayout.Width(30)))
            {
                MissionTransformReference reference = FindTransformReferenceById(property.stringValue);
                if (reference != null)
                {
                    Selection.activeGameObject = reference.gameObject;
                    EditorGUIUtility.PingObject(reference.gameObject);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawTransformReferenceSelector(string label, string currentValue, Action<string> onValueChanged)
        {
            EditorGUILayout.BeginHorizontal();

            string[] allIds = GetAllTransformReferenceIds();
            if (allIds.Length == 0)
            {
                EditorGUILayout.LabelField(label, "(No MissionTransformReference in scene)");
                EditorGUILayout.EndHorizontal();
                return;
            }

            int currentIndex = Array.IndexOf(allIds, currentValue);
            if (currentIndex < 0 && !string.IsNullOrEmpty(currentValue))
            {
                var tempList = new List<string>(allIds);
                tempList.Insert(0, currentValue + " (Missing)");
                allIds = tempList.ToArray();
                currentIndex = 0;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup(label, currentIndex, allIds);
            if (newIndex != currentIndex)
            {
                string selectedId = allIds[newIndex];
                if (selectedId.EndsWith(" (Missing)"))
                    selectedId = selectedId.Replace(" (Missing)", string.Empty);

                onValueChanged?.Invoke(selectedId);
            }

            if (GUILayout.Button("→", GUILayout.Width(30)))
            {
                MissionTransformReference reference = FindTransformReferenceById(currentValue);
                if (reference != null)
                {
                    Selection.activeGameObject = reference.gameObject;
                    EditorGUIUtility.PingObject(reference.gameObject);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
