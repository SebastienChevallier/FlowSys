using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(WaitForUIButtonClickMissionActionData))]
    internal sealed class WaitForUIButtonClickMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForUIButtonClickMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.targetUIButton = (MissionUIButtonType)EditorGUILayout.EnumPopup("Target Button", data.targetUIButton);
            data.userActionId = EditorGUILayout.TextField("User Action ID", data.userActionId);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not WaitForUIButtonClickMissionActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateEnumField(
                "Target Button",
                data.targetUIButton,
                value =>
                {
                    data.targetUIButton = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "User Action ID",
                data.userActionId,
                value =>
                {
                    data.userActionId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
