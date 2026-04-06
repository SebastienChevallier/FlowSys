using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(PlayAnimationMissionActionData))]
    internal sealed class PlayAnimationMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayAnimationMissionActionData data)
                return false;

            bool changed = false;

            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", data.objectId, value => data.objectId = value);
            if (!string.Equals(previousObjectId, data.objectId, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.animationName = EditorGUILayout.TextField("Animation Name", data.animationName);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not PlayAnimationMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
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

            return root;
        }
    }
}
