using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(StopVoiceOverMissionActionData))]
    internal sealed class StopVoiceOverMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not StopVoiceOverMissionActionData)
                return false;

            // This action has no configurable parameters
            return false;
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not StopVoiceOverMissionActionData)
                return null;

            var root = new VisualElement();
            root.Add(new Label("Stops the currently playing voice over immediately."));

            return root;
        }
    }
}
