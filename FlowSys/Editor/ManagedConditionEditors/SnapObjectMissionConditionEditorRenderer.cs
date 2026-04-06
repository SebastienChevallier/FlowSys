using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.FlowSys.Editor
{
    [MissionConditionEditorRenderer(typeof(SnapObjectMissionConditionData))]
    internal sealed class SnapObjectMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not SnapObjectMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            string previousObjectId = data.snapableObjectId;
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Snapable Object", data.snapableObjectId, value => data.snapableObjectId = value);
            bool changed = !string.Equals(previousObjectId, data.snapableObjectId, System.StringComparison.Ordinal);

            if (!EditorGUI.EndChangeCheck() && !changed)
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not SnapObjectMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                "Snapable Object",
                data.snapableObjectId,
                value =>
                {
                    data.snapableObjectId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
