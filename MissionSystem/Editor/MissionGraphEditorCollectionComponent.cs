using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    /// <summary>
    /// UI Toolkit collection management components.
    /// Provides reusable elements for editing arrays and lists in the editor.
    /// Replaces the IMGUI patterns for collection editing.
    /// </summary>
    internal static class MissionGraphEditorCollectionComponent
    {
        /// <summary>
        /// Creates a collection item container with controls (move, remove).
        /// </summary>
        public static VisualElement CreateCollectionItemEditor(
            int index,
            string label,
            VisualElement fieldElement,
            Action onMoveUp,
            Action onMoveDown,
            Action onRemove,
            bool canMoveUp,
            bool canMoveDown)
        {
            VisualElement container = new VisualElement();
            container.style.marginLeft = 4;
            container.style.marginRight = 4;
            container.style.marginTop = 2;
            container.style.marginBottom = 2;
            container.style.paddingLeft = 4;
            container.style.paddingRight = 4;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            container.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            container.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);

            // Header with label and controls
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 4;

            Label itemLabel = new Label($"{label} {index}");
            itemLabel.style.flexGrow = 1;
            itemLabel.style.color = Color.white;
            itemLabel.style.fontSize = 11;
            itemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Control buttons
            VisualElement buttonGroup = new VisualElement();
            buttonGroup.style.flexDirection = FlexDirection.Row;

            Button upButton = MissionGraphEditorUIComponents.CreateIconButton(
                "↑",
                onMoveUp,
                20,
                false,
                canMoveUp);

            Button downButton = MissionGraphEditorUIComponents.CreateIconButton(
                "↓",
                onMoveDown,
                20,
                false,
                canMoveDown);

            Button removeButton = MissionGraphEditorUIComponents.CreateIconButton(
                "✕",
                onRemove,
                20,
                true,
                true);

            buttonGroup.Add(upButton);
            buttonGroup.Add(downButton);
            buttonGroup.Add(removeButton);

            header.Add(itemLabel);
            header.Add(buttonGroup);

            container.Add(header);

            // Field element
            if (fieldElement != null)
            {
                container.Add(fieldElement);
            }

            return container;
        }

        /// <summary>
        /// Creates a collection section with add button and items list.
        /// </summary>
        public static VisualElement CreateCollectionSection(
            string sectionTitle,
            int itemCount,
            Action<int> onCreateItem,
            Func<int, VisualElement> buildItemElement,
            Color textColor,
            int fontSize)
        {
            VisualElement section = new VisualElement();

            // Title
            Label titleLabel = MissionGraphEditorUIComponents.CreateSectionHeaderLabel(sectionTitle, textColor);
            section.Add(titleLabel);

            // Items container
            VisualElement itemsContainer = new VisualElement();
            itemsContainer.style.paddingLeft = 4;

            for (int i = 0; i < itemCount; i++)
            {
                int itemIndex = i;
                VisualElement itemElement = buildItemElement?.Invoke(itemIndex);
                if (itemElement != null)
                {
                    itemsContainer.Add(itemElement);
                }
            }

            section.Add(itemsContainer);

            // Add button
            Button addButton = new Button(() => onCreateItem?.Invoke(itemCount))
            {
                text = $"+ Add {sectionTitle.TrimEnd('s')}"
            };
            addButton.style.marginTop = 4;
            addButton.style.marginLeft = 4;
            addButton.style.marginRight = 4;
            addButton.style.color = textColor;
            addButton.style.fontSize = fontSize;

            section.Add(addButton);

            return section;
        }

        /// <summary>
        /// Creates an empty state container when there are no items.
        /// </summary>
        public static VisualElement CreateEmptyCollectionState(
            string message,
            Action onAddItem,
            Color textColor,
            int fontSize)
        {
            VisualElement container = new VisualElement();
            container.style.paddingTop = 8;
            container.style.paddingBottom = 8;
            container.style.paddingLeft = 4;
            container.style.paddingRight = 4;

            Label emptyLabel = MissionGraphEditorUIComponents.CreateEmptyStateLabel(message, textColor, fontSize);
            container.Add(emptyLabel);

            Button addButton = new Button(onAddItem)
            {
                text = "Add Item"
            };
            addButton.style.marginTop = 8;
            addButton.style.color = textColor;
            addButton.style.fontSize = fontSize;

            container.Add(addButton);

            return container;
        }
    }
}
