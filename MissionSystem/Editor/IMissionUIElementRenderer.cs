using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    /// <summary>
    /// UI Toolkit-based renderer interface for Mission action/condition editors.
    /// Replaces the IMGUI-based IMissionActionEditorRenderer.
    ///
    /// Renderers can implement this interface to build VisualElement-based UI
    /// instead of using EditorGUILayout (IMGUI).
    ///
    /// Architecture:
    /// - Renderers inherit from this interface
    /// - BuildVisualElement() returns fully-built VisualElement
    /// - MissionStepNode orchestrates renderer selection
    /// - No business logic in MissionFlowGraphView - stays pure orchestrator
    /// - Each managed type = 1 dedicated renderer file
    /// </summary>
    internal interface IMissionUIElementRenderer
    {
        /// <summary>
        /// Builds a UI Toolkit VisualElement for the action/condition editor.
        /// Called by MissionStepNode to populate the editor panel.
        /// </summary>
        /// <param name="entry">The action entry to render</param>
        /// <param name="context">Context with save/repaint callbacks</param>
        /// <returns>VisualElement ready to display, or null to use fallback IMGUI</returns>
        VisualElement BuildElement(MissionStepActionEntry entry, MissionInlineEditorContext context);
    }

    /// <summary>
    /// UI Toolkit-based renderer interface for conditions.
    /// Parallel to IMissionUIElementRenderer for actions.
    /// </summary>
    internal interface IMissionUIConditionElementRenderer
    {
        /// <summary>
        /// Builds a UI Toolkit VisualElement for the condition editor.
        /// </summary>
        /// <param name="entry">The condition entry to render</param>
        /// <param name="context">Context with save/repaint callbacks</param>
        /// <returns>VisualElement ready to display, or null to use fallback IMGUI</returns>
        VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context);
    }

    /// <summary>
    /// Adapter: Allows renderers to optionally support both IMGUI and UI Toolkit.
    /// During migration phase, renderers can implement both interfaces.
    /// Once migration is complete, only IMissionUIElementRenderer is needed.
    /// </summary>
    internal interface IMissionDualModeRenderer
    {
        /// <summary>
        /// Indicates whether this renderer supports UI Toolkit rendering.
        /// Return false to fall back to IMGUI Draw() method.
        /// </summary>
        bool SupportsUIToolkit { get; }
    }
}
