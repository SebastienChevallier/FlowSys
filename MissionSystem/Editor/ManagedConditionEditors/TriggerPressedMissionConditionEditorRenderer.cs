using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionConditionEditorRenderer(typeof(TriggerPressedMissionConditionData))]
    internal sealed class TriggerPressedMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not TriggerPressedMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.triggerHand = (XRControllerHand)EditorGUILayout.EnumPopup("Trigger Hand", data.triggerHand);
            data.triggerThreshold = EditorGUILayout.Slider("Trigger Threshold", data.triggerThreshold, 0f, 1f);

            if (!EditorGUI.EndChangeCheck())
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not TriggerPressedMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Trigger Hand",
                data.triggerHand,
                value =>
                {
                    data.triggerHand = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateSliderField(
                "Trigger Threshold",
                data.triggerThreshold,
                0f,
                1f,
                value =>
                {
                    data.triggerThreshold = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
