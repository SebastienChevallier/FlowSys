using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(SetAnimatorFloatMissionActionData))]
    internal sealed class SetAnimatorFloatMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not SetAnimatorFloatMissionActionData data)
                return false;

            bool changed = false;

            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", data.objectId, value => data.objectId = value);
            if (!string.Equals(previousObjectId, data.objectId, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.animatorParameterName = EditorGUILayout.TextField("Parameter Name", data.animatorParameterName);
            data.animatorFloatValue = EditorGUILayout.FloatField("Float Value", data.animatorFloatValue);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not SetAnimatorFloatMissionActionData data)
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
                "Parameter Name",
                data.animatorParameterName,
                value =>
                {
                    data.animatorParameterName = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Float Value",
                data.animatorFloatValue,
                value =>
                {
                    data.animatorFloatValue = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
