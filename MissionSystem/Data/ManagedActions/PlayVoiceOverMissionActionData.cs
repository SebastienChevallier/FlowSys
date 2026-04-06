using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Audio")]
    [Serializable]
    public sealed class PlayVoiceOverMissionActionData : MissionActionData
    {
        public string voiceOverKey;
        public bool waitForCompletion = true;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(voiceOverKey) ? $"Play VO '{voiceOverKey}'" : "Play Voice Over";
        }

        public override string GetTypeName()
        {
            return nameof(PlayVoiceOverMissionActionData);
        }

        public override bool IsAsync => waitForCompletion;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (context == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (waitForCompletion)
            {
                context.PlayVoiceOverByKey(voiceOverKey, onComplete);
                return;
            }

            context.PlayVoiceOverByKey(voiceOverKey, null);
            onComplete?.Invoke();
        }
    }
}
