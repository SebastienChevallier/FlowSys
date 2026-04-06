using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ShowMultipleChoiceDialogMissionActionData))]
    internal sealed class ShowMultipleChoiceDialogMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowMultipleChoiceDialogMissionActionData data)
                return false;

            bool changed = false;

            EditorGUI.BeginChangeCheck();
            data.correctChoiceIndex = EditorGUILayout.IntField("Correct Choice Index", data.correctChoiceIndex);
            data.correctSound = EditorGUILayout.TextField("Correct Sound", data.correctSound);
            data.wrongSound = EditorGUILayout.TextField("Wrong Sound", data.wrongSound);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            string previousQuestionText = data.questionText;
            MissionGraphEditorFieldUtility.DrawLocalizationKeyField("Question", data.questionText, value => data.questionText = value);
            if (!string.Equals(previousQuestionText, data.questionText, StringComparison.Ordinal))
                changed = true;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);

            string[] choices = data.choiceTexts ?? Array.Empty<string>();
            for (int i = 0; i < choices.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                int index = i;
                string previousChoice = choices[i];
                MissionGraphEditorFieldUtility.DrawLocalizationKeyField($"Choice {i}", choices[i], value => choices[index] = value);
                if (!string.Equals(previousChoice, choices[i], StringComparison.Ordinal))
                {
                    data.choiceTexts = choices;
                    changed = true;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    data.choiceTexts = RemoveAt(choices, i);
                    if (data.correctChoiceIndex >= data.choiceTexts.Length)
                        data.correctChoiceIndex = Mathf.Max(0, data.choiceTexts.Length - 1);
                    changed = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Choice"))
            {
                string[] current = data.choiceTexts ?? Array.Empty<string>();
                Array.Resize(ref current, current.Length + 1);
                current[current.Length - 1] = string.Empty;
                data.choiceTexts = current;
                changed = true;
            }

            if (data.choiceTexts == null || data.choiceTexts.Length == 0)
                data.correctChoiceIndex = 0;
            else if (data.correctChoiceIndex < 0 || data.correctChoiceIndex >= data.choiceTexts.Length)
                data.correctChoiceIndex = Mathf.Clamp(data.correctChoiceIndex, 0, data.choiceTexts.Length - 1);

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        private static string[] RemoveAt(string[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source ?? Array.Empty<string>();

            string[] result = new string[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, result, 0, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            return result;
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowMultipleChoiceDialogMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateLocalizationKeyField(
                "Question",
                data.questionText,
                value =>
                {
                    data.questionText = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            string[] transformRefIds = MissionTransformReferenceUtility.GetAllTransformReferenceIds();
            var transformRefChoices = new System.Collections.Generic.List<string> { "(none)" };
            transformRefChoices.AddRange(transformRefIds);
            string currentTransformRef = string.IsNullOrEmpty(data.transformReferenceId) ? "(none)" : data.transformReferenceId;
            if (!transformRefChoices.Contains(currentTransformRef))
                transformRefChoices.Insert(1, currentTransformRef + " (Missing)");

            var transformDropdown = new UnityEngine.UIElements.DropdownField("UI Position Ref", transformRefChoices, transformRefChoices.IndexOf(currentTransformRef));
            transformDropdown.style.color = paramTextColor;
            transformDropdown.style.fontSize = paramFontSize;
            transformDropdown.RegisterValueChangedCallback(evt =>
            {
                string selected = evt.newValue;
                if (selected == "(none)")
                    data.transformReferenceId = string.Empty;
                else
                    data.transformReferenceId = selected.Replace(" (Missing)", string.Empty);
                context?.SaveGraphPositions?.Invoke();
                context?.Repaint?.Invoke();
            });
            root.Add(transformDropdown);

            root.Add(MissionGraphEditorUIComponents.CreateIntField(
                "Correct Choice Index",
                data.correctChoiceIndex,
                value =>
                {
                    data.correctChoiceIndex = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Correct Sound",
                data.correctSound,
                value =>
                {
                    data.correctSound = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Wrong Sound",
                data.wrongSound,
                value =>
                {
                    data.wrongSound = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            // Choices collection
            string[] choices = data.choiceTexts ?? Array.Empty<string>();
            if (choices.Length == 0)
            {
                root.Add(MissionGraphEditorCollectionComponent.CreateEmptyCollectionState(
                    "No choices defined",
                    () =>
                    {
                        string[] current = data.choiceTexts ?? Array.Empty<string>();
                        Array.Resize(ref current, current.Length + 1);
                        current[current.Length - 1] = string.Empty;
                        data.choiceTexts = current;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    paramTextColor,
                    paramFontSize));
            }
            else
            {
                root.Add(MissionGraphEditorCollectionComponent.CreateCollectionSection(
                    "Choices",
                    choices.Length,
                    addIndex =>
                    {
                        string[] current = data.choiceTexts ?? Array.Empty<string>();
                        Array.Resize(ref current, current.Length + 1);
                        current[current.Length - 1] = string.Empty;
                        data.choiceTexts = current;
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    },
                    itemIndex =>
                    {
                        return MissionGraphEditorCollectionComponent.CreateCollectionItemEditor(
                            itemIndex,
                            "Choice",
                            MissionGraphEditorUIComponents.CreateLocalizationKeyField(
                                "Text",
                                choices[itemIndex],
                                value =>
                                {
                                    choices[itemIndex] = value;
                                    data.choiceTexts = choices;
                                    context?.SaveGraphPositions?.Invoke();
                                    context?.Repaint?.Invoke();
                                },
                                paramTextColor,
                                paramFontSize),
                            () => { /* move up not supported for choices */ },
                            () => { /* move down not supported for choices */ },
                            () =>
                            {
                                data.choiceTexts = RemoveAt(choices, itemIndex);
                                if (data.correctChoiceIndex >= data.choiceTexts.Length)
                                    data.correctChoiceIndex = Mathf.Max(0, data.choiceTexts.Length - 1);
                                context?.SaveGraphPositions?.Invoke();
                                context?.Repaint?.Invoke();
                            },
                            false,
                            false);
                    },
                    paramTextColor,
                    paramFontSize));
            }

            return root;
        }

    }
}
