using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionConditionEditorRenderer(typeof(GrabObjectMissionConditionData))]
    internal sealed class GrabObjectMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not GrabObjectMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            string previousObjectId = data.grabbableObjectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Grabbable Object", data.grabbableObjectId, value => data.grabbableObjectId = value);
            bool changed = !string.Equals(previousObjectId, data.grabbableObjectId, System.StringComparison.Ordinal);

            if (!EditorGUI.EndChangeCheck() && !changed)
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not GrabObjectMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                "Grabbable Object",
                data.grabbableObjectId,
                value =>
                {
                    data.grabbableObjectId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
