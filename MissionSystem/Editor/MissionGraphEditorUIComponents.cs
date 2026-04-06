using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    /// <summary>
    /// UI Toolkit components library for Mission Graph Editor.
    /// Provides reusable, styled visual elements for consistent UI rendering.
    /// </summary>
    internal static class MissionGraphEditorUIComponents
    {
        /// <summary>
        /// Creates a foldout card container with header and body sections.
        /// Replaces the IMGUI DrawFoldoutCard pattern.
        /// </summary>
        public static VisualElement CreateFoldoutCard(
            string title,
            Color cardColor,
            Color borderColor,
            bool isExpanded,
            Action<bool> onExpandedChanged,
            Action drawHeaderButtons = null)
        {
            VisualElement card = new VisualElement();
            card.style.marginLeft = 4;
            card.style.marginRight = 4;
            card.style.marginTop = 4;
            card.style.marginBottom = 0;
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderTopColor = borderColor;
            card.style.borderRightColor = borderColor;
            card.style.borderBottomColor = borderColor;
            card.style.borderLeftColor = borderColor;
            card.style.backgroundColor = cardColor;
            card.style.borderTopLeftRadius = 3;
            card.style.borderTopRightRadius = 3;
            card.style.borderBottomLeftRadius = 3;
            card.style.borderBottomRightRadius = 3;
            card.style.overflow = Overflow.Hidden;

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.paddingLeft = 4;
            headerRow.style.paddingRight = 2;
            headerRow.style.paddingTop = 2;
            headerRow.style.paddingBottom = 2;
            headerRow.style.backgroundColor = cardColor;

            Foldout foldout = new Foldout();
            foldout.text = title;
            foldout.value = isExpanded;
            foldout.style.flexGrow = 1;
            foldout.style.unityFontStyleAndWeight = FontStyle.Normal;
            foldout.style.fontSize = 11;
            foldout.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            foldout.RegisterValueChangedCallback(evt => onExpandedChanged?.Invoke(evt.newValue));

            headerRow.Add(foldout);
            drawHeaderButtons?.Invoke();
            // Buttons added by drawHeaderButtons will be added after foldout

            card.Add(headerRow);

            VisualElement body = new VisualElement();
            body.style.paddingLeft = 4;
            body.style.paddingRight = 4;
            body.style.paddingTop = 4;
            body.style.paddingBottom = 4;

            // Store body reference on card for later population
            card.userData = body;

            // Register callback to toggle body visibility when foldout changes
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && body.parent == null)
                {
                    // Expanded - add body
                    card.Add(body);
                }
                else if (!evt.newValue && body.parent == card)
                {
                    // Collapsed - remove body
                    card.Remove(body);
                }
            });

            if (isExpanded)
                card.Add(body);

            return card;
        }

        /// <summary>
        /// Creates a collection item card with standard controls (move up/down, delete).
        /// Replaces IMGUI DrawManagedActionCard pattern.
        /// </summary>
        public static VisualElement CreateCollectionItemCard(
            string title,
            Color cardColor,
            Color borderColor,
            bool isExpanded,
            Action<bool> onExpandedChanged,
            Action<VisualElement> onHeaderButtonsNeeded,
            int itemIndex,
            int totalItems,
            Action onMoveUp,
            Action onMoveDown,
            Action onDelete)
        {
            VisualElement card = new VisualElement();
            card.style.marginLeft = 4;
            card.style.marginRight = 4;
            card.style.marginTop = 4;
            card.style.marginBottom = 0;
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderTopColor = borderColor;
            card.style.borderRightColor = borderColor;
            card.style.borderBottomColor = borderColor;
            card.style.borderLeftColor = borderColor;
            card.style.backgroundColor = cardColor;
            card.style.borderTopLeftRadius = 3;
            card.style.borderTopRightRadius = 3;
            card.style.borderBottomLeftRadius = 3;
            card.style.borderBottomRightRadius = 3;

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.paddingLeft = 4;
            headerRow.style.paddingRight = 2;
            headerRow.style.paddingTop = 2;
            headerRow.style.paddingBottom = 2;
            headerRow.style.backgroundColor = cardColor;

            VisualElement body = new VisualElement();
            body.style.paddingLeft = 4;
            body.style.paddingRight = 4;
            body.style.paddingTop = 4;
            body.style.paddingBottom = 4;

            // Store body reference on card for later population
            card.userData = body;

            Foldout foldout = new Foldout();
            foldout.text = title;
            foldout.value = isExpanded;
            foldout.style.flexGrow = 1;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            foldout.style.fontSize = 11;
            foldout.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            foldout.RegisterValueChangedCallback(evt =>
            {
                // Toggle body visibility
                if (evt.newValue && body.parent == null)
                {
                    // Expanded - add body after header
                    card.Add(body);
                }
                else if (!evt.newValue && body.parent == card)
                {
                    // Collapsed - remove body
                    card.Remove(body);
                }
                onExpandedChanged?.Invoke(evt.newValue);
            });

            headerRow.Add(foldout);

            // Move up button
            Button upButton = CreateIconButton("↑", onMoveUp, 24, false, itemIndex > 0);
            headerRow.Add(upButton);

            // Move down button
            Button downButton = CreateIconButton("↓", onMoveDown, 24, false, itemIndex < totalItems - 1);
            headerRow.Add(downButton);

            // Delete button
            Button deleteButton = CreateIconButton("X", onDelete, 24, true, true);
            headerRow.Add(deleteButton);

            card.Add(headerRow);

            // Add body to card if expanded
            if (isExpanded)
                card.Add(body);

            return card;
        }

        /// <summary>
        /// Creates a section header with title and optional add button.
        /// </summary>
        public static VisualElement CreateSectionHeader(
            string title,
            Color headerColor,
            Color textColor,
            int fontSize,
            Action onAddClicked = null)
        {
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.backgroundColor = headerColor;
            headerRow.style.paddingLeft = 6;
            headerRow.style.paddingRight = 4;
            headerRow.style.paddingTop = 3;
            headerRow.style.paddingBottom = 3;
            headerRow.style.marginTop = 3;
            headerRow.style.borderTopLeftRadius = 4;
            headerRow.style.borderTopRightRadius = 4;

            Label titleLabel = new Label(title);
            titleLabel.style.flexGrow = 1;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = fontSize;
            titleLabel.style.color = textColor;

            headerRow.Add(titleLabel);

            if (onAddClicked != null)
            {
                Button addButton = CreateIconButton("+", onAddClicked, 24, false, true);
                headerRow.Add(addButton);
            }

            return headerRow;
        }

        /// <summary>
        /// Creates a styled icon button with standard sizing and colors.
        /// </summary>
        public static Button CreateIconButton(
            string text,
            Action onClick,
            float width,
            bool isDelete,
            bool isInteractive)
        {
            Button button = new Button(() =>
            {
                if (isInteractive)
                    onClick?.Invoke();
            })
            {
                text = text
            };

            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            Color baseColor = isDelete
                ? (settings != null ? settings.deleteButtonColor : new Color(0.8f, 0.2f, 0.2f, 1f))
                : (settings != null ? settings.addButtonColor : new Color(0.2f, 0.7f, 0.2f, 1f));
            Color textColor = isDelete
                ? (settings != null ? settings.deleteButtonTextColor : Color.white)
                : (settings != null ? settings.addButtonTextColor : Color.white);

            button.style.width = width;
            button.style.height = 20;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.backgroundColor = baseColor;
            button.style.color = textColor;
            button.style.borderTopWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.borderTopLeftRadius = 2;
            button.style.borderTopRightRadius = 2;
            button.style.borderBottomLeftRadius = 2;
            button.style.borderBottomRightRadius = 2;
            button.style.marginLeft = 2;
            button.style.marginRight = 0;
            button.pickingMode = isInteractive ? PickingMode.Position : PickingMode.Ignore;
            button.SetEnabled(isInteractive);

            return button;
        }

        /// <summary>
        /// Creates a styled toggle field with label.
        /// </summary>
        public static VisualElement CreateToggleField(
            string label,
            bool initialValue,
            Action<bool> onValueChanged,
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

            Toggle toggle = new Toggle();
            toggle.value = initialValue;
            toggle.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            row.Add(labelElement);
            row.Add(toggle);

            return row;
        }

        /// <summary>
        /// Creates a styled text field with label.
        /// </summary>
        public static VisualElement CreateTextField(
            string label,
            string initialValue,
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

            TextField textField = new TextField();
            textField.value = initialValue;
            textField.style.flexGrow = 1;
            textField.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            row.Add(labelElement);
            row.Add(textField);

            return row;
        }

        /// <summary>
        /// Creates a styled float field with label.
        /// </summary>
        public static VisualElement CreateFloatField(
            string label,
            float initialValue,
            Action<float> onValueChanged,
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

            FloatField floatField = new FloatField();
            floatField.value = initialValue;
            floatField.style.flexGrow = 1;
            floatField.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            row.Add(labelElement);
            row.Add(floatField);

            return row;
        }

        /// <summary>
        /// Creates an enum popup field.
        /// </summary>
        public static VisualElement CreateEnumField<T>(
            string label,
            T initialValue,
            Action<T> onValueChanged,
            Color textColor,
            int fontSize) where T : System.Enum
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

            EnumField enumField = new EnumField(initialValue);
            enumField.style.flexGrow = 1;
            enumField.RegisterValueChangedCallback(evt => onValueChanged?.Invoke((T)evt.newValue));

            row.Add(labelElement);
            row.Add(enumField);

            return row;
        }

        /// <summary>
        /// Creates a slider field.
        /// </summary>
        public static VisualElement CreateSliderField(
            string label,
            float initialValue,
            float minValue,
            float maxValue,
            Action<float> onValueChanged,
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

            Slider slider = new Slider(minValue, maxValue);
            slider.value = initialValue;
            slider.style.flexGrow = 1;
            slider.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            row.Add(labelElement);
            row.Add(slider);

            return row;
        }

        /// <summary>
        /// Creates an integer field.
        /// </summary>
        public static VisualElement CreateIntField(
            string label,
            int initialValue,
            Action<int> onValueChanged,
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

            IntegerField intField = new IntegerField();
            intField.value = initialValue;
            intField.style.flexGrow = 1;
            intField.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));

            row.Add(labelElement);
            row.Add(intField);

            return row;
        }

        /// <summary>
        /// Creates an empty state label for empty collections.
        /// </summary>
        public static Label CreateEmptyStateLabel(string text, Color textColor, int fontSize)
        {
            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Italic;
            label.style.color = textColor;
            label.style.fontSize = fontSize;
            label.style.paddingLeft = 8;
            label.style.paddingRight = 8;
            label.style.paddingTop = 6;
            label.style.paddingBottom = 4;
            return label;
        }

        /// <summary>
        /// Gets the body container from a foldout card for adding content.
        /// </summary>
        public static VisualElement GetCardBody(VisualElement card)
        {
            if (card?.userData is VisualElement body)
                return body;
            return null;
        }

        /// <summary>
        /// Calculates border color from node background color.
        /// </summary>
        public static Color GetBorderColor(Color nodeBackgroundColor)
        {
            return Color.Lerp(nodeBackgroundColor, Color.black, 0.55f);
        }

        /// <summary>
        /// Creates a settings button with icon.
        /// Replaces IMGUI DrawSettingsButton pattern.
        /// </summary>
        public static Button CreateSettingsButton(
            string label,
            Action onClicked,
            Color textColor,
            int fontSize)
        {
            Button button = new Button(onClicked)
            {
                text = label
            };
            button.style.color = textColor;
            button.style.fontSize = fontSize;
            button.style.paddingLeft = 4;
            button.style.paddingRight = 4;
            button.style.paddingTop = 2;
            button.style.paddingBottom = 2;
            return button;
        }

        /// <summary>
        /// Creates a section header label (bold, larger).
        /// Replaces IMGUI DrawSectionHeader pattern.
        /// </summary>
        public static Label CreateSectionHeaderLabel(
            string text,
            Color textColor)
        {
            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = textColor;
            label.style.fontSize = 12;
            label.style.marginTop = 4;
            label.style.marginBottom = 4;
            return label;
        }

        /// <summary>
        /// Creates a localization key field with popup selector.
        /// Replaces IMGUI DrawLocalizationKeyField pattern.
        /// </summary>
        public static VisualElement CreateLocalizationKeyField(
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

            Button popupButton = new Button(() => ShowLocalizationKeyMenu(currentValue, onValueChanged))
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
        /// Shows localization key selector window for key selection.
        /// </summary>
        private static void ShowLocalizationKeyMenu(string currentValue, Action<string> onValueChanged)
        {
            LocalizationKeySelectorWindow.Show(
                LocalizationReferenceUtility.GetTableName(currentValue),
                currentValue,
                selectedValue =>
                {
                    onValueChanged?.Invoke(selectedValue);
                });
        }
    }
}
