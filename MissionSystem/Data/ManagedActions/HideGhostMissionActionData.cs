using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class HideGhostMissionActionData : MissionActionData
    {
        public string ghostObjectId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(ghostObjectId) ? $"Hide Ghost '{ghostObjectId}'" : "Hide Ghost";
        }

        public override string GetTypeName()
        {
            return nameof(HideGhostMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject ghostObj = context != null ? context.GetObjectById(ghostObjectId) : null;
            if (ghostObj != null)
            {
                var ghostTrace = ghostObj.GetComponent<Evaveo.nsVorta.RUNTIME.GhostTraceBase>();
                if (ghostTrace != null)
                    ghostTrace.DeActivate();
                else
                    ghostObj.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[MissionSystem] Cannot hide ghost: '{ghostObjectId}' not found");
            }

            onComplete?.Invoke();
        }
    }
}
