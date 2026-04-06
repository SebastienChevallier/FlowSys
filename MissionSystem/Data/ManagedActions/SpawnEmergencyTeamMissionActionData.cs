using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class SpawnEmergencyTeamMissionActionData : MissionActionData
    {
        public string npcPrefabId;
        public string spawnPointId;
        public string targetPointId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(npcPrefabId)
                ? $"Spawn Emergency Team '{npcPrefabId}' from '{spawnPointId}' to '{targetPointId}'"
                : "Spawn Emergency Team";
        }

        public override string GetTypeName() => nameof(SpawnEmergencyTeamMissionActionData);
        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (string.IsNullOrEmpty(npcPrefabId))
            {
                Debug.LogError("[MissionSystem] Cannot spawn emergency team: npcPrefabId is null or empty");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[MissionSystem] Spawning emergency team '{npcPrefabId}' from '{spawnPointId}' to '{targetPointId}'");
            Debug.LogWarning("[MissionSystem] SpawnEmergencyTeam not fully implemented - needs NPC system integration");
            onComplete?.Invoke();
        }
    }
}
