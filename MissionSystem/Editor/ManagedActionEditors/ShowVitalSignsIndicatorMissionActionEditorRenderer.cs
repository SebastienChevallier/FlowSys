using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ShowVitalSignsIndicatorMissionActionData))]
    internal sealed class ShowVitalSignsIndicatorMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowVitalSignsIndicatorMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.indicatorType = EditorGUILayout.TextField("Indicator Type", data.indicatorType);
            data.signalStrength = EditorGUILayout.FloatField("Signal Strength", data.signalStrength);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowVitalSignsIndicatorMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Indicator Type",
                data.indicatorType,
                value =>
                {
                    data.indicatorType = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Signal Strength",
                data.signalStrength,
                value =>
                {
                    data.signalStrength = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
