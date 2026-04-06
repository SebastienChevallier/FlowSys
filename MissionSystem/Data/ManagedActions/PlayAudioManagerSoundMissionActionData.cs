using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Audio")]
    [Serializable]
    public sealed class PlayAudioManagerSoundMissionActionData : MissionActionData
    {
        public string audioManagerSoundId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(audioManagerSoundId) ? $"Play Sound '{audioManagerSoundId}'" : "Play AudioManager Sound";
        }

        public override string GetTypeName()
        {
            return nameof(PlayAudioManagerSoundMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (!string.IsNullOrEmpty(audioManagerSoundId))
                AudioManager.PlaySFX(audioManagerSoundId);

            onComplete?.Invoke();
        }
    }
}
