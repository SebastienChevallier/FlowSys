using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(PlayAudioManagerSoundMissionActionData))]
    internal sealed class PlayAudioManagerSoundMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayAudioManagerSoundMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.audioManagerSoundId = EditorGUILayout.TextField("Sound", data.audioManagerSoundId);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayAudioManagerSoundMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Sound",
                data.audioManagerSoundId,
                value =>
                {
                    data.audioManagerSoundId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
