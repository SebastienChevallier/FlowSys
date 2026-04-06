using System;
using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    internal static class MissionGraphEditorCollectionUtility
    {
        [System.Obsolete("Use UI Toolkit collection components via MissionGraphEditorCollectionComponent instead", false)]
        internal static bool DrawManagedActionCard(
            MissionStepActionEntry entry,
            Color color,
            bool isExpanded,
            Action<bool> setExpanded,
            Action onMoveUp,
            bool canMoveUp,
            Action onMoveDown,
            bool canMoveDown,
            Action onDelete,
            Action drawBody)
        {
            return MissionManagedActionCardIMGUIRenderer.Draw(
                entry,
                color,
                MissionGraphEditorFieldUtility.GetItemTextColor(),
                MissionGraphEditorFieldUtility.GetActionFontSize(),
                MissionGraphEditorFieldUtility.GetButtonWidth(),
                isExpanded,
                setExpanded,
                onMoveUp,
                canMoveUp,
                onMoveDown,
                canMoveDown,
                onDelete,
                drawBody);
        }

        [System.Obsolete("Use UI Toolkit collection components via MissionGraphEditorCollectionComponent instead", false)]
        internal static bool DrawManagedActionCard(
            MissionStepActionEntry entry,
            bool isExpanded,
            Action<bool> setExpanded,
            Action onMoveUp,
            bool canMoveUp,
            Action onMoveDown,
            bool canMoveDown,
            Action onDelete,
            Action drawBody)
        {
            return DrawManagedActionCard(
                entry,
                MissionGraphEditorFieldUtility.GetReactionColor(),
                isExpanded,
                setExpanded,
                onMoveUp,
                canMoveUp,
                onMoveDown,
                canMoveDown,
                onDelete,
                drawBody);
        }

        internal static GUIStyle CreateNestedCardStyle(Color color)
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return MissionGraphIMGUIStyleUtility.GetPanelStyle("graph_nested_card", color, settings != null ? settings.parameterTextColor : Color.white, new RectOffset(1, 1, 1, 1), new RectOffset(4, 4, 4, 4), new RectOffset(6, 6, 4, 6));
        }

        internal static GUIStyle CreateNestedHeaderStyle(Color color)
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return MissionGraphIMGUIStyleUtility.GetPanelStyle("graph_nested_header", color, settings != null ? settings.itemTextColor : Color.white, new RectOffset(0, 0, 0, 0), new RectOffset(0, 0, 0, 4), new RectOffset(4, 4, 4, 4));
        }

        internal static GUIStyle CreateOutlineStyle()
        {
            return MissionGraphIMGUIStyleUtility.GetOutlineStyle(4, 4, 4, 4, 1, 1, 1, 1);
        }

        internal static GUIStyle CreateFoldoutStyle()
        {
            return MissionGraphIMGUIStyleUtility.GetFoldoutStyle(MissionGraphEditorFieldUtility.GetItemTextColor(), MissionGraphEditorFieldUtility.GetActionFontSize());
        }

        internal static bool DrawFoldoutCard(string title, Color color, bool isExpanded, Action<bool> setExpanded, Action drawBody, Action drawHeaderButtons = null)
        {
            using (new EditorGUILayout.VerticalScope(CreateOutlineStyle()))
            {
                using (new EditorGUILayout.VerticalScope(CreateNestedCardStyle(color)))
                {
                    using (new EditorGUILayout.HorizontalScope(CreateNestedHeaderStyle(color)))
                    {
                        bool newExpanded = EditorGUILayout.Foldout(isExpanded, title, true, CreateFoldoutStyle());
                        if (newExpanded != isExpanded)
                            setExpanded?.Invoke(newExpanded);

                        drawHeaderButtons?.Invoke();
                    }

                    if (isExpanded)
                        drawBody?.Invoke();
                }
            }

            return false;
        }

        internal static T[] RemoveAt<T>(T[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source ?? Array.Empty<T>();

            T[] result = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, result, 0, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            return result;
        }

        internal static T[] Move<T>(T[] source, int fromIndex, int toIndex)
        {
            if (source == null || source.Length == 0)
                return source ?? Array.Empty<T>();
            if (fromIndex < 0 || fromIndex >= source.Length || toIndex < 0 || toIndex >= source.Length || fromIndex == toIndex)
                return source;

            T[] result = new T[source.Length];
            Array.Copy(source, result, source.Length);
            (result[fromIndex], result[toIndex]) = (result[toIndex], result[fromIndex]);
            return result;
        }
    }
}
