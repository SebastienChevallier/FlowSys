using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Animation")]
    [Serializable]
    public sealed class SetAnimatorFloatMissionActionData : MissionActionData
    {
        public string objectId;
        public string animatorParameterName;
        public float animatorFloatValue;

        public override string GetDisplayName()
        {
            return $"Set Float '{animatorParameterName}' = {animatorFloatValue} on '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(SetAnimatorFloatMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            MissionAnimatorController animatorController = obj != null ? obj.GetComponent<MissionAnimatorController>() : null;
            if (animatorController != null)
                animatorController.SetFloat(animatorParameterName, animatorFloatValue);

            onComplete?.Invoke();
        }
    }
}
