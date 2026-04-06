using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class SnapObjectMissionConditionData : MissionConditionData
    {
        public string snapableObjectId;

        [NonSerialized]
        public Func<string, bool> snapStateEvaluator;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(snapableObjectId)
                ? $"Is Snapped '{snapableObjectId}'"
                : "Is Snapped";
        }

        public override string GetTypeName()
        {
            return nameof(SnapObjectMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return snapStateEvaluator != null && snapStateEvaluator.Invoke(snapableObjectId);
        }
    }
}
