using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Interaction")]
    [Serializable]
    public sealed class EnableUserActionMissionActionData : MissionActionData
    {
        public string userActionId;

        public override string GetDisplayName()
        {
            return $"Enable '{userActionId}'";
        }

        public override string GetTypeName()
        {
            return nameof(EnableUserActionMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            Evaveo.nsVorta.HighLevel.UserActionsManager.SetActionCanDo(userActionId, true);
            onComplete?.Invoke();
        }
    }
}
