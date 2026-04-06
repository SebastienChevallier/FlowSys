using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(StopTimerMissionActionData))]
    internal sealed class StopTimerMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not StopTimerMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.timerId = EditorGUILayout.TextField("Timer ID", data.timerId);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not StopTimerMissionActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
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

            return root;
        }
    }
}
