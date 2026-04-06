using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.FlowSys.Editor
{
    [MissionConditionEditorRenderer(typeof(AllStepActionsCompletedMissionConditionData))]
    internal sealed class AllStepActionsCompletedMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not AllStepActionsCompletedMissionConditionData)
                return false;

            EditorGUILayout.LabelField("Condition Type", "All Step Actions Completed");
            EditorGUILayout.HelpBox("This condition has no editable parameters. Configure its outgoing transition directly from the graph connector.", MessageType.None);
            return false;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not AllStepActionsCompletedMissionConditionData)
                return null;

            VisualElement root = new VisualElement();

            Label titleLabel = new Label("Condition Type");
            titleLabel.style.color = MissionGraphEditorUIUtility.GetParameterTextColor();
            titleLabel.style.fontSize = MissionGraphEditorUIUtility.GetParameterFontSize();
            titleLabel.style.marginBottom = 4;
            root.Add(titleLabel);

            Label valueLabel = new Label("All Step Actions Completed");
            valueLabel.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.7f);
            valueLabel.style.fontSize = MissionGraphEditorUIUtility.GetParameterFontSize();
            valueLabel.style.marginLeft = 4;
            root.Add(valueLabel);

            Label helpLabel = new Label("This condition has no editable parameters. Configure its outgoing transition directly from the graph connector.");
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            helpLabel.style.fontSize = MissionGraphEditorUIUtility.GetParameterFontSize() - 1;
            helpLabel.style.color = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
            helpLabel.style.marginTop = 8;
            helpLabel.style.paddingLeft = 4;
            helpLabel.style.paddingRight = 4;
            helpLabel.style.paddingTop = 4;
            helpLabel.style.paddingBottom = 4;
            helpLabel.style.backgroundColor = new UnityEngine.Color(0.2f, 0.2f, 0.2f, 0.5f);
            root.Add(helpLabel);

            return root;
        }
    }
}
