using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class StartTimerMissionActionData : MissionActionData
    {
        public string timerId = "MainTimer";
        public float duration = 60f;

        public override string GetDisplayName()
        {
            return $"Start Timer '{timerId}' ({duration}s)";
        }

        public override string GetTypeName()
        {
            return nameof(StartTimerMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionTimerManager timerManager = MissionTimerManager.instance;
            if (timerManager != null)
                timerManager.StartTimer(timerId, duration);

            onComplete?.Invoke();
        }
    }
}
