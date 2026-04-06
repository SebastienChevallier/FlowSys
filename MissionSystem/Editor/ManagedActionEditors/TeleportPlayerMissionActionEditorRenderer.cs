using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(TeleportPlayerMissionActionData))]
    internal sealed class TeleportPlayerMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not TeleportPlayerMissionActionData data)
                return false;

            string previousTransformReferenceId = data.transformReferenceId;
            MissionTransformReferenceUtility.DrawTransformReferenceSelector("Transform Reference", data.transformReferenceId, value => data.transformReferenceId = value);
            return MissionGraphEditorFieldUtility.ApplyChange(!string.Equals(previousTransformReferenceId, data.transformReferenceId, System.StringComparison.Ordinal), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not TeleportPlayerMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateTransformReferencePopupField(
                "Transform Reference",
                data.transformReferenceId,
                value =>
                {
                    data.transformReferenceId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
