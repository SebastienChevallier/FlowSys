using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ShowTextMissionActionData))]
    internal sealed class ShowTextMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowTextMissionActionData data)
                return false;

            bool changed = false;

            EditorGUI.BeginChangeCheck();
            data.textDisplayType = (TextDisplayType)EditorGUILayout.EnumPopup("Display Type", data.textDisplayType);
            data.teleportPlayerOnShowText = EditorGUILayout.Toggle("Move UI To Transform", data.teleportPlayerOnShowText);
            data.showCanvas = EditorGUILayout.Toggle("Show Canvas", data.showCanvas);
            data.showTextPanel = EditorGUILayout.Toggle("Show Text Panel", data.showTextPanel);
            data.showButtonsPanel = EditorGUILayout.Toggle("Show Buttons Panel", data.showButtonsPanel);
            data.enableTypewriterEffect = EditorGUILayout.Toggle("Typewriter Effect", data.enableTypewriterEffect);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            if (data.teleportPlayerOnShowText)
            {
                string previousTransformReferenceId = data.transformReferenceId;
                MissionTransformReferenceUtility.DrawTransformReferenceSelector("Transform Reference", data.transformReferenceId, value => data.transformReferenceId = value);
                if (!string.Equals(previousTransformReferenceId, data.transformReferenceId, StringComparison.Ordinal))
                    changed = true;
            }

            changed |= DrawTextReference(data.textReference);
            changed |= DrawButtons(data);

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        private static bool DrawTextReference(ManagedLocalizedTextReference textReference)
        {
            if (textReference == null)
                return false;

            bool changed = false;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            textReference.textSourceMode = (TextSourceMode)EditorGUILayout.EnumPopup("Source", textReference.textSourceMode);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            if (textReference.textSourceMode == TextSourceMode.PlainText)
            {
                EditorGUI.BeginChangeCheck();
                textReference.textContent = EditorGUILayout.TextArea(textReference.textContent ?? string.Empty, GUILayout.MinHeight(48f));
                if (EditorGUI.EndChangeCheck())
                    changed = true;
            }
            else
            {
                string currentReference = BuildLocalizationReference(textReference);
                string previousReference = currentReference;
                MissionGraphEditorFieldUtility.DrawLocalizationKeyField("Localization Key", currentReference, value => ApplyLocalizationReference(textReference, value));
                currentReference = BuildLocalizationReference(textReference);
                if (!string.Equals(previousReference, currentReference, StringComparison.Ordinal))
                    changed = true;
            }

            return changed;
        }

        private static bool DrawButtons(ShowTextMissionActionData data)
        {
            bool changed = false;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);

            ManagedMissionUIButtonItem[] items = data.uiButtonItems ?? Array.Empty<ManagedMissionUIButtonItem>();
            for (int i = 0; i < items.Length; i++)
            {
                ManagedMissionUIButtonItem item = items[i] ?? new ManagedMissionUIButtonItem();
                items[i] = item;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Button {i + 1}", EditorStyles.boldLabel);
                        if (GUILayout.Button("X", GUILayout.Width(24f)))
                        {
                            data.uiButtonItems = RemoveAt(items, i);
                            return true;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    item.buttonType = (MissionUIButtonType)EditorGUILayout.EnumPopup("Type", item.buttonType);
                    item.visible = EditorGUILayout.Toggle("Visible", item.visible);
                    item.label = EditorGUILayout.TextField("Label", item.label);
                    item.userActionId = EditorGUILayout.TextField("User Action ID", item.userActionId);
                    if (EditorGUI.EndChangeCheck())
                        changed = true;
                }
            }

            if (GUILayout.Button("Add Button"))
            {
                Array.Resize(ref items, items.Length + 1);
                items[items.Length - 1] = new ManagedMissionUIButtonItem();
                data.uiButtonItems = items;
                changed = true;
            }
            else
            {
                data.uiButtonItems = items;
            }

            return changed;
        }

        private static ManagedMissionUIButtonItem[] RemoveAt(ManagedMissionUIButtonItem[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source ?? Array.Empty<ManagedMissionUIButtonItem>();

            ManagedMissionUIButtonItem[] result = new ManagedMissionUIButtonItem[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, result, 0, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            return result;
        }

        private static string BuildLocalizationReference(ManagedLocalizedTextReference textReference)
        {
            if (textReference == null || textReference.localizedText == null)
                return string.Empty;

            return LocalizationReferenceUtility.BuildReference(textReference.localizedText.TableReference, textReference.localizedText.TableEntryReference);
        }

        private static void ApplyLocalizationReference(ManagedLocalizedTextReference textReference, string value)
        {
            if (textReference == null)
                return;

            if (textReference.localizedText == null)
                textReference.localizedText = new UnityEngine.Localization.LocalizedString();

            string table = LocalizationReferenceUtility.GetTableName(value);
            string key = LocalizationReferenceUtility.GetKey(value);
            textReference.localizedText.TableReference = table;
            textReference.localizedText.TableEntryReference = key;
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowTextMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Display Type",
                data.textDisplayType,
                value =>
                {
                    data.textDisplayType = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Move UI To Transform",
                data.teleportPlayerOnShowText,
                value =>
                {
                    data.teleportPlayerOnShowText = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            if (data.teleportPlayerOnShowText)
            {
                root.Add(MissionGraphEditorUIUtility.CreateTransformReferencePopupField(
                    "Transform Reference",
                    data.transformReferenceId,
                    value =>
                    {
                        data.transformReferenceId = value;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    paramTextColor,
                    paramFontSize));
            }

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Show Canvas",
                data.showCanvas,
                value =>
                {
                    data.showCanvas = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Show Text Panel",
                data.showTextPanel,
                value =>
                {
                    data.showTextPanel = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Show Buttons Panel",
                data.showButtonsPanel,
                value =>
                {
                    data.showButtonsPanel = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Typewriter Effect",
                data.enableTypewriterEffect,
                value =>
                {
                    data.enableTypewriterEffect = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            // Text reference section
            if (data.textReference != null)
            {
                root.Add(BuildTextReferenceElement(data.textReference, context, paramTextColor, paramFontSize));
            }

            // Buttons collection section
            ManagedMissionUIButtonItem[] buttons = data.uiButtonItems ?? Array.Empty<ManagedMissionUIButtonItem>();
            if (buttons.Length == 0)
            {
                root.Add(MissionGraphEditorCollectionComponent.CreateEmptyCollectionState(
                    "No buttons defined",
                    () =>
                    {
                        ManagedMissionUIButtonItem[] current = data.uiButtonItems ?? Array.Empty<ManagedMissionUIButtonItem>();
                        Array.Resize(ref current, current.Length + 1);
                        current[current.Length - 1] = new ManagedMissionUIButtonItem();
                        data.uiButtonItems = current;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    paramTextColor,
                    paramFontSize));
            }
            else
            {
                root.Add(MissionGraphEditorCollectionComponent.CreateCollectionSection(
                    "Buttons",
                    buttons.Length,
                    addIndex =>
                    {
                        ManagedMissionUIButtonItem[] current = data.uiButtonItems ?? Array.Empty<ManagedMissionUIButtonItem>();
                        Array.Resize(ref current, current.Length + 1);
                        current[current.Length - 1] = new ManagedMissionUIButtonItem();
                        data.uiButtonItems = current;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    itemIndex =>
                    {
                        ManagedMissionUIButtonItem buttonItem = buttons[itemIndex];
                        return BuildButtonItemElement(buttonItem, itemIndex, buttons, data, context, paramTextColor, paramFontSize);
                    },
                    paramTextColor,
                    paramFontSize));
            }

            return root;
        }

        private static VisualElement BuildTextReferenceElement(
            ManagedLocalizedTextReference textReference,
            MissionInlineEditorContext context,
            Color textColor,
            int fontSize)
        {
            VisualElement container = new VisualElement();
            container.style.marginTop = 8;

            Label header = MissionGraphEditorUIComponents.CreateSectionHeaderLabel("Text", textColor);
            container.Add(header);

            container.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Source",
                textReference.textSourceMode,
                value =>
                {
                    textReference.textSourceMode = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                textColor,
                fontSize));

            if (textReference.textSourceMode == TextSourceMode.PlainText)
            {
                container.Add(MissionGraphEditorUIComponents.CreateTextField(
                    "Text",
                    textReference.textContent ?? string.Empty,
                    value =>
                    {
                        textReference.textContent = value;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    textColor,
                    fontSize));
            }
            else
            {
                string currentReference = BuildLocalizationReference(textReference);
                container.Add(MissionGraphEditorUIComponents.CreateLocalizationKeyField(
                    "Localization Key",
                    currentReference,
                    value =>
                    {
                        ApplyLocalizationReference(textReference, value);
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    textColor,
                    fontSize));
            }

            return container;
        }

        private static VisualElement BuildButtonItemElement(
            ManagedMissionUIButtonItem buttonItem,
            int itemIndex,
            ManagedMissionUIButtonItem[] allButtons,
            ShowTextMissionActionData data,
            MissionInlineEditorContext context,
            Color textColor,
            int fontSize)
        {
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.style.marginTop = 4;
            fieldContainer.style.marginBottom = 4;

            fieldContainer.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Type",
                buttonItem.buttonType,
                value =>
                {
                    buttonItem.buttonType = value;
                    data.uiButtonItems = allButtons;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                textColor,
                fontSize));

            fieldContainer.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Visible",
                buttonItem.visible,
                value =>
                {
                    buttonItem.visible = value;
                    data.uiButtonItems = allButtons;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                textColor,
                fontSize));

            fieldContainer.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Label",
                buttonItem.label ?? string.Empty,
                value =>
                {
                    buttonItem.label = value;
                    data.uiButtonItems = allButtons;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                textColor,
                fontSize));

            fieldContainer.Add(MissionGraphEditorUIComponents.CreateTextField(
                "User Action ID",
                buttonItem.userActionId ?? string.Empty,
                value =>
                {
                    buttonItem.userActionId = value;
                    data.uiButtonItems = allButtons;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                textColor,
                fontSize));

            return MissionGraphEditorCollectionComponent.CreateCollectionItemEditor(
                itemIndex,
                "Button",
                fieldContainer,
                () => { /* move up not supported for buttons */ },
                () => { /* move down not supported for buttons */ },
                () =>
                {
                    data.uiButtonItems = RemoveAt(allButtons, itemIndex);
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                false,
                false);
        }
    }
}
