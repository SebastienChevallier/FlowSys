using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class GrabObjectMissionConditionData : MissionConditionData
    {
        public string grabbableObjectId;

        [NonSerialized]
        public Func<string, bool> grabStateEvaluator;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(grabbableObjectId)
                ? $"Is Grabbed '{grabbableObjectId}'"
                : "Is Grabbed";
        }

        public override string GetTypeName()
        {
            return nameof(GrabObjectMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return grabStateEvaluator != null && grabStateEvaluator.Invoke(grabbableObjectId);
        }
    }
}
