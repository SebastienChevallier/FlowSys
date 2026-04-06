using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Animation")]
    [Serializable]
    public sealed class TriggerAnimatorMissionActionData : MissionActionData
    {
        public string objectId;
        public string animatorParameterName;

        public override string GetDisplayName()
        {
            return $"Trigger '{animatorParameterName}' on '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(TriggerAnimatorMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj != null)
            {
                Component triggerRelay = obj.GetComponent("MissionAnimatorTriggerRelay");
                if (triggerRelay != null)
                {
                    triggerRelay.SendMessage("Trigger", animatorParameterName, SendMessageOptions.DontRequireReceiver);
                    onComplete?.Invoke();
                    return;
                }

                Animator animator = obj.GetComponent<Animator>();
                if (animator == null)
                    animator = obj.GetComponentInChildren<Animator>(true);

                if (animator != null)
                    animator.SetTrigger(animatorParameterName);
            }

            onComplete?.Invoke();
        }
    }
}
