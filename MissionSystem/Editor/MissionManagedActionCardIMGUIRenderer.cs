using System;
using UnityEditor;
using UnityEngine;

namespace GAME.MissionSystem.Editor
{
    [System.Obsolete("Use UI Toolkit collection components via MissionGraphEditorCollectionComponent instead", false)]
    internal static class MissionManagedActionCardIMGUIRenderer
    {
        internal static bool Draw(
            MissionStepActionEntry entry,
            Color bodyColor,
            Color itemTextColor,
            int actionFontSize,
            float buttonWidth,
            bool isExpanded,
            Action<bool> setExpanded,
            Action onMoveUp,
            bool canMoveUp,
            Action onMoveDown,
            bool canMoveDown,
            Action onDelete,
            Action drawBody)
        {
            if (entry == null)
                return false;

            using (new EditorGUILayout.VerticalScope(MissionGraphEditorCollectionUtility.CreateNestedCardStyle(bodyColor)))
            {
                using (new EditorGUILayout.HorizontalScope(MissionGraphEditorCollectionUtility.CreateNestedHeaderStyle(bodyColor)))
                {
                    GUIStyle foldoutStyle = MissionGraphEditorCollectionUtility.CreateFoldoutStyle();
                    bool newExpanded = EditorGUILayout.Foldout(isExpanded, entry.managedAction?.GetDisplayName() ?? "(empty action)", true, foldoutStyle);
                    if (newExpanded != isExpanded)
                        setExpanded?.Invoke(newExpanded);

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("↑", false, buttonWidth, canMoveUp))
                        onMoveUp?.Invoke();

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("↓", false, buttonWidth, canMoveDown))
                        onMoveDown?.Invoke();

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("X", true, buttonWidth))
                    {
                        onDelete?.Invoke();
                        return true;
                    }
                }

                if (isExpanded)
                    drawBody?.Invoke();
            }

            return false;
        }
    }
}
