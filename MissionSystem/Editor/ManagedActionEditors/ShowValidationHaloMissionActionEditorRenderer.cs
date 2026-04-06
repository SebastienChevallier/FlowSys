using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ShowValidationHaloMissionActionData))]
    internal sealed class ShowValidationHaloMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowValidationHaloMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", data.objectId, value => data.objectId = value);
            data.isValidPosition = EditorGUILayout.Toggle("Is Valid Position", data.isValidPosition);
            data.haloDuration = EditorGUILayout.FloatField("Halo Duration", data.haloDuration);
            bool changed = EditorGUI.EndChangeCheck();
            return MissionGraphEditorFieldUtility.ApplyChange(changed || !string.Equals(previousObjectId, data.objectId, System.StringComparison.Ordinal), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowValidationHaloMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

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

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Is Valid Position",
                data.isValidPosition,
                value =>
                {
                    data.isValidPosition = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Halo Duration",
                data.haloDuration,
                value =>
                {
                    data.haloDuration = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
