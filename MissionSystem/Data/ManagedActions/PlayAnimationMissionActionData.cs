using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Animation")]
    [Serializable]
    public sealed class PlayAnimationMissionActionData : MissionActionData
    {
        public string objectId;
        public string animationName;

        public override string GetDisplayName()
        {
            return $"Play Anim '{animationName}' on '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(PlayAnimationMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            MissionAnimatorController animatorController = obj != null ? obj.GetComponent<MissionAnimatorController>() : null;
            if (animatorController != null)
                animatorController.PlayAnimation(animationName);

            onComplete?.Invoke();
        }
    }
}
