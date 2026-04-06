using System;
using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    internal static class MissionGraphEditorFieldUtility
    {
        internal static bool ApplyChange(bool changed, MissionInlineEditorContext context)
        {
            if (!changed)
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        internal static GUIStyle CreateSectionLabelStyle()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.fontSize = settings != null ? Mathf.RoundToInt(settings.parameterFontSize) : 9;
            style.normal.textColor = settings != null ? settings.itemTextColor : Color.white;
            return style;
        }

        internal static float GetButtonWidth()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return settings != null ? settings.buttonSize : 24f;
        }

        internal static int GetActionFontSize()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return settings != null ? Mathf.RoundToInt(settings.actionFontSize) : 11;
        }

        internal static Color GetItemTextColor()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return settings != null ? settings.itemTextColor : Color.white;
        }

        internal static Color GetReactionColor()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            return settings != null ? settings.reactionColor : new Color(0.4f, 0.2f, 0.6f, 1f);
        }

        internal static Color GetBodyColor(MissionStepActionEntry action)
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            if (settings == null)
                return action != null && action.phase == MissionStepActionPhase.OnExit ? new Color(0.6f, 0.3f, 0.2f, 1f) : new Color(0.2f, 0.3f, 0.6f, 1f);
            return action != null && action.phase == MissionStepActionPhase.OnExit ? settings.actionOnExitColor : settings.actionOnEnterColor;
        }

        internal static bool DrawSettingsButton(string label, bool isDelete, float? width = null, bool isInteractive = true)
        {
            return MissionGraphIMGUIStyleUtility.DrawSettingsButton(label, isDelete, width ?? GetButtonWidth(), isInteractive);
        }

        internal static void DrawSectionHeader(string label, Action onAdd = null, string addLabel = "+", float? addButtonWidth = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, CreateSectionLabelStyle());
                if (onAdd != null && DrawSettingsButton(addLabel, false, addButtonWidth ?? GetButtonWidth()))
                    onAdd.Invoke();
            }
        }

        internal static void DrawVoiceOverSelectorField(string label, string currentValue, Action<string> onValueChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            if (GUILayout.Button(string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue, EditorStyles.popup))
            {
                VoiceOverSelectorWindow.Show(currentValue, selectedValue =>
                {
                    onValueChanged?.Invoke(selectedValue);
                });
            }
            EditorGUILayout.EndHorizontal();
        }

        internal static void DrawLocalizationKeyField(string label, string currentValue, Action<string> onValueChanged, string emptyLabel = "None (Click to select)")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            if (GUILayout.Button(LocalizationReferenceUtility.GetDisplayLabel(currentValue, emptyLabel), EditorStyles.popup))
            {
                LocalizationKeySelectorWindow.Show(LocalizationReferenceUtility.GetTableName(currentValue), currentValue, selectedValue =>
                {
                    onValueChanged?.Invoke(selectedValue);
                });
            }
            EditorGUILayout.EndHorizontal();
        }

        internal static string GetTypeMenuLabel(Type type)
        {
            string name = type.Name;
            name = name.Replace("MissionActionData", string.Empty);
            name = name.Replace("MissionConditionData", string.Empty);
            return ObjectNames.NicifyVariableName(name);
        }
    }
}
