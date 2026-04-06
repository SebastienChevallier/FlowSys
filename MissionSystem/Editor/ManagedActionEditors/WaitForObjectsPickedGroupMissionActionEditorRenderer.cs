using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(WaitForObjectsPickedGroupMissionActionData))]
    internal sealed class WaitForObjectsPickedGroupMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        private static readonly Dictionary<ManagedObjectPickReactionItem, bool> PickItemFoldoutStates = new Dictionary<ManagedObjectPickReactionItem, bool>();
        private static readonly Dictionary<MissionActionData, bool> ReactionFoldoutStates = new Dictionary<MissionActionData, bool>();

        /// <summary>
        /// IMGUI-based Draw method for backward compatibility.
        /// </summary>
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForObjectsPickedGroupMissionActionData data)
                return false;

            bool changed = false;

            EditorGUI.BeginChangeCheck();
            data.enablePickingOnStart = EditorGUILayout.Toggle("Enable Picking On Start", data.enablePickingOnStart);
            data.waitForVoiceOverCompletion = EditorGUILayout.Toggle("Wait For VO Completion", data.waitForVoiceOverCompletion);
            data.completionDelaySeconds = Mathf.Max(0f, EditorGUILayout.FloatField("Completion Delay (s)", data.completionDelaySeconds));
            if (EditorGUI.EndChangeCheck())
                changed = true;

            EditorGUILayout.Space(4f);
            MissionGraphEditorFieldUtility.DrawSectionHeader("Pick Items");

            ManagedObjectPickReactionItem[] items = data.items ?? Array.Empty<ManagedObjectPickReactionItem>();
            for (int i = 0; i < items.Length; i++)
            {
                ManagedObjectPickReactionItem item = items[i] ?? new ManagedObjectPickReactionItem();
                items[i] = item;
                int itemIndex = i;
                string itemTitle = string.IsNullOrEmpty(item.objectId) ? $"Item {i + 1}" : item.objectId;

                MissionGraphEditorCollectionUtility.DrawFoldoutCard(itemTitle, MissionGraphEditorFieldUtility.GetBodyColor(action), GetPickItemFoldoutState(item), value =>
                {
                    PickItemFoldoutStates[item] = value;
                }, () =>
                {
                    string previousObjectId = item.objectId;
                    MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", item.objectId, value => item.objectId = value);
                    if (!string.Equals(previousObjectId, item.objectId, StringComparison.Ordinal))
                        changed = true;

                    string previousVoiceOverKey = item.voiceOverKey;
                    MissionGraphEditorFieldUtility.DrawVoiceOverSelectorField("Voice Over Key", item.voiceOverKey, value => item.voiceOverKey = value);
                    if (!string.Equals(previousVoiceOverKey, item.voiceOverKey, StringComparison.Ordinal))
                        changed = true;

                    MissionActionData[] reactions = item.onPickedActions ?? Array.Empty<MissionActionData>();
                    EditorGUILayout.Space(2f);
                    MissionGraphEditorFieldUtility.DrawSectionHeader("Reactions", () =>
                    {
                        ShowAddReactionMenu(item, context);
                        GUIUtility.ExitGUI();
                    });

                    for (int reactionIndex = 0; reactionIndex < reactions.Length; reactionIndex++)
                    {
                        MissionActionData reaction = reactions[reactionIndex];
                        MissionStepActionEntry reactionEntry = new MissionStepActionEntry
                        {
                            phase = action.phase,
                            managedAction = reaction
                        };

                        if (DrawReactionCard(item, reactions, reactionIndex, reactionEntry, action, context))
                        {
                            changed = true;
                            break;
                        }
                    }
                }, () =>
                {
                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("X", true, MissionGraphEditorFieldUtility.GetButtonWidth()))
                    {
                        data.items = MissionGraphEditorCollectionUtility.RemoveAt(items, itemIndex);
                        changed = true;
                        GUIUtility.ExitGUI();
                    }
                });
            }

            if (MissionGraphEditorFieldUtility.DrawSettingsButton("Add Pick Item", false, 120f))
            {
                ManagedObjectPickReactionItem[] current = data.items ?? Array.Empty<ManagedObjectPickReactionItem>();
                Array.Resize(ref current, current.Length + 1);
                current[current.Length - 1] = new ManagedObjectPickReactionItem();
                data.items = current;
                changed = true;
            }

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        private static void ShowAddReactionMenu(ManagedObjectPickReactionItem item, MissionInlineEditorContext context)
        {
            GenericMenu menu = new GenericMenu();
            bool hasItem = false;

            foreach (Type type in TypeCache.GetTypesDerivedFrom<MissionActionData>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                hasItem = true;
                string label = MissionGraphEditorFieldUtility.GetTypeMenuLabel(type);
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    MissionActionData instance = Activator.CreateInstance(type) as MissionActionData;
                    if (instance == null)
                        return;

                    MissionActionData[] current = item.onPickedActions ?? Array.Empty<MissionActionData>();
                    Array.Resize(ref current, current.Length + 1);
                    current[current.Length - 1] = instance;
                    item.onPickedActions = current;
                    MissionGraphEditorFieldUtility.ApplyChange(true, context);
                });
            }

            if (!hasItem)
                menu.AddDisabledItem(new GUIContent("No managed action type found"));

            menu.ShowAsContext();
        }

        private static bool DrawReactionCard(ManagedObjectPickReactionItem item, MissionActionData[] reactions, int reactionIndex, MissionStepActionEntry reactionEntry, MissionStepActionEntry parentAction, MissionInlineEditorContext context)
        {
            bool deleted = false;
            MissionGraphEditorCollectionUtility.DrawFoldoutCard(
                reactionEntry.managedAction?.GetDisplayName() ?? "(empty action)",
                MissionGraphEditorFieldUtility.GetBodyColor(parentAction),
                GetReactionFoldoutState(reactionEntry.managedAction),
                value =>
                {
                    if (reactionEntry.managedAction != null)
                        ReactionFoldoutStates[reactionEntry.managedAction] = value;
                },
                () => DrawReactionBody(reactionEntry, context),
                () =>
                {
                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("↑", false, MissionGraphEditorFieldUtility.GetButtonWidth(), reactionIndex > 0))
                        item.onPickedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex - 1);

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("↓", false, MissionGraphEditorFieldUtility.GetButtonWidth(), reactionIndex < reactions.Length - 1))
                        item.onPickedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex + 1);

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("X", true, MissionGraphEditorFieldUtility.GetButtonWidth()))
                    {
                        item.onPickedActions = MissionGraphEditorCollectionUtility.RemoveAt(reactions, reactionIndex);
                        deleted = true;
                        GUIUtility.ExitGUI();
                    }
                });

            return deleted;
        }

        private static void DrawReactionBody(MissionStepActionEntry reactionEntry, MissionInlineEditorContext context)
        {
            if (reactionEntry?.managedAction == null)
            {
                EditorGUILayout.HelpBox("Missing reaction instance.", MessageType.Warning);
                return;
            }

            if (!MissionManagedTypeEditorRegistry.TryGetActionRenderer(reactionEntry.managedAction.GetType(), out IMissionActionEditorRenderer renderer) || renderer == null)
            {
                EditorGUILayout.HelpBox($"No editor renderer registered for {reactionEntry.managedAction.GetType().Name}.", MessageType.Info);
                return;
            }

            renderer.Draw(reactionEntry, context);
        }

        private static bool GetPickItemFoldoutState(ManagedObjectPickReactionItem item)
        {
            if (item == null)
                return false;
            if (PickItemFoldoutStates.TryGetValue(item, out bool value))
                return value;
            return false;
        }

        private static bool GetReactionFoldoutState(MissionActionData reaction)
        {
            if (reaction == null)
                return true;
            if (ReactionFoldoutStates.TryGetValue(reaction, out bool value))
                return value;
            return true;
        }

        /// <summary>
        /// UI Toolkit-based BuildElement method.
        /// Handles nested structure: pick items containing reactions.
        /// </summary>
        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForObjectsPickedGroupMissionActionData data)
                return null;

            Color textColor = MissionGraphEditorUIUtility.GetItemTextColor();
            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int fontSize = MissionGraphEditorUIUtility.GetActionFontSize();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();
            Color nodeBackgroundColor = new Color(0.2f, 0.65f, 0.9f, 1f);

            VisualElement root = new VisualElement();

            // Main properties
            VisualElement propsSection = new VisualElement();
            propsSection.style.paddingBottom = 4;

            propsSection.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Enable Picking On Start",
                data.enablePickingOnStart,
                value =>
                {
                    data.enablePickingOnStart = value;
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

            propsSection.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Completion Delay (s)",
                data.completionDelaySeconds,
                value =>
                {
                    data.completionDelaySeconds = Mathf.Max(0f, value);
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(propsSection);

            // Pick items section
            ManagedObjectPickReactionItem[] items = data.items ?? Array.Empty<ManagedObjectPickReactionItem>();
            VisualElement itemsContent = new VisualElement();

            if (items.Length == 0)
            {
                itemsContent.Add(MissionGraphEditorUIComponents.CreateEmptyStateLabel(
                    "No pick items",
                    paramTextColor,
                    paramFontSize));
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    ManagedObjectPickReactionItem item = items[i] ?? new ManagedObjectPickReactionItem();
                    items[i] = item;
                    int itemIndex = i;

                    string itemTitle = string.IsNullOrEmpty(item.objectId) ? $"Item {i + 1}" : item.objectId;
                    Color itemColor = new Color(0.25f, 0.25f, 0.35f, 1f);
                    Color borderColor = MissionGraphEditorUIComponents.GetBorderColor(nodeBackgroundColor);

                    VisualElement itemCard = MissionGraphEditorUIComponents.CreateFoldoutCard(
                        itemTitle,
                        itemColor,
                        borderColor,
                        GetPickItemFoldoutState(item),
                        value => PickItemFoldoutStates[item] = value);

                    // Delete button
                    Button deleteBtn = MissionGraphEditorUIComponents.CreateIconButton("X", () =>
                    {
                        data.items = MissionGraphEditorCollectionUtility.RemoveAt(items, itemIndex);
                        context?.SaveGraphPositions?.Invoke();
                        context?.Repaint?.Invoke();
                    }, 24, true, true);
                    itemCard.Q<VisualElement>(null, "unity-foldout__toggle").parent.Add(deleteBtn);

                    VisualElement itemBody = MissionGraphEditorUIComponents.GetCardBody(itemCard);
                    if (itemBody != null)
                    {
                        itemBody.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                            "Object ID",
                            item.objectId,
                            value =>
                            {
                                item.objectId = value;
                                context?.SaveGraphPositions?.Invoke();
                                context?.Repaint?.Invoke();
                            },
                            paramTextColor,
                            paramFontSize));

                        itemBody.Add(MissionGraphEditorUIUtility.CreateVoiceOverPopupField(
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

                        // Reactions sub-section with foldout cards
                        MissionActionData[] reactions = item.onPickedActions ?? Array.Empty<MissionActionData>();

                        VisualElement reactionsSection = new VisualElement();
                        reactionsSection.style.marginTop = 8;

                        for (int r = 0; r < reactions.Length; r++)
                        {
                            MissionActionData reaction = reactions[r];
                            int reactionIndex = r;

                            string reactionTitle = reaction?.GetDisplayName() ?? "(empty)";
                            VisualElement reactionCard = MissionGraphEditorUIComponents.CreateCollectionItemCard(
                                reactionTitle,
                                MissionGraphEditorFieldUtility.GetBodyColor(action),
                                borderColor,
                                GetReactionFoldoutState(reaction),
                                value =>
                                {
                                    if (reaction != null)
                                        ReactionFoldoutStates[reaction] = value;
                                },
                                null,
                                reactionIndex,
                                reactions.Length,
                                () =>
                                {
                                    item.onPickedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex - 1);
                                    context?.SaveGraphPositions?.Invoke();
                                    context?.Repaint?.Invoke();
                                },
                                () =>
                                {
                                    item.onPickedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex + 1);
                                    context?.SaveGraphPositions?.Invoke();
                                    context?.Repaint?.Invoke();
                                },
                                () =>
                                {
                                    item.onPickedActions = MissionGraphEditorCollectionUtility.RemoveAt(reactions, reactionIndex);
                                    context?.SaveGraphPositions?.Invoke();
                                    context?.Repaint?.Invoke();
                                });

                            // Add reaction content to body
                            VisualElement reactionBody = MissionGraphEditorUIComponents.GetCardBody(reactionCard);
                            if (reactionBody != null)
                            {
                                MissionStepActionEntry reactionEntry = new MissionStepActionEntry
                                {
                                    phase = action.phase,
                                    managedAction = reaction
                                };
                                reactionBody.Add(MissionRendererAdapter.BuildManagedActionElement(reactionEntry, context));
                            }

                            reactionsSection.Add(reactionCard);
                        }

                        // Toujours afficher le bouton, même si aucune réaction n'existe encore
                        Button addReactionBtn = MissionGraphEditorUIComponents.CreateIconButton("+", () => ShowAddReactionMenu(item, context), 24, false, true);
                        addReactionBtn.text = "+ Add Reaction";
                        addReactionBtn.style.width = StyleKeyword.Auto;
                        addReactionBtn.style.marginTop = 4;
                        addReactionBtn.style.marginLeft = 4;
                        addReactionBtn.style.marginRight = 4;
                        reactionsSection.Add(addReactionBtn);

                        itemBody.Add(reactionsSection);
                    }

                    itemsContent.Add(itemCard);
                }
            }

            VisualElement itemsSection = MissionGraphEditorUIUtility.CreateCollectionSection(
                "Pick Items",
                items.Length,
                new Color(0.15f, 0.15f, 0.25f, 1f),
                MissionGraphEditorFieldUtility.GetBodyColor(action),
                textColor,
                nodeBackgroundColor,
                fontSize,
                true,
                null,
                () =>
                {
                    ManagedObjectPickReactionItem[] current = data.items ?? Array.Empty<ManagedObjectPickReactionItem>();
                    Array.Resize(ref current, current.Length + 1);
                    current[current.Length - 1] = new ManagedObjectPickReactionItem();
                    data.items = current;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                () => itemsContent);

            root.Add(itemsSection);

            return root;
        }

        private void ShowAddReactionMenuUI(ManagedObjectPickReactionItem item, MissionInlineEditorContext context)
        {
            ShowAddReactionMenu(item, context);
        }
    }
}
