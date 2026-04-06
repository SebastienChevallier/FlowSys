using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(PlayBreathingAnimationMissionActionData))]
    internal sealed class PlayBreathingAnimationMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayBreathingAnimationMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.objectId = EditorGUILayout.TextField("Object ID", data.objectId);
            data.loopAnimation = EditorGUILayout.Toggle("Loop Animation", data.loopAnimation);
            data.breathingSound = EditorGUILayout.TextField("Breathing Sound", data.breathingSound);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayBreathingAnimationMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
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
                "Loop Animation",
                data.loopAnimation,
                value =>
                {
                    data.loopAnimation = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Breathing Sound",
                data.breathingSound,
                value =>
                {
                    data.breathingSound = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
