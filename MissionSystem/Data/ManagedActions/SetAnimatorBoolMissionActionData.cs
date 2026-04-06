using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Animation")]
    [Serializable]
    public sealed class SetAnimatorBoolMissionActionData : MissionActionData
    {
        public string objectId;
        public string animatorParameterName;
        public bool animatorBoolValue;

        public override string GetDisplayName()
        {
            return $"Set Bool '{animatorParameterName}' = {animatorBoolValue} on '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(SetAnimatorBoolMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            MissionAnimatorController animatorController = obj != null ? obj.GetComponent<MissionAnimatorController>() : null;
            if (animatorController != null)
                animatorController.SetBool(animatorParameterName, animatorBoolValue);

            onComplete?.Invoke();
        }
    }
}
