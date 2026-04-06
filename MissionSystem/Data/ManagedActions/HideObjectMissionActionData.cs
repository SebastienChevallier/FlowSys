using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class HideObjectMissionActionData : MissionActionData
    {
        public string objectId;

        public override string GetDisplayName()
        {
            return $"Hide '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(HideObjectMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject target = context != null ? context.GetObjectById(objectId) : null;
            if (target != null)
            {
                MissionObjectRegistrar registrar = target.GetComponent<MissionObjectRegistrar>();
                if (registrar != null && registrar.hideAfterRegistration)
                    registrar.ApplyRuntimeHiddenState();
                else
                    target.SetActive(false);
            }

            onComplete?.Invoke();
        }
    }
}
