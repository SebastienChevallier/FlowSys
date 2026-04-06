using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(WaitForUserActionsGroupMissionActionData))]
    internal sealed class WaitForUserActionsGroupMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        private static readonly Dictionary<ManagedUserActionVoiceOverItem, bool> ItemFoldoutStates = new Dictionary<ManagedUserActionVoiceOverItem, bool>();

        /// <summary>
        /// IMGUI-based Draw method for backward compatibility.
        /// Kept for transitional support during migration to UI Toolkit.
        /// </summary>
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForUserActionsGroupMissionActionData data)
                return false;

            bool changed = false;

            EditorGUI.BeginChangeCheck();
            data.autoEnableUserActions = EditorGUILayout.Toggle("Auto Enable User Actions", data.autoEnableUserActions);
            data.waitForVoiceOverCompletion = EditorGUILayout.Toggle("Wait For VO Completion", data.waitForVoiceOverCompletion);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            EditorGUILayout.Space(4f);
            MissionGraphEditorFieldUtility.DrawSectionHeader("User Action Items");

            ManagedUserActionVoiceOverItem[] items = data.items ?? Array.Empty<ManagedUserActionVoiceOverItem>();
            for (int i = 0; i < items.Length; i++)
            {
                ManagedUserActionVoiceOverItem item = items[i] ?? new ManagedUserActionVoiceOverItem();
                items[i] = item;

                int itemIndex = i;
                string itemTitle = string.IsNullOrEmpty(item.userActionId) ? $"Item {i + 1}" : item.userActionId;
                MissionGraphEditorCollectionUtility.DrawFoldoutCard(itemTitle, MissionGraphEditorFieldUtility.GetBodyColor(action), true, null, () =>
                {
                    EditorGUI.BeginChangeCheck();
                    item.userActionId = EditorGUILayout.TextField("User Action ID", item.userActionId);
                    if (EditorGUI.EndChangeCheck())
                        changed = true;

                    string previousVoiceOverKey = item.voiceOverKey;
                    MissionGraphEditorFieldUtility.DrawVoiceOverSelectorField("Voice Over Key", item.voiceOverKey, value => item.voiceOverKey = value);
                    if (!string.Equals(previousVoiceOverKey, item.voiceOverKey, System.StringComparison.Ordinal))
                        changed = true;
                }, () =>
                {
                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("X", true, 70f))
                    {
                        data.items = MissionGraphEditorCollectionUtility.RemoveAt(items, itemIndex);
                        changed = true;
                        GUIUtility.ExitGUI();
                    }
                });
            }

            if (MissionGraphEditorFieldUtility.DrawSettingsButton("Add User Action Item", false, 140f))
            {
                ManagedUserActionVoiceOverItem[] current = data.items ?? Array.Empty<ManagedUserActionVoiceOverItem>();
                Array.Resize(ref current, current.Length + 1);
                current[current.Length - 1] = new ManagedUserActionVoiceOverItem();
                data.items = current;
                changed = true;
            }

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        /// <summary>
        /// UI Toolkit-based BuildElement method for Full UI Toolkit pipeline.
        /// Progressively replaces IMGUI Draw method.
        /// </summary>
        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForUserActionsGroupMissionActionData data)
                return null;

            Color textColor = MissionGraphEditorUIUtility.GetItemTextColor();
            int fontSize = MissionGraphEditorUIUtility.GetActionFontSize();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();
            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            Color bodyColor = MissionGraphEditorFieldUtility.GetBodyColor(action);
            Color nodeBackgroundColor = new Color(0.2f, 0.65f, 0.9f, 1f);

            VisualElement root = new VisualElement();

            // Main properties
            VisualElement propsSection = new VisualElement();
            propsSection.style.paddingBottom = 4;

            propsSection.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Auto Enable User Actions",
                data.autoEnableUserActions,
                value =>
                {
                    data.autoEnableUserActions = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            propsSection.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Wait For VO Completion",
                data.waitForVoiceOverCompletion,
                value =>
                {
                    data.waitForVoiceOverCompletion = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(propsSection);

            // Items section
            ManagedUserActionVoiceOverItem[] items = data.items ?? Array.Empty<ManagedUserActionVoiceOverItem>();

            VisualElement itemsContent = new VisualElement();
            if (items.Length == 0)
            {
                itemsContent.Add(MissionGraphEditorUIComponents.CreateEmptyStateLabel(
                    "No user action items",
                    paramTextColor,
                    paramFontSize));
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    ManagedUserActionVoiceOverItem item = items[i] ?? new ManagedUserActionVoiceOverItem();
                    items[i] = item;
                    int itemIndex = i;

                    string itemTitle = string.IsNullOrEmpty(item.userActionId) ? $"Item {i + 1}" : item.userActionId;
                    VisualElement itemCard = MissionGraphEditorUIComponents.CreateCollectionItemCard(
                        itemTitle,
                        bodyColor,
                        MissionGraphEditorUIComponents.GetBorderColor(nodeBackgroundColor),
                        GetItemFoldoutState(item),
                        value => ItemFoldoutStates[item] = value,
                        null,
                        itemIndex,
                        items.Length,
                        () =>
                        {
                            data.items = MissionGraphEditorCollectionUtility.Move(items, itemIndex, itemIndex - 1);
                            context?.SaveGraphPositions?.Invoke();
                            context?.Repaint?.Invoke();
                        },
                        () =>
                        {
                            data.items = MissionGraphEditorCollectionUtility.Move(items, itemIndex, itemIndex + 1);
                            context?.SaveGraphPositions?.Invoke();
                            context?.Repaint?.Invoke();
                        },
                        () =>
                        {
                            data.items = MissionGraphEditorCollectionUtility.RemoveAt(items, itemIndex);
                            context?.SaveGraphPositions?.Invoke();
                            context?.Repaint?.Invoke();
                        });

                    VisualElement cardBody = MissionGraphEditorUIComponents.GetCardBody(itemCard);
                    if (cardBody != null)
                    {
                        cardBody.Add(MissionGraphEditorUIComponents.CreateTextField(
                            "User Action ID",
                            item.userActionId,
                            value =>
                            {
                                item.userActionId = value;
                                context?.SaveGraphPositions?.Invoke();
                                context?.Repaint?.Invoke();
                            },
                            paramTextColor,
                            paramFontSize));

                        cardBody.Add(MissionGraphEditorUIUtility.CreateVoiceOverPopupField(
                            "Voice Over Key",
                            item.voiceOverKey,
                            value =>
                            {
                                item.voiceOverKey = value;
                                context?.SaveGraphPositions?.Invoke();
                                context?.Repaint?.Invoke();
                            },
                            paramTextColor,
                            paramFontSize));
                    }

                    itemsContent.Add(itemCard);
                }
            }

            VisualElement itemsSection = MissionGraphEditorUIUtility.CreateCollectionSection(
                "User Action Items",
                items.Length,
                new Color(0.15f, 0.15f, 0.25f, 1f),
                new Color(0.3f, 0.4f, 0.6f, 1f),
                textColor,
                new Color(0.2f, 0.65f, 0.9f, 1f),
                fontSize,
                true,
                null,
                () =>
                {
                    ManagedUserActionVoiceOverItem[] current = data.items ?? Array.Empty<ManagedUserActionVoiceOverItem>();
                    Array.Resize(ref current, current.Length + 1);
                    current[current.Length - 1] = new ManagedUserActionVoiceOverItem();
                    data.items = current;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                () => itemsContent);

            root.Add(itemsSection);

            return root;
        }

        private static bool GetItemFoldoutState(ManagedUserActionVoiceOverItem item)
        {
            if (item == null)
                return false;

            if (ItemFoldoutStates.TryGetValue(item, out bool value))
                return value;

            return false;
        }
    }
}
