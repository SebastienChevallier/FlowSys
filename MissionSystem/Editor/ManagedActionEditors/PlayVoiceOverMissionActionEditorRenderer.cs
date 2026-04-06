using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(PlayVoiceOverMissionActionData))]
    internal sealed class PlayVoiceOverMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayVoiceOverMissionActionData data)
                return false;

            bool changed = false;

            string previousVoiceOverKey = data.voiceOverKey;
            MissionGraphEditorFieldUtility.DrawVoiceOverSelectorField("Voice Over Key", data.voiceOverKey, value => data.voiceOverKey = value);
            if (!string.Equals(previousVoiceOverKey, data.voiceOverKey, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.waitForCompletion = EditorGUILayout.Toggle("Wait For Completion", data.waitForCompletion);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayVoiceOverMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateVoiceOverPopupField(
                "Voice Over Key",
                data.voiceOverKey,
                value =>
                {
                    data.voiceOverKey = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Wait For Completion",
                data.waitForCompletion,
                value =>
                {
                    data.waitForCompletion = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
