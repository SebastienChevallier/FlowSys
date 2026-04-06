using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Audio")]
    [Serializable]
    public sealed class StopVoiceOverMissionActionData : MissionActionData
    {
        public override string GetDisplayName()
        {
            return "Stop Voice Over";
        }

        public override string GetTypeName()
        {
            return nameof(StopVoiceOverMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (context == null)
            {
                onComplete?.Invoke();
                return;
            }

            context.StopVoiceOver();
            onComplete?.Invoke();
        }
    }
}
