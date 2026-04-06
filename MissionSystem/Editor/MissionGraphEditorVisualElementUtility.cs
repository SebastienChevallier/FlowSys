using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    internal static class MissionGraphEditorVisualElementUtility
    {
        internal static VisualElement CreateSectionContainer(Color bodyColor, Color nodeBackgroundColor)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = 3;
            section.style.borderTopWidth = 1;
            section.style.borderRightWidth = 1;
            section.style.borderBottomWidth = 1;
            section.style.borderLeftWidth = 1;
            Color sectionBorderColor = Color.Lerp(nodeBackgroundColor, Color.black, 0.55f);
            section.style.borderTopColor = sectionBorderColor;
            section.style.borderRightColor = sectionBorderColor;
            section.style.borderBottomColor = sectionBorderColor;
            section.style.borderLeftColor = sectionBorderColor;
            section.style.backgroundColor = bodyColor;
            section.style.paddingBottom = 4;
            section.style.borderTopLeftRadius = 4;
            section.style.borderTopRightRadius = 4;
            section.style.borderBottomLeftRadius = 4;
            section.style.borderBottomRightRadius = 4;
            return section;
        }

        internal static VisualElement CreateSectionHeader(string title, bool isExpanded, Action<bool> onExpandedChanged, Color headerColor, Color textColor, int fontSize, Button addButton = null)
        {
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.backgroundColor = headerColor;
            headerRow.style.paddingLeft = 6;
            headerRow.style.paddingRight = 4;
            headerRow.style.paddingTop = 3;
            headerRow.style.paddingBottom = 3;
            headerRow.style.borderTopLeftRadius = 4;
            headerRow.style.borderTopRightRadius = 4;

            Foldout sectionFoldout = new Foldout();
            sectionFoldout.text = title;
            sectionFoldout.value = isExpanded;
            sectionFoldout.style.flexGrow = 1;
            sectionFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionFoldout.style.fontSize = fontSize;
            sectionFoldout.style.color = textColor;
            sectionFoldout.RegisterValueChangedCallback(evt => onExpandedChanged?.Invoke(evt.newValue));

            headerRow.Add(sectionFoldout);
            if (addButton != null)
                headerRow.Add(addButton);

            return headerRow;
        }

        internal static Label CreateEmptyStateLabel(string text, Color textColor, int fontSize)
        {
            Label emptyLabel = new Label(text);
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            emptyLabel.style.color = textColor;
            emptyLabel.style.fontSize = fontSize;
            emptyLabel.style.paddingLeft = 8;
            emptyLabel.style.paddingRight = 8;
            emptyLabel.style.paddingTop = 6;
            emptyLabel.style.paddingBottom = 4;
            return emptyLabel;
        }

        internal static VisualElement CreateCard(Color cardColor, Color nodeBackgroundColor)
        {
            VisualElement card = new VisualElement();
            card.style.marginLeft = 4;
            card.style.marginRight = 4;
            card.style.marginTop = 4;
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            Color cardBorderColor = Color.Lerp(nodeBackgroundColor, Color.black, 0.62f);
            card.style.borderTopColor = cardBorderColor;
            card.style.borderRightColor = cardBorderColor;
            card.style.borderBottomColor = cardBorderColor;
            card.style.borderLeftColor = cardBorderColor;
            card.style.backgroundColor = cardColor;
            card.style.borderTopLeftRadius = 3;
            card.style.borderTopRightRadius = 3;
            card.style.borderBottomLeftRadius = 3;
            card.style.borderBottomRightRadius = 3;
            return card;
        }

        internal static VisualElement CreateCardHeader(Color cardColor)
        {
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.paddingLeft = 4;
            headerRow.style.paddingRight = 2;
            headerRow.style.paddingTop = 2;
            headerRow.style.paddingBottom = 2;
            headerRow.style.backgroundColor = cardColor;
            return headerRow;
        }

        internal static IMGUIContainer CreateInspectorBody(Action drawGUI)
        {
            IMGUIContainer body = new IMGUIContainer(() => drawGUI?.Invoke());
            body.style.paddingLeft = 8;
            body.style.paddingRight = 8;
            body.style.paddingBottom = 6;
            body.style.paddingTop = 4;
            return body;
        }

        internal static VisualElement CreateSummaryContainer()
        {
            VisualElement container = new VisualElement();
            container.style.paddingLeft = 8;
            container.style.paddingRight = 6;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 2;
            return container;
        }

        internal static VisualElement CreateSummaryRow(string label, Port port, Color textColor, int fontSize)
        {
            if (port == null)
                return null;

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 2;
            row.style.marginBottom = 2;

            Label summaryLabel = new Label(label);
            summaryLabel.style.flexGrow = 1;
            summaryLabel.style.fontSize = fontSize;
            summaryLabel.style.color = textColor;
            summaryLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryLabel.style.marginRight = 6;

            port.RemoveFromHierarchy();
            port.style.marginLeft = 0;
            port.style.marginRight = 0;

            row.Add(summaryLabel);
            row.Add(port);
            return row;
        }

        internal static Button CreateIconButton(string text, Action onClick, float width, bool isDelete, bool isInteractive)
        {
            Button button = new Button(() =>
            {
                if (isInteractive)
                    onClick?.Invoke();
            })
            {
                text = text
            };
            ApplyNodeButtonStyle(button, width, isDelete, isInteractive);
            return button;
        }

        internal static void ApplyNodeButtonStyle(Button button, float width, bool isDelete, bool isInteractive = true)
        {
            if (button == null)
                return;

            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            Color baseColor = isDelete
                ? (settings != null ? settings.deleteButtonColor : new Color(0.8f, 0.2f, 0.2f, 1f))
                : (settings != null ? settings.addButtonColor : new Color(0.2f, 0.7f, 0.2f, 1f));
            Color textColor = isDelete
                ? (settings != null ? settings.deleteButtonTextColor : Color.white)
                : (settings != null ? settings.addButtonTextColor : Color.white);

            Texture2D backgroundTexture = MissionGraphIMGUIStyleUtility.GetOrCreateSolidTexture(baseColor);

            button.style.width = width;
            button.style.height = 20;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.backgroundColor = baseColor;
            button.style.backgroundImage = new StyleBackground(backgroundTexture);
            button.style.unityBackgroundImageTintColor = Color.white;
            button.style.color = textColor;
            button.style.opacity = 1f;
            button.style.borderTopWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.borderTopLeftRadius = 2;
            button.style.borderTopRightRadius = 2;
            button.style.borderBottomLeftRadius = 2;
            button.style.borderBottomRightRadius = 2;
            button.SetEnabled(true);
            button.pickingMode = isInteractive ? PickingMode.Position : PickingMode.Ignore;
        }
    }
}
