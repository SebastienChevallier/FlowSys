using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Interaction")]
    [Serializable]
    public sealed class WaitForUIButtonClickMissionActionData : MissionActionData
    {
        public MissionUIButtonType targetUIButton;
        public string userActionId;

        public override string GetDisplayName()
        {
            return $"Wait UI Button '{targetUIButton}'";
        }

        public override string GetTypeName()
        {
            return nameof(WaitForUIButtonClickMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            bool isBound = context != null && context.BindMissionUIButton(targetUIButton, onComplete, userActionId);
            if (isBound)
                return;

            Debug.LogError($"[MissionSystem] Cannot wait for UI button click: '{targetUIButton}' is not available");
            onComplete?.Invoke();
        }
    }
}
