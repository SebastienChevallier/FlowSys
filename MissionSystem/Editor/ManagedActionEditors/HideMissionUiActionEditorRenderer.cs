using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(HideMissionUiActionData))]
    internal sealed class HideMissionUiActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not HideMissionUiActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.hideCanvas = EditorGUILayout.Toggle("Hide Canvas", data.hideCanvas);
            data.hideTextPanel = EditorGUILayout.Toggle("Hide Text Panel", data.hideTextPanel);
            data.hideButtonsPanel = EditorGUILayout.Toggle("Hide Buttons Panel", data.hideButtonsPanel);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not HideMissionUiActionData data)
                return null;

            Color paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Hide Canvas",
                data.hideCanvas,
                value =>
                {
                    data.hideCanvas = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Hide Text Panel",
                data.hideTextPanel,
                value =>
                {
                    data.hideTextPanel = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Hide Buttons Panel",
                data.hideButtonsPanel,
                value =>
                {
                    data.hideButtonsPanel = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
