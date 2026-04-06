using System;
using UnityEngine.UIElements;

namespace GAME.FlowSys.Editor
{
    /// <summary>
    /// Adapter for progressive renderer migration from IMGUI to UI Toolkit.
    ///
    /// Allows renderers to optionally implement IMissionUIElementRenderer
    /// while still supporting the legacy IMissionActionEditorRenderer interface.
    ///
    /// During Phase 2-4 of migration:
    /// 1. Renderer implements both interfaces
    /// 2. MissionStepNode checks for UI Toolkit support first
    /// 3. Falls back to IMGUI if UI Toolkit not available
    /// 4. Once all renderers migrated, remove IMGUI support
    /// </summary>
    internal static class MissionRendererAdapter
    {
        /// <summary>
        /// Tries to get UI Toolkit renderer for an action.
        /// Returns true if renderer supports UI Toolkit, false if IMGUI-only.
        /// </summary>
        public static bool TryGetUIElementRenderer(
            Type managedType,
            out IMissionUIElementRenderer uiRenderer)
        {
            uiRenderer = null;

            if (managedType == null)
                return false;

            // Try to get the IMGUI renderer first (all renderers are IMGUI-based currently)
            if (!MissionManagedTypeEditorRegistry.TryGetActionRenderer(managedType, out IMissionActionEditorRenderer imgui))
                return false;

            // Check if it also implements the UI Toolkit interface
            if (imgui is IMissionUIElementRenderer uiElementRenderer)
            {
                uiRenderer = uiElementRenderer;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to get UI Toolkit renderer for a condition.
        /// Returns true if renderer supports UI Toolkit, false if IMGUI-only.
        /// </summary>
        public static bool TryGetUIConditionElementRenderer(
            Type managedType,
            out IMissionUIConditionElementRenderer uiRenderer)
        {
            uiRenderer = null;

            if (managedType == null)
                return false;

            // Try to get the IMGUI renderer first
            if (!MissionManagedTypeEditorRegistry.TryGetConditionRenderer(managedType, out IMissionConditionEditorRenderer imgui))
                return false;

            // Check if it also implements the UI Toolkit interface
            if (imgui is IMissionUIConditionElementRenderer uiElementRenderer)
            {
                uiRenderer = uiElementRenderer;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Wraps an action drawBody function in a VisualElement container.
        /// Used for transitioning IMGUI renderers to UI Toolkit during migration.
        /// </summary>
        public static VisualElement WrapIMGUIInContainer(Action drawGUI)
        {
            if (drawGUI == null)
                return null;

            IMGUIContainer container = new IMGUIContainer(() => drawGUI?.Invoke());
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.paddingBottom = 6;
            container.style.paddingTop = 4;
            return container;
        }

        public static VisualElement BuildManagedActionElement(MissionStepActionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedAction == null)
                return new Label("Missing reaction instance.");

            Type managedType = entry.managedAction.GetType();

            if (TryGetUIElementRenderer(managedType, out IMissionUIElementRenderer uiRenderer))
            {
                VisualElement uiElement = uiRenderer.BuildElement(entry, context);
                if (uiElement != null)
                    return uiElement;
            }

            if (MissionManagedTypeEditorRegistry.TryGetActionRenderer(managedType, out IMissionActionEditorRenderer imguiRenderer) && imguiRenderer != null)
                return WrapIMGUIInContainer(() => imguiRenderer.Draw(entry, context));

            return new Label($"No editor renderer registered for {managedType.Name}.");
        }
    }
}
