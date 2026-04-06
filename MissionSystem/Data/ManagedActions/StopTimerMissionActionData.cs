using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class StopTimerMissionActionData : MissionActionData
    {
        public string timerId = "MainTimer";

        public override string GetDisplayName()
        {
            return $"Stop Timer '{timerId}'";
        }

        public override string GetTypeName()
        {
            return nameof(StopTimerMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionTimerManager timerManager = MissionTimerManager.instance;
            if (timerManager != null)
                timerManager.StopTimer(timerId);

            onComplete?.Invoke();
        }
    }
}
