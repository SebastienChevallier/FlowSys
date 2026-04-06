using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class TeleportPlayerMissionActionData : MissionActionData
    {
        public string transformReferenceId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(transformReferenceId) ? $"Teleport to '{transformReferenceId}'" : "Teleport Player";
        }

        public override string GetTypeName()
        {
            return nameof(TeleportPlayerMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
            if (targetTransform != null && context != null)
                context.TeleportPlayer(targetTransform.GetPosition(), targetTransform.GetRotation().eulerAngles);

            onComplete?.Invoke();
        }
    }
}
