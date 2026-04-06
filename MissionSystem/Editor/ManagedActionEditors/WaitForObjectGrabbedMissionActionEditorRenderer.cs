using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(WaitForObjectGrabbedMissionActionData))]
    internal sealed class WaitForObjectGrabbedMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        private static readonly Dictionary<MissionActionData, bool> ReactionFoldoutStates = new Dictionary<MissionActionData, bool>();

        /// <summary>
        /// IMGUI-based Draw method for backward compatibility.
        /// </summary>
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForObjectGrabbedMissionActionData data)
                return false;

            bool changed = false;

            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", data.objectId, value => data.objectId = value);
            if (!string.Equals(previousObjectId, data.objectId, StringComparison.Ordinal))
                changed = true;

            MissionActionData[] reactions = data.onGrabbedActions ?? Array.Empty<MissionActionData>();
            EditorGUILayout.Space(2f);
            MissionGraphEditorFieldUtility.DrawSectionHeader("Reactions", () =>
            {
                ShowAddReactionMenu(data, context);
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

                bool deleted = DrawReactionCard(data, reactions, reactionIndex, reactionEntry, action, context);
                if (deleted)
                {
                    changed = true;
                    break;
                }
            }

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        /// <summary>
        /// UI Toolkit-based BuildElement method.
        /// </summary>
        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForObjectGrabbedMissionActionData data)
                return null;

            Color textColor = MissionGraphEditorUIUtility.GetItemTextColor();
            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int fontSize = MissionGraphEditorUIUtility.GetActionFontSize();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();
            Color bodyColor = MissionGraphEditorFieldUtility.GetBodyColor(action);
            Color nodeBackgroundColor = new Color(0.2f, 0.65f, 0.9f, 1f);

            VisualElement root = new VisualElement();

            // Object ID field
            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                "Object ID",
                data.objectId,
                value =>
                {
                    data.objectId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            VisualElement spacer = new VisualElement();
            spacer.style.height = 4;
            root.Add(spacer);

            // Reactions section with foldout cards
            MissionActionData[] reactions = data.onGrabbedActions ?? Array.Empty<MissionActionData>();

            if (reactions.Length > 0)
            {
                VisualElement reactionsSection = new VisualElement();
                reactionsSection.style.marginTop = 8;

                for (int i = 0; i < reactions.Length; i++)
                {
                    MissionActionData reaction = reactions[i];
                    int reactionIndex = i;

                    string reactionTitle = reaction?.GetDisplayName() ?? "(empty)";
                    VisualElement reactionCard = MissionGraphEditorUIComponents.CreateCollectionItemCard(
                        reactionTitle,
                        bodyColor,
                        MissionGraphEditorUIComponents.GetBorderColor(nodeBackgroundColor),
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
                            data.onGrabbedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex - 1);
                            context?.SaveGraphPositions?.Invoke();
                            context?.Repaint?.Invoke();
                        },
                        () =>
                        {
                            data.onGrabbedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex + 1);
                            context?.SaveGraphPositions?.Invoke();
                            context?.Repaint?.Invoke();
                        },
                        () =>
                        {
                            data.onGrabbedActions = MissionGraphEditorCollectionUtility.RemoveAt(reactions, reactionIndex);
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

                // Add button to add new reaction
                Button addReactionBtn = MissionGraphEditorUIComponents.CreateIconButton("+", () => ShowAddReactionMenu(data, context), 24, false, true);
                addReactionBtn.text = "+ Add Reaction";
                addReactionBtn.style.width = StyleKeyword.Auto;
                addReactionBtn.style.marginTop = 4;
                addReactionBtn.style.marginLeft = 4;
                addReactionBtn.style.marginRight = 4;
                reactionsSection.Add(addReactionBtn);

                root.Add(reactionsSection);
            }

            return root;
        }

        private void ShowAddReactionMenuUI(WaitForObjectGrabbedMissionActionData data, MissionInlineEditorContext context)
        {
            ShowAddReactionMenu(data, context);
        }

        private static void ShowAddReactionMenu(WaitForObjectGrabbedMissionActionData data, MissionInlineEditorContext context)
        {
            GenericMenu menu = new GenericMenu();
            bool hasItem = false;

            foreach (Type type in TypeCache.GetTypesDerivedFrom<MissionActionData>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType || type == typeof(WaitForObjectGrabbedMissionActionData) || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                hasItem = true;
                string label = MissionGraphEditorFieldUtility.GetTypeMenuLabel(type);
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    MissionActionData instance = Activator.CreateInstance(type) as MissionActionData;
                    if (instance == null)
                        return;

                    MissionActionData[] current = data.onGrabbedActions ?? Array.Empty<MissionActionData>();
                    Array.Resize(ref current, current.Length + 1);
                    current[current.Length - 1] = instance;
                    data.onGrabbedActions = current;
                    MissionGraphEditorFieldUtility.ApplyChange(true, context);
                });
            }

            if (!hasItem)
                menu.AddDisabledItem(new GUIContent("No managed action type found"));

            menu.ShowAsContext();
        }

        private static bool DrawReactionCard(WaitForObjectGrabbedMissionActionData data, MissionActionData[] reactions, int reactionIndex, MissionStepActionEntry reactionEntry, MissionStepActionEntry parentAction, MissionInlineEditorContext context)
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
                        data.onGrabbedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex - 1);

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("↓", false, MissionGraphEditorFieldUtility.GetButtonWidth(), reactionIndex < reactions.Length - 1))
                        data.onGrabbedActions = MissionGraphEditorCollectionUtility.Move(reactions, reactionIndex, reactionIndex + 1);

                    if (MissionGraphEditorFieldUtility.DrawSettingsButton("X", true, MissionGraphEditorFieldUtility.GetButtonWidth()))
                    {
                        data.onGrabbedActions = MissionGraphEditorCollectionUtility.RemoveAt(reactions, reactionIndex);
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

        private static bool GetReactionFoldoutState(MissionActionData reaction)
        {
            if (reaction == null)
                return true;
            if (ReactionFoldoutStates.TryGetValue(reaction, out bool value))
                return value;
            return true;
        }
    }
}
