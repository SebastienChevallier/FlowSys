using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(EnableUserActionMissionActionData))]
    internal sealed class EnableUserActionMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not EnableUserActionMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.userActionId = EditorGUILayout.TextField("User Action ID", data.userActionId);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not EnableUserActionMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

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
