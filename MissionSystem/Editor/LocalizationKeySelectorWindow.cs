using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace GAME.MissionSystem.Editor
{
    public class LocalizationKeySelectorWindow : EditorWindow
    {
        private static Action<string> onSelected;
        private static string currentKey;
        private static string targetTable = "UI";

        private string searchTerm = string.Empty;
        private Vector2 scrollPosition;
        private List<SharedTableData.SharedTableEntry> entries = new List<SharedTableData.SharedTableEntry>();
        private List<string> tableNames = new List<string>();

        public static void Show(string tableName, string selectedKey, Action<string> onKeySelected)
        {
            string reference = selectedKey ?? string.Empty;
            string parsedKey = LocalizationReferenceUtility.GetKey(reference);
            string parsedTable = LocalizationReferenceUtility.GetTableName(reference, tableName);

            targetTable = string.IsNullOrEmpty(parsedTable) ? "UI" : parsedTable;
            currentKey = parsedKey;
            onSelected = onKeySelected;

            var window = CreateInstance<LocalizationKeySelectorWindow>();
            window.titleContent = new GUIContent($"Select Localization Key ({targetTable})");
            window.minSize = new Vector2(420, 500);
            window.maxSize = new Vector2(420, 700);
            window.LoadTables();
            window.LoadEntries();
            window.ShowUtility();
        }

        private void LoadTables()
        {
            tableNames = LocalizationEditorSettings.GetStringTableCollections()
                .Where(collection => collection != null && !string.IsNullOrEmpty(collection.TableCollectionName))
                .Select(collection => collection.TableCollectionName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (tableNames.Count == 0)
                tableNames.Add("UI");

            if (!tableNames.Contains(targetTable))
                tableNames.Insert(0, targetTable);
        }

        private void LoadEntries()
        {
            entries.Clear();

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(targetTable);
            if (collection?.SharedData?.Entries == null)
                return;

            entries = collection.SharedData.Entries
                .Where(e => e != null && !string.IsNullOrEmpty(e.Key))
                .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table", GUILayout.Width(50));
            int currentTableIndex = Mathf.Max(0, tableNames.IndexOf(targetTable));
            int selectedTableIndex = EditorGUILayout.Popup(currentTableIndex, tableNames.ToArray());
            string newTable = tableNames.Count > 0 ? tableNames[Mathf.Clamp(selectedTableIndex, 0, tableNames.Count - 1)] : "UI";
            if (!string.Equals(newTable, targetTable, StringComparison.Ordinal))
            {
                targetTable = newTable;
                currentKey = string.Empty;
                LoadEntries();
                titleContent = new GUIContent($"Select Localization Key ({targetTable})");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search", GUILayout.Width(50));
                string newSearch = EditorGUILayout.TextField(searchTerm);
                if (newSearch != searchTerm)
                {
                    searchTerm = newSearch;
                }
            }

            EditorGUILayout.Space(6);

            if (GUILayout.Button("None", EditorStyles.miniButton))
            {
                SelectKey(string.Empty);
                return;
            }

            EditorGUILayout.Space(6);

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox($"No localization entries found in table '{targetTable}'.", MessageType.Warning);
                return;
            }

            IEnumerable<SharedTableData.SharedTableEntry> filtered = entries;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(e => e.Key.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (SharedTableData.SharedTableEntry entry in filtered)
            {
                bool isSelected = string.Equals(currentKey, entry.Key, StringComparison.Ordinal);
                GUIStyle style = isSelected ? EditorStyles.toolbarButton : EditorStyles.miniButton;
                if (GUILayout.Button(entry.Key, style))
                {
                    SelectKey(LocalizationReferenceUtility.BuildReference(targetTable, entry.Key));
                    return;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void SelectKey(string reference)
        {
            currentKey = LocalizationReferenceUtility.GetKey(reference);
            onSelected?.Invoke(reference);
            Close();
        }
    }
}
