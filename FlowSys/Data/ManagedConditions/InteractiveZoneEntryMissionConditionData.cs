using System;
using UnityEngine;

namespace GAME.FlowSys
{
    [System.Flags]
    public enum MissionControllerType
    {
        None = 0,
        Left = 1,
        Right = 2,
        Head = 4,
        LeftRight = Left | Right,
        All = Left | Right | Head
    }

    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class InteractiveZoneEntryMissionConditionData : MissionConditionData
    {
        public string zoneId;
        public MissionControllerType allowedControllers = MissionControllerType.All;

        [NonSerialized]
        public Func<string, MissionControllerType, bool> zoneEntryEvaluator;

        public override string GetDisplayName()
        {
            string controllers = allowedControllers.ToString();
            return !string.IsNullOrEmpty(zoneId)
                ? $"Entered Zone '{zoneId}' ({controllers})"
                : "Entered Interactive Zone";
        }

        public override string GetTypeName()
        {
            return nameof(InteractiveZoneEntryMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return zoneEntryEvaluator != null && zoneEntryEvaluator.Invoke(zoneId, allowedControllers);
        }
    }
}
