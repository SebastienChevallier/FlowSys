using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class ShowObjectMissionActionData : MissionActionData
    {
        public string objectId;

        public override string GetDisplayName()
        {
            return $"Show '{objectId}'";
        }

        public override string GetTypeName()
        {
            return nameof(ShowObjectMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject target = context != null ? context.GetObjectById(objectId) : null;
            if (target != null)
            {
                MissionObjectRegistrar registrar = target.GetComponent<MissionObjectRegistrar>();
                if (registrar != null && registrar.hideAfterRegistration)
                    registrar.RestoreRuntimeVisibleState();
                else
                    target.SetActive(true);
            }

            onComplete?.Invoke();
        }
    }
}
