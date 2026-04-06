using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class FadeInMissionActionData : MissionActionData
    {
        public float fadeDuration = 0.5f;

        public override string GetDisplayName()
        {
            return $"Fade In ({fadeDuration}s)";
        }

        public override string GetTypeName()
        {
            return nameof(FadeInMissionActionData);
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

            sceneLoader.FadeFromBlack(fadeDuration, onComplete);
        }
    }
}
