using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ControlTrainerMissionActionData))]
    internal sealed class ControlTrainerMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ControlTrainerMissionActionData data)
                return false;

            bool changed = false;

            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Trainer Object", data.objectId, value => data.objectId = value);
            if (!string.Equals(previousObjectId, data.objectId, System.StringComparison.Ordinal))
                changed = true;

            string previousTransformReferenceId = data.transformReferenceId;
            MissionTransformReferenceUtility.DrawTransformReferenceSelector("Transform Reference", data.transformReferenceId, value => data.transformReferenceId = value);
            if (!string.Equals(previousTransformReferenceId, data.transformReferenceId, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.trainerVisibilityMode = (TrainerVisibilityMode)EditorGUILayout.EnumPopup("Trainer Visibility", data.trainerVisibilityMode);
            data.useCustomTrainerAnimation = EditorGUILayout.Toggle("Custom Animation", data.useCustomTrainerAnimation);
            data.animationName = EditorGUILayout.TextField("Animation Name", data.animationName);
            data.syncTrainerWithVoiceOver = EditorGUILayout.Toggle("Sync With Voice Over", data.syncTrainerWithVoiceOver);
            string previousVoiceOverKey = data.voiceOverKey;
            MissionGraphEditorFieldUtility.DrawVoiceOverSelectorField("Voice Over Key", data.voiceOverKey, value => data.voiceOverKey = value);
            if (!string.Equals(previousVoiceOverKey, data.voiceOverKey, System.StringComparison.Ordinal))
                changed = true;
            data.waitForCompletion = EditorGUILayout.Toggle("Wait For Completion", data.waitForCompletion);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ControlTrainerMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                "Trainer Object",
                data.objectId,
                value =>
                {
                    data.objectId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

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

            root.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Trainer Visibility",
                data.trainerVisibilityMode,
                value =>
                {
                    data.trainerVisibilityMode = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Custom Animation",
                data.useCustomTrainerAnimation,
                value =>
                {
                    data.useCustomTrainerAnimation = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Animation Name",
                data.animationName,
                value =>
                {
                    data.animationName = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Sync With Voice Over",
                data.syncTrainerWithVoiceOver,
                value =>
                {
                    data.syncTrainerWithVoiceOver = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

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
