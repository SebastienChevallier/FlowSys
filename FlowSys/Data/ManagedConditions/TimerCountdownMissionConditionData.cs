using System;

namespace GAME.FlowSys
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class TimerCountdownMissionConditionData : MissionConditionData
    {
        public string timerId = "MainTimer";
        public bool requireAllActionsCompleted = true;
        public MissionStepConfigSO targetStepOnTimeout;
        [NonSerialized]
        public Func<string, bool, bool> timerEvaluator;

        public override string GetDisplayName()
        {
            string actionCheck = requireAllActionsCompleted ? " + All Actions" : "";
            string timeoutTarget = targetStepOnTimeout != null ? $" / Timeout→{targetStepOnTimeout.stepName}" : "";
            return $"Timer '{timerId}' OK{actionCheck}{timeoutTarget}";
        }

        public override string GetTypeName()
        {
            return nameof(TimerCountdownMissionConditionData);
        }

        public override bool Evaluate(IMissionContext context)
        {
            return timerEvaluator != null && timerEvaluator.Invoke(timerId, requireAllActionsCompleted);
        }
    }
}
