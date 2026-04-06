using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(DelayMissionActionData))]
    internal sealed class DelayMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not DelayMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.delaySeconds = EditorGUILayout.FloatField("Delay (seconds)", data.delaySeconds);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not DelayMissionActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Delay (seconds)",
                data.delaySeconds,
                value =>
                {
                    data.delaySeconds = Mathf.Max(0f, value);
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
