using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Interaction")]
    [Serializable]
    public sealed class DisableUserActionMissionActionData : MissionActionData
    {
        public string userActionId;

        public override string GetDisplayName()
        {
            return $"Disable '{userActionId}'";
        }

        public override string GetTypeName()
        {
            return nameof(DisableUserActionMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            Evaveo.nsVorta.HighLevel.UserActionsManager.SetActionCanDo(userActionId, false);
            onComplete?.Invoke();
        }
    }
}
