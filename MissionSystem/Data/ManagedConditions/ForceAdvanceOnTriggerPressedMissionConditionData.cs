using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Input")]
    [Serializable]
    public sealed class ForceAdvanceOnTriggerPressedMissionConditionData : MissionConditionData
    {
        public XRControllerHand triggerHand = XRControllerHand.Either;
        [Range(0f, 1f)]
        public float triggerThreshold = 0.5f;

        public override string GetDisplayName()
        {
            return $"Force Advance On Trigger ({triggerHand}, > {triggerThreshold:0.00})";
        }

        public override string GetTypeName()
        {
            return nameof(ForceAdvanceOnTriggerPressedMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return false;
        }
    }
}
