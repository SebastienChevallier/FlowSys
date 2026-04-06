using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class ShowTextMissionActionData : MissionActionData
    {
        public TextDisplayType textDisplayType = TextDisplayType.Dialogue;
        public ManagedLocalizedTextReference textReference = new ManagedLocalizedTextReference();
        public bool teleportPlayerOnShowText;
        public string transformReferenceId;
        public bool showCanvas = true;
        public bool showTextPanel = true;
        public bool showButtonsPanel;
        public bool enableTypewriterEffect = true;
        public ManagedMissionUIButtonItem[] uiButtonItems = Array.Empty<ManagedMissionUIButtonItem>();

        public override string GetDisplayName()
        {
            string value = textReference != null ? textReference.ResolveText() : string.Empty;
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            if (value.Length > 20)
                value = value.Substring(0, 20) + "...";
            return teleportPlayerOnShowText && !string.IsNullOrEmpty(transformReferenceId)
                ? $"Show Text ({textDisplayType}) + Move UI '{transformReferenceId}': '{value}'"
                : $"Show Text ({textDisplayType}): '{value}'";
        }

        public override string GetTypeName() => nameof(ShowTextMissionActionData);
        public override bool IsAsync => ShouldWaitForContinuation();

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (teleportPlayerOnShowText && !string.IsNullOrEmpty(transformReferenceId))
            {
                MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
                if (targetTransform != null)
                {
                    MissionTextUIManager uiManager = MissionTextUIManager.Instance;
                    if (uiManager != null)
                        uiManager.MoveTo(targetTransform.GetPosition(), targetTransform.GetRotation());
                }
            }

            MissionUIButtonConfiguration[] buttonConfigurations = BuildButtonConfigurations();
            context.ShowMissionUI(textReference != null ? textReference.ResolveText() : string.Empty, showCanvas, showTextPanel, showButtonsPanel, buttonConfigurations, enableTypewriterEffect);

            MissionUIButtonConfiguration nextButtonConfiguration = GetVisibleButtonConfiguration(MissionUIButtonType.Suivant, buttonConfigurations);
            if (showCanvas && showButtonsPanel && nextButtonConfiguration != null)
            {
                bool isBound = context.BindMissionUIButton(MissionUIButtonType.Suivant, onComplete, nextButtonConfiguration.userActionId);
                if (!isBound)
                {
                    Debug.LogError("[MissionSystem] Cannot wait for ShowText continuation: 'Suivant' button is not available");
                    onComplete?.Invoke();
                }
                return;
            }

            onComplete?.Invoke();
        }

        private bool ShouldWaitForContinuation()
        {
            if (!showCanvas || !showButtonsPanel || uiButtonItems == null || uiButtonItems.Length == 0)
                return false;

            for (int i = 0; i < uiButtonItems.Length; i++)
            {
                ManagedMissionUIButtonItem item = uiButtonItems[i];
                if (item != null && item.visible && item.buttonType == MissionUIButtonType.Suivant)
                    return true;
            }

            return false;
        }

        private MissionUIButtonConfiguration[] BuildButtonConfigurations()
        {
            if (uiButtonItems == null || uiButtonItems.Length == 0)
                return Array.Empty<MissionUIButtonConfiguration>();

            List<MissionUIButtonConfiguration> configurations = new List<MissionUIButtonConfiguration>(uiButtonItems.Length);
            for (int i = 0; i < uiButtonItems.Length; i++)
            {
                ManagedMissionUIButtonItem item = uiButtonItems[i];
                if (item == null)
                    continue;

                configurations.Add(new MissionUIButtonConfiguration
                {
                    buttonType = item.buttonType,
                    visible = item.visible,
                    label = item.label,
                    userActionId = item.userActionId
                });
            }

            return configurations.ToArray();
        }

        private MissionUIButtonConfiguration GetVisibleButtonConfiguration(MissionUIButtonType buttonType, MissionUIButtonConfiguration[] buttonConfigurations)
        {
            if (buttonConfigurations == null)
                return null;

            for (int i = 0; i < buttonConfigurations.Length; i++)
            {
                MissionUIButtonConfiguration configuration = buttonConfigurations[i];
                if (configuration != null && configuration.visible && configuration.buttonType == buttonType)
                    return configuration;
            }

            return null;
        }
    }
}
