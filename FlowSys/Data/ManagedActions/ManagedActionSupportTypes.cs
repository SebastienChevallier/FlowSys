using System;
using UnityEngine.Localization;

namespace GAME.FlowSys
{
    /// <summary>
    /// Shared enums and types used by actions and the core mission system.
    /// </summary>

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

    /// <summary>
    /// A serializable reference to either a plain text string or a localized string entry.
    /// Useful for actions that display text.
    /// </summary>
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
