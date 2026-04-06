using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class ShowVitalSignsIndicatorMissionActionData : MissionActionData
    {
        public string indicatorType = "oscilloscope";
        public float signalStrength;

        public override string GetDisplayName()
        {
            return $"Show Vital Signs '{indicatorType}' (strength={signalStrength})";
        }

        public override string GetTypeName() => nameof(ShowVitalSignsIndicatorMissionActionData);
        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionTextUIManager uiManager = MissionTextUIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogError("[MissionSystem] Cannot show vital signs indicator: UIManager not found");
                onComplete?.Invoke();
                return;
            }

            VitalSignsIndicator indicator = uiManager.GetVitalSignsIndicator();
            if (indicator == null)
            {
                Debug.LogError("[MissionSystem] Cannot show vital signs indicator: VitalSignsIndicator component not found in UIManager");
                onComplete?.Invoke();
                return;
            }

            indicator.Show(indicatorType, signalStrength);
            onComplete?.Invoke();
        }
    }
}
