using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(WaitForPlayerPositionMissionActionData))]
    internal sealed class WaitForPlayerPositionMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForPlayerPositionMissionActionData data)
                return false;

            bool changed = false;

            string previousTransformReferenceId = data.transformReferenceId;
            MissionTransformReferenceUtility.DrawTransformReferenceSelector("Transform Reference", data.transformReferenceId, value => data.transformReferenceId = value);
            if (!string.Equals(previousTransformReferenceId, data.transformReferenceId, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.validationRadius = EditorGUILayout.FloatField("Validation Radius", data.validationRadius);
            data.bodyPartToTrack = EditorGUILayout.TextField("Body Part", data.bodyPartToTrack);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForPlayerPositionMissionActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
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

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Validation Radius",
                data.validationRadius,
                value =>
                {
                    data.validationRadius = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Body Part",
                data.bodyPartToTrack,
                value =>
                {
                    data.bodyPartToTrack = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
