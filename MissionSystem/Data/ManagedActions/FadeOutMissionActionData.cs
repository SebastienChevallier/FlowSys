using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class FadeOutMissionActionData : MissionActionData
    {
        public float fadeDuration = 0.5f;

        public override string GetDisplayName()
        {
            return $"Fade Out ({fadeDuration}s)";
        }

        public override string GetTypeName()
        {
            return nameof(FadeOutMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionSceneLoader sceneLoader = MissionRuntimeResolver.FindSceneLoader();
            if (sceneLoader == null)
            {
                onComplete?.Invoke();
                return;
            }

            sceneLoader.FadeToBlack(fadeDuration, onComplete);
        }
    }
}
