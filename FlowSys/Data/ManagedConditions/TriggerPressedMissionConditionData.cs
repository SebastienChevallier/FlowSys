using System;
using UnityEngine;

namespace GAME.FlowSys
{
    [MissionActionCategory("Input")]
    [Serializable]
    public sealed class TriggerPressedMissionConditionData : MissionConditionData
    {
        public XRControllerHand triggerHand = XRControllerHand.Either;
        [Range(0f, 1f)]
        public float triggerThreshold = 0.5f;
        [NonSerialized]
        public Func<XRControllerHand, float, bool> triggerEvaluator;

        public override string GetDisplayName()
        {
            return $"Trigger Pressed ({triggerHand}, > {triggerThreshold:0.00})";
        }

        public override string GetTypeName()
        {
            return nameof(TriggerPressedMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return triggerEvaluator != null && triggerEvaluator.Invoke(triggerHand, triggerThreshold);
        }
    }
}
