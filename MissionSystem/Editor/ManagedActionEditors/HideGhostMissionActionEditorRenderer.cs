using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(HideGhostMissionActionData))]
    internal sealed class HideGhostMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not HideGhostMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            MissionObjectIdEditorUtility.DrawObjectIdPopup("Ghost Object ID", data.ghostObjectId, value => data.ghostObjectId = value);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not HideGhostMissionActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIUtility.CreateObjectIdPopupField(
                "Ghost Object ID",
                data.ghostObjectId,
                value =>
                {
                    data.ghostObjectId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
