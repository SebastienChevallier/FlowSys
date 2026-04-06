using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GAME.MissionSystem.Editor
{
    public class AudioLibrarySoundSelectorWindow : EditorWindow
    {
        private static Action<string> onSelected;
        private static string currentSoundId;

        private string searchTerm = string.Empty;
        private Vector2 scrollPosition;
        private List<string> soundIds = new List<string>();

        public static void Show(string selectedSoundId, Action<string> onSoundSelected)
        {
            currentSoundId = selectedSoundId ?? string.Empty;
            onSelected = onSoundSelected;

            var window = CreateInstance<AudioLibrarySoundSelectorWindow>();
            window.titleContent = new GUIContent("Select AudioManager Sound");
            window.minSize = new Vector2(420, 500);
            window.maxSize = new Vector2(420, 700);
            window.LoadSoundIds();
            window.ShowUtility();
        }

        private void LoadSoundIds()
        {
            soundIds.Clear();

            string[] guids = AssetDatabase.FindAssets("t:AudioLibrary");
            HashSet<string> uniqueSoundIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioLibrary library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(path);
                if (library?.sfx == null)
                    continue;

                foreach (SFX sfx in library.sfx)
                {
                    if (sfx?.clip == null || string.IsNullOrEmpty(sfx.clip.name))
                        continue;

                    uniqueSoundIds.Add(sfx.clip.name);
                }
            }

            soundIds = uniqueSoundIds
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("AudioManager Sounds", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search", GUILayout.Width(50));
                searchTerm = EditorGUILayout.TextField(searchTerm);
            }

            EditorGUILayout.Space(6);

            if (GUILayout.Button("None", EditorStyles.miniButton))
            {
                SelectSound(string.Empty);
                return;
            }

            EditorGUILayout.Space(6);

            if (soundIds.Count == 0)
            {
                EditorGUILayout.HelpBox("No AudioLibrary sounds found in the project.", MessageType.Warning);
                return;
            }

            IEnumerable<string> filtered = soundIds;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(id => id.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string soundId in filtered)
            {
                bool isSelected = string.Equals(currentSoundId, soundId, StringComparison.Ordinal);
                GUIStyle style = isSelected ? EditorStyles.toolbarButton : EditorStyles.miniButton;
                if (GUILayout.Button(soundId, style))
                {
                    SelectSound(soundId);
                    return;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void SelectSound(string soundId)
        {
            currentSoundId = soundId;
            onSelected?.Invoke(soundId);
            Close();
        }
    }
}
