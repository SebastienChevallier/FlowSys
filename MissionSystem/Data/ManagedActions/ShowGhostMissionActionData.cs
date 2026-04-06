using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class ShowGhostMissionActionData : MissionActionData
    {
        public string ghostObjectId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(ghostObjectId) ? $"Show Ghost '{ghostObjectId}'" : "Show Ghost";
        }

        public override string GetTypeName()
        {
            return nameof(ShowGhostMissionActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject ghostObj = context != null ? context.GetObjectById(ghostObjectId) : null;
            if (ghostObj != null)
            {
                var ghostTrace = ghostObj.GetComponent<Evaveo.nsVorta.RUNTIME.GhostTraceBase>();
                if (ghostTrace != null)
                    ghostTrace.Activate();
                else
                    ghostObj.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[MissionSystem] Cannot show ghost: '{ghostObjectId}' not found");
            }

            onComplete?.Invoke();
        }
    }
}
