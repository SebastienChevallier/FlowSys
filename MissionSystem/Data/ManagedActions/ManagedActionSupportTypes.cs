using System;
using UnityEngine;
using UnityEngine.Localization;

namespace GAME.MissionSystem
{
    public enum XRControllerHand
    {
        Either,
        Left,
        Right
    }

    public enum TextSourceMode
    {
        PlainText,
        LocalizationEntry
    }

    public enum TrainerVisibilityMode
    {
        NoChange,
        Show,
        Hide
    }

    // Deprecated: kept for backward compatibility
    [Obsolete("Use TextSourceMode instead", false)]
    public enum InlineTextSourceMode
    {
        PlainText,
        LocalizationEntry
    }

    [Serializable]
    public sealed class ManagedUserActionVoiceOverItem
    {
        public string userActionId;
        public string voiceOverKey;
    }

    [Serializable]
    public sealed class ManagedMissionUIButtonItem
    {
        public MissionUIButtonType buttonType;
        public bool visible = true;
        public string label;
        public string userActionId;
    }

    [Serializable]
    public sealed class ManagedObjectPickReactionItem
    {
        public string objectId;
        public string voiceOverKey;
        [SerializeReference]
        public MissionActionData[] onPickedActions = Array.Empty<MissionActionData>();
    }

    [Serializable]
    public sealed class ManagedLocalizedTextReference
    {
        public TextSourceMode textSourceMode = TextSourceMode.PlainText;
        public string textContent;
        public LocalizedString localizedText = new LocalizedString();

        public string ResolveText()
        {
            if (textSourceMode == TextSourceMode.LocalizationEntry)
            {
                if (localizedText == null)
                    return string.Empty;

                string localizedValue = localizedText.GetLocalizedString();
                return string.IsNullOrEmpty(localizedValue) ? string.Empty : localizedValue;
            }

            return textContent ?? string.Empty;
        }
    }
}
