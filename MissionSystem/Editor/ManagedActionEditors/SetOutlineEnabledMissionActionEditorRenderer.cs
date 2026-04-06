using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(SetOutlineEnabledMissionActionData))]
    internal sealed class SetOutlineEnabledMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context) => false;

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not SetOutlineEnabledMissionActionData data)
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

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Outline Enabled",
                data.outlineEnabled,
                value =>
                {
                    data.outlineEnabled = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateToggleField(
                "Disable On Step Exit",
                data.disableOutlineOnStepExit,
                value =>
                {
                    data.disableOutlineOnStepExit = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            // Outline Color Swatch with Label
            VisualElement colorContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4, marginBottom = 4 } };
            Label colorLabel = new Label("Color") { style = { minWidth = 100, color = paramTextColor, fontSize = paramFontSize } };
            Box colorSwatch = new Box { style = { width = 50, height = 20, backgroundColor = data.outlineColor, borderLeftWidth = 1, borderTopWidth = 1, borderRightWidth = 1, borderBottomWidth = 1 } };
            colorContainer.Add(colorLabel);
            colorContainer.Add(colorSwatch);
            root.Add(colorContainer);

            // Outline Width Slider
            Slider widthSlider = new Slider("Width", 0f, 0.1f) { value = data.outlineWidth };
            widthSlider.style.marginTop = 4;
            widthSlider.style.marginBottom = 4;
            widthSlider.RegisterValueChangedCallback(evt =>
            {
                data.outlineWidth = evt.newValue;
                context?.SaveGraphPositions?.Invoke();
                context?.Repaint?.Invoke();
            });
            root.Add(widthSlider);

            return root;
        }
    }
}
