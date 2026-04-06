using System;

namespace GAME.FlowSys
{
    [MissionActionCategory("Flow")]
    [Serializable]
    public sealed class AllStepActionsCompletedMissionConditionData : MissionConditionData
    {
        [NonSerialized]
        public Func<bool> areActionsCompleted;

        public override string GetDisplayName()
        {
            return "All Step Actions Completed";
        }

        public override string GetTypeName()
        {
            return nameof(AllStepActionsCompletedMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return areActionsCompleted != null && areActionsCompleted.Invoke();
        }
    }
}
