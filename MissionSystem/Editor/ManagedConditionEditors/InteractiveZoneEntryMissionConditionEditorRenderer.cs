using UnityEditor;
using UnityEngine.UIElements;
using Evaveo.nsVorta.RUNTIME;

namespace GAME.MissionSystem.Editor
{
    [MissionConditionEditorRenderer(typeof(InteractiveZoneEntryMissionConditionData))]
    internal sealed class InteractiveZoneEntryMissionConditionEditorRenderer : IMissionConditionEditorRenderer, IMissionUIConditionElementRenderer
    {
        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)
        {
            if (condition?.managedCondition is not InteractiveZoneEntryMissionConditionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            string previousZoneId = data.zoneId;
            DrawZoneIdPopup("Interactive Zone", data, value => data.zoneId = value);
            bool zoneChanged = !string.Equals(previousZoneId, data.zoneId, System.StringComparison.Ordinal);

            MissionControllerType previousControllers = data.allowedControllers;
            data.allowedControllers = (MissionControllerType)EditorGUILayout.EnumFlagsField("Allowed Controllers", data.allowedControllers);
            bool controllersChanged = previousControllers != data.allowedControllers;

            if (!EditorGUI.EndChangeCheck() && !zoneChanged && !controllersChanged)
                return false;

            context?.SaveGraphPositions?.Invoke();
            context?.Repaint?.Invoke();
            return true;
        }

        private void DrawZoneIdPopup(string label, InteractiveZoneEntryMissionConditionData data, System.Action<string> onSelected)
        {
            var zones = UnityEngine.Object.FindObjectsOfType<InteractiveZone>();
            var zoneIds = new System.Collections.Generic.List<string>();
            foreach (var zone in zones)
            {
                if (!string.IsNullOrEmpty(zone.zone) && !zoneIds.Contains(zone.zone))
                    zoneIds.Add(zone.zone);
            }

            int currentIndex = zoneIds.IndexOf(data.zoneId);
            currentIndex = EditorGUILayout.Popup(label, currentIndex, zoneIds.ToArray());

            if (currentIndex >= 0 && currentIndex < zoneIds.Count)
                onSelected?.Invoke(zoneIds[currentIndex]);
        }

        private VisualElement CreateControllerFlagsField(string label, MissionControllerType currentValue, System.Action<MissionControllerType> onChanged, UnityEngine.Color paramTextColor, int paramFontSize)
        {
            var container = new VisualElement();
            container.style.marginBottom = 4;

            var labelElement = new UnityEngine.UIElements.Label(label);
            labelElement.style.color = paramTextColor;
            labelElement.style.fontSize = paramFontSize;
            labelElement.style.marginBottom = 2;
            container.Add(labelElement);

            var flagContainer = new VisualElement();
            flagContainer.style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
            flagContainer.style.paddingLeft = 4;

            foreach (MissionControllerType controller in System.Enum.GetValues(typeof(MissionControllerType)))
            {
                if (controller == MissionControllerType.None || controller == MissionControllerType.LeftRight || controller == MissionControllerType.All)
                    continue;

                var toggle = new UnityEngine.UIElements.Toggle();
                toggle.label = controller.ToString();
                toggle.value = (currentValue & controller) != 0;
                toggle.style.marginRight = 8;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        onChanged?.Invoke(currentValue | controller);
                    else
                        onChanged?.Invoke(currentValue & ~controller);
                });
                flagContainer.Add(toggle);
            }

            container.Add(flagContainer);
            return container;
        }

        public VisualElement BuildElement(MissionStepConditionEntry entry, MissionInlineEditorContext context)
        {
            if (entry?.managedCondition is not InteractiveZoneEntryMissionConditionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            // Zone ID field
            var zoneContainer = new VisualElement();
            var zoneLabel = new Label("Interactive Zone");
            zoneLabel.style.color = paramTextColor;
            zoneLabel.style.fontSize = paramFontSize;
            zoneContainer.Add(zoneLabel);

            var zoneField = new TextField();
            zoneField.value = data.zoneId ?? "";
            zoneField.RegisterValueChangedCallback(evt =>
            {
                data.zoneId = evt.newValue;
                context?.SaveGraphPositions?.Invoke();
                context?.Repaint?.Invoke();
            });
            zoneContainer.Add(zoneField);
            root.Add(zoneContainer);

            root.Add(CreateControllerFlagsField(
                "Allowed Controllers",
                data.allowedControllers,
                value =>
                {
                    data.allowedControllers = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
