using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.FlowSys.Editor
{
    /// <summary>
    /// UI Toolkit utilities for Mission Graph Editor.
    /// Provides helper methods for building UI Toolkit-based editor panels.
    /// Progressively replaces IMGUI utilities.
    /// </summary>
    internal static class MissionGraphEditorUIUtility
    {
        /// <summary>
        /// Creates a popup field for selecting Object IDs.
        /// Replaces IMGUI DrawObjectIdPopup pattern.
        /// </summary>
        public static VisualElement CreateObjectIdPopupField(
            string label,
            string currentValue,
            Action<string> onValueChanged,
            Color textColor,
            int fontSize)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            Label labelElement = new Label(label);
            labelElement.style.minWidth = 150;
            labelElement.style.color = textColor;
            labelElement.style.fontSize = fontSize;

            Button popupButton = new Button(() => ShowObjectIdMenu(currentValue, onValueChanged))
            {
                text = string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue
            };
            popupButton.style.flexGrow = 1;
            popupButton.style.paddingLeft = 4;
            popupButton.style.paddingRight = 4;
            popupButton.style.color = textColor;
            popupButton.style.fontSize = fontSize;

            row.Add(labelElement);
            row.Add(popupButton);

            return row;
        }

        /// <summary>
        /// Creates a popup field for selecting Voice Over keys.
        /// Replaces IMGUI DrawVoiceOverSelectorField pattern.
        /// </summary>
        public static VisualElement CreateVoiceOverPopupField(
            string label,
            string currentValue,
            Action<string> onValueChanged,
            Color textColor,
            int fontSize)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            Label labelElement = new Label(label);
            labelElement.style.minWidth = 150;
            labelElement.style.color = textColor;
            labelElement.style.fontSize = fontSize;

            Button popupButton = new Button(() => ShowVoiceOverMenu(currentValue, onValueChanged))
            {
                text = string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue
            };
            popupButton.style.flexGrow = 1;
            popupButton.style.paddingLeft = 4;
            popupButton.style.paddingRight = 4;
            popupButton.style.color = textColor;
            popupButton.style.fontSize = fontSize;

            row.Add(labelElement);
            row.Add(popupButton);

            return row;
        }

        /// <summary>
        /// Shows context menu for Object ID selection.
        /// </summary>
        public static void ShowObjectIdMenu(string currentValue, Action<string> onValueChanged)
        {
            GenericMenu menu = new GenericMenu();
            List<string> ids = MissionObjectIdEditorUtility.GetAvailableObjectIds();

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

        /// <summary>
        /// Shows context menu for Voice Over key selection.
        /// </summary>
        public static void ShowVoiceOverMenu(string currentValue, Action<string> onValueChanged)
        {
            VoiceOverSelectorWindow.Show(currentValue, onValueChanged);
        }

        /// <summary>
        /// Creates a foldout card section for a collection of items.
        /// Replaces IMGUI DrawFoldoutCard pattern with full UI Toolkit.
        /// </summary>
        public static VisualElement CreateCollectionSection(
            string sectionTitle,
            int itemCount,
            Color sectionColor,
            Color headerColor,
            Color textColor,
            Color nodeBackgroundColor,
            int fontSize,
            bool isExpanded,
            Action<bool> onExpandedChanged,
            Action onAddClicked,
            Func<VisualElement> buildItemsContent)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = 3;
            section.style.borderTopWidth = 1;
            section.style.borderRightWidth = 1;
            section.style.borderBottomWidth = 1;
            section.style.borderLeftWidth = 1;
            Color borderColor = MissionGraphEditorUIComponents.GetBorderColor(nodeBackgroundColor);
            section.style.borderTopColor = borderColor;
            section.style.borderRightColor = borderColor;
            section.style.borderBottomColor = borderColor;
            section.style.borderLeftColor = borderColor;
            section.style.backgroundColor = sectionColor;
            section.style.paddingBottom = 4;
            section.style.borderTopLeftRadius = 4;
            section.style.borderTopRightRadius = 4;
            section.style.borderBottomLeftRadius = 4;
            section.style.borderBottomRightRadius = 4;

            // Header
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.backgroundColor = headerColor;
            headerRow.style.paddingLeft = 6;
            headerRow.style.paddingRight = 4;
            headerRow.style.paddingTop = 3;
            headerRow.style.paddingBottom = 3;
            headerRow.style.borderTopLeftRadius = 4;
            headerRow.style.borderTopRightRadius = 4;

            Foldout foldout = new Foldout();
            foldout.text = $"{sectionTitle} ({itemCount})";
            foldout.value = isExpanded;
            foldout.style.flexGrow = 1;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.style.fontSize = fontSize;
            foldout.style.color = textColor;
            foldout.RegisterValueChangedCallback(evt => onExpandedChanged?.Invoke(evt.newValue));

            headerRow.Add(foldout);

            if (onAddClicked != null)
            {
                Button addButton = MissionGraphEditorUIComponents.CreateIconButton("+", onAddClicked, 24, false, true);
                headerRow.Add(addButton);
            }

            section.Add(headerRow);

            if (isExpanded)
            {
                VisualElement content = buildItemsContent?.Invoke();
                if (content != null)
                    section.Add(content);
            }

            return section;
        }

        /// <summary>
        /// Gets settings from MissionFlowGraphSettings or returns defaults.
        /// </summary>
        public static MissionFlowGraphSettings GetSettings()
        {
            return MissionFlowGraphSettings.Instance ?? new MissionFlowGraphSettings();
        }

        /// <summary>
        /// Gets item text color from settings.
        /// </summary>
        public static Color GetItemTextColor()
        {
            MissionFlowGraphSettings settings = GetSettings();
            return settings != null ? settings.itemTextColor : Color.white;
        }

        /// <summary>
        /// Gets parameter text color from settings.
        /// </summary>
        public static Color GetParameterTextColor()
        {
            MissionFlowGraphSettings settings = GetSettings();
            return settings != null ? settings.parameterTextColor : Color.white;
        }

        /// <summary>
        /// Gets action font size from settings.
        /// </summary>
        public static int GetActionFontSize()
        {
            MissionFlowGraphSettings settings = GetSettings();
            return settings != null ? Mathf.RoundToInt(settings.actionFontSize) : 11;
        }

        /// <summary>
        /// Gets parameter font size from settings.
        /// </summary>
        public static int GetParameterFontSize()
        {
            MissionFlowGraphSettings settings = GetSettings();
            return settings != null ? Mathf.RoundToInt(settings.parameterFontSize) : 10;
        }

        /// <summary>
        /// Gets button size from settings.
        /// </summary>
        public static float GetButtonSize()
        {
            MissionFlowGraphSettings settings = GetSettings();
            return settings != null ? settings.buttonSize : 24f;
        }

        /// <summary>
        /// Creates a popup field for selecting Transform References.
        /// Replaces IMGUI DrawTransformReferenceSelector pattern.
        /// </summary>
        public static VisualElement CreateTransformReferencePopupField(
            string label,
            string currentValue,
            Action<string> onValueChanged,
            Color textColor,
            int fontSize)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            Label labelElement = new Label(label);
            labelElement.style.minWidth = 150;
            labelElement.style.color = textColor;
            labelElement.style.fontSize = fontSize;

            Button popupButton = new Button(() => ShowTransformReferenceMenu(currentValue, onValueChanged))
            {
                text = string.IsNullOrEmpty(currentValue) ? "None (Click to select)" : currentValue
            };
            popupButton.style.flexGrow = 1;
            popupButton.style.paddingLeft = 4;
            popupButton.style.paddingRight = 4;
            popupButton.style.color = textColor;
            popupButton.style.fontSize = fontSize;

            row.Add(labelElement);
            row.Add(popupButton);

            return row;
        }

        /// <summary>
        /// Shows context menu for Transform Reference selection.
        /// </summary>
        public static void ShowTransformReferenceMenu(string currentValue, Action<string> onValueChanged)
        {
            GenericMenu menu = new GenericMenu();
            string[] ids = MissionTransformReferenceUtility.GetAllTransformReferenceIds();

            if (ids.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("(No MissionTransformReference in scene)"));
                menu.ShowAsContext();
                return;
            }

            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(currentValue), () => onValueChanged?.Invoke(string.Empty));
            menu.AddSeparator("");

            foreach (string id in ids)
            {
                string idCopy = id;
                menu.AddItem(new GUIContent(id), currentValue == id, () => onValueChanged?.Invoke(idCopy));
            }

            menu.ShowAsContext();
        }

    }
}
