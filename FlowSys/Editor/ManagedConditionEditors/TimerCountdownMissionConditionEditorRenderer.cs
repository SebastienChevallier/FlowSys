using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.FlowSys.Editor
{
    [MissionConditionEditorRenderer(typeof(TimerCountdownMissionConditionData))]
    internal sealed class TimerCountdownMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not TimerCountdownMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.timerId = EditorGUILayout.TextField("Timer ID", data.timerId);
            data.requireAllActionsCompleted = EditorGUILayout.Toggle("Require All Actions Completed", data.requireAllActionsCompleted);
            if (!EditorGUI.EndChangeCheck())
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not TimerCountdownMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Timer ID",
                data.timerId,
                value =>
                {
                    data.timerId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Require All Actions Completed",
                data.requireAllActionsCompleted,
                value =>
                {
                    data.requireAllActionsCompleted = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
