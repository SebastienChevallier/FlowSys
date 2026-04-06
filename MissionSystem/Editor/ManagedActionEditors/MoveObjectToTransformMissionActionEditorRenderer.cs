using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(MoveObjectToTransformMissionActionData))]
    internal sealed class MoveObjectToTransformMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not MoveObjectToTransformMissionActionData data)
                return false;

            bool changed = false;

            string previousObjectId = data.objectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Object ID", data.objectId, value => data.objectId = value);
            if (!string.Equals(previousObjectId, data.objectId, System.StringComparison.Ordinal))
                changed = true;

            string previousTransformReferenceId = data.transformReferenceId;
            MissionTransformReferenceUtility.DrawTransformReferenceSelector("Transform Reference", data.transformReferenceId, value => data.transformReferenceId = value);
            if (!string.Equals(previousTransformReferenceId, data.transformReferenceId, System.StringComparison.Ordinal))
                changed = true;

            EditorGUI.BeginChangeCheck();
            data.moveDuration = EditorGUILayout.FloatField("Move Duration", data.moveDuration);
            data.useLocalSpace = EditorGUILayout.Toggle("Use Local Space", data.useLocalSpace);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not MoveObjectToTransformMissionActionData data)
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

            root.Add(MissionGraphEditorUIComponents.CreateFloatField(
                "Move Duration",
                data.moveDuration,
                value =>
                {
                    data.moveDuration = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Use Local Space",
                data.useLocalSpace,
                value =>
                {
                    data.useLocalSpace = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
