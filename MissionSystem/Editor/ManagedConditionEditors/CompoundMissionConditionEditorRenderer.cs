using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionConditionEditorRenderer(typeof(CompoundMissionConditionData))]
    internal sealed class CompoundMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not CompoundMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();

            // Draw operator dropdown
            data.Operator = (LogicalOperator)EditorGUILayout.EnumPopup("Operator", data.Operator);

            // Draw WaitForOnEnterCompletion toggle
            data.WaitForParentStepOnEnterCompletion = EditorGUILayout.Toggle(
                "Wait for OnEnter",
                data.WaitForParentStepOnEnterCompletion);

            // Draw sub-conditions list
            EditorGUILayout.LabelField("Sub-Conditions", EditorStyles.boldLabel);

            if (data.SubConditions == null)
                data.SubConditions = new List<MissionStepConditionEntry>();

            EditorGUI.indentLevel++;

            for (int i = 0; i < data.SubConditions.Count; i++)
            {
                var entry = data.SubConditions[i];
                string label = entry?.managedCondition?.GetDisplayName() ?? "<Empty>";
                EditorGUILayout.LabelField($"[{i}] {label}");
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("+ Add Sub-Condition", GUILayout.Height(20)))
            {
                ShowAddSubConditionMenu(data.SubConditions, context);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                context?.SaveGraphPositions?.Invoke();
                context?.Repaint?.Invoke();
            }
            return changed;
        }

        private bool DrawSubConditionsList(List<MissionStepConditionEntry> subConditions, MissionInlineEditorContext context)
        {
            bool anyChanged = false;

            // Draw each sub-condition
            for (int i = 0; i < subConditions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var entry = subConditions[i];
                if (entry != null && entry.managedCondition != null)
                {
                    EditorGUILayout.LabelField($"[{i}] {entry.managedCondition.GetDisplayName()}");
                }
                else
                {
                    EditorGUILayout.LabelField($"[{i}] <Empty Condition>");
                }

                // Move up button
                if (i > 0 && GUILayout.Button("↑", GUILayout.Width(30)))
                {
                    (subConditions[i], subConditions[i - 1]) = (subConditions[i - 1], subConditions[i]);
                    anyChanged = true;
                }

                // Move down button
                if (i < subConditions.Count - 1 && GUILayout.Button("↓", GUILayout.Width(30)))
                {
                    (subConditions[i], subConditions[i + 1]) = (subConditions[i + 1], subConditions[i]);
                    anyChanged = true;
                }

                // Remove button
                if (GUILayout.Button("−", GUILayout.Width(30)))
                {
                    subConditions.RemoveAt(i);
                    anyChanged = true;
                }

                EditorGUILayout.EndHorizontal();
            }

            // Add button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("+ Add Sub-Condition"))
            {
                ShowAddSubConditionMenu(subConditions, context);
                anyChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            return anyChanged;
        }

        private void ShowAddSubConditionMenu(List<MissionStepConditionEntry> subConditions, MissionInlineEditorContext context, Action onAdded = null)
        {
            GenericMenu menu = new GenericMenu();
            bool hasItem = false;

            var allConditionTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<MissionConditionData>();
            foreach (var type in allConditionTypes)
            {
                if (type == null || type.IsAbstract || type.IsGenericType || type == typeof(CompoundMissionConditionData))
                    continue;

                ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    continue;

                hasItem = true;
                string label = type.Name;
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    MissionConditionData instance = Activator.CreateInstance(type) as MissionConditionData;
                    if (instance == null)
                        return;

                    var newEntry = new MissionStepConditionEntry
                    {
                        managedCondition = instance
                    };
                    subConditions.Add(newEntry);

                    onAdded?.Invoke();
                    context?.SaveGraphPositions?.Invoke();
                });
            }

            if (!hasItem)
                menu.AddDisabledItem(new GUIContent("No condition types available"));

            menu.ShowAsContext();
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not CompoundMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            var root = new VisualElement();

            // Operator dropdown
            var operatorContainer = new VisualElement();
            var operatorLabel = new Label("Logical Operator");
            operatorLabel.style.color = paramTextColor;
            operatorLabel.style.fontSize = paramFontSize;
            operatorContainer.Add(operatorLabel);

            var operatorField = new EnumField("", data.Operator);
            operatorField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is LogicalOperator op)
                {
                    data.Operator = op;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                }
            });
            operatorContainer.Add(operatorField);
            root.Add(operatorContainer);

            // Wait for OnEnter completion toggle
            var waitContainer = new VisualElement();
            var waitToggle = new Toggle("Wait for OnEnter Completion");
            waitToggle.value = data.WaitForParentStepOnEnterCompletion;
            waitToggle.RegisterValueChangedCallback(evt =>
            {
                data.WaitForParentStepOnEnterCompletion = evt.newValue;
                context?.SaveGraphPositions?.Invoke();
                context?.Repaint?.Invoke();
            });
            waitToggle.style.marginTop = 4;
            waitContainer.Add(waitToggle);
            root.Add(waitContainer);

            // Sub-conditions section
            var subConditionsContainer = new VisualElement();
            subConditionsContainer.style.marginTop = 8;
            var subConditionsLabel = new Label("Sub-Conditions");
            subConditionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            subConditionsLabel.style.color = paramTextColor;
            subConditionsLabel.style.fontSize = paramFontSize;
            subConditionsContainer.Add(subConditionsLabel);

            var subConditionsRows = new VisualElement();
            subConditionsContainer.Add(subConditionsRows);

            void RefreshSubConditionRows()
            {
                subConditionsRows.Clear();
                if (data.SubConditions == null) return;
                for (int i = 0; i < data.SubConditions.Count; i++)
                {
                    var capturedIndex = i;
                    var subEntry = data.SubConditions[capturedIndex];
                    var subConditionElement = BuildSubConditionElement(
                        subEntry, capturedIndex, data.SubConditions, context,
                        paramTextColor, paramFontSize,
                        onRemoved: RefreshSubConditionRows);
                    subConditionsRows.Add(subConditionElement);
                }
            }

            RefreshSubConditionRows();

            // Add button
            var addButtonContainer = new VisualElement();
            addButtonContainer.style.marginTop = 4;
            var addButton = new Button(() =>
            {
                ShowAddSubConditionMenu(data.SubConditions, context, onAdded: RefreshSubConditionRows);
            });
            addButton.text = "+ Add Sub-Condition";
            addButton.style.height = 24;
            addButtonContainer.Add(addButton);
            subConditionsContainer.Add(addButtonContainer);

            root.Add(subConditionsContainer);

            return root;
        }

        private VisualElement BuildSubConditionElement(MissionStepConditionEntry entry, int index, List<MissionStepConditionEntry> list, MissionInlineEditorContext context, Color textColor, int fontSize, Action onRemoved = null)
        {
            var container = new VisualElement();
            container.style.marginLeft = 8;
            container.style.marginTop = 4;
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = new StyleColor(Color.gray);
            container.style.paddingLeft = 8;

            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;

            var label = new Label(entry?.managedCondition?.GetDisplayName() ?? "<Empty>");
            label.style.color = textColor;
            label.style.fontSize = fontSize;
            label.style.flexGrow = 1;
            headerContainer.Add(label);

            // Remove button
            var removeButton = new Button(() =>
            {
                list.RemoveAt(index);
                onRemoved?.Invoke();
                context?.SaveGraphPositions?.Invoke();
            });
            removeButton.text = "−";
            removeButton.style.width = 24;
            removeButton.style.height = 24;
            headerContainer.Add(removeButton);

            container.Add(headerContainer);

            // Render sub-condition parameters via its dedicated renderer
            if (entry?.managedCondition != null)
            {
                Type subType = entry.managedCondition.GetType();
                if (MissionRendererAdapter.TryGetUIConditionElementRenderer(subType, out IMissionUIConditionElementRenderer subUIRenderer))
                {
                    VisualElement subElement = subUIRenderer.BuildElement(entry, context);
                    if (subElement != null)
                    {
                        subElement.style.marginTop = 4;
                        container.Add(subElement);
                    }
                }
                else if (MissionManagedTypeEditorRegistry.TryGetConditionRenderer(subType, out IMissionConditionEditorRenderer subImguiRenderer))
                {
                    var imguiWrapper = MissionRendererAdapter.WrapIMGUIInContainer(() => subImguiRenderer.Draw(entry, context));
                    if (imguiWrapper != null)
                        container.Add(imguiWrapper);
                }
            }

            return container;
        }
    }
}
