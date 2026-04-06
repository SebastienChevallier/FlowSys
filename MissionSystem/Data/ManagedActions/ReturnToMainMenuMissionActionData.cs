using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class ReturnToMainMenuMissionActionData : MissionActionData
    {
        public override string GetDisplayName()
        {
            return "Return To Main Menu";
        }

        public override string GetTypeName()
        {
            return nameof(ReturnToMainMenuMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            context?.ReturnToMainMenu();
            onComplete?.Invoke();
        }
    }
}
