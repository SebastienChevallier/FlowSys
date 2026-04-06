using System.Collections.Generic;
using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// ScriptableObject qui mappe les IDs de missions (string) vers les MissionFlowConfigSO
    /// </summary>
    [CreateAssetMenu(fileName = "MissionMappingConfig", menuName = "GAME/FlowSys/Mission Mapping Config")]
    public class MissionMappingConfigSO : ScriptableObject
    {
        [System.Serializable]
        public class MissionMapping
        {
            [Tooltip("ID de la mission (doit correspondre à l'enum EnumMission)")]
            public string missionId;
            
            [Tooltip("Configuration de la mission")]
            public MissionFlowConfigSO missionFlow;
        }
        
        [Header("Mission Mappings")]
        [SerializeField] private List<MissionMapping> mappings = new List<MissionMapping>();
        
        /// <summary>
        /// Récupère la configuration d'une mission par son ID
        /// </summary>
        public MissionFlowConfigSO GetMissionFlow(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
            {
                Debug.LogError("[MissionMappingConfig] Mission ID is null or empty");
                return null;
            }
            
            foreach (var mapping in mappings)
            {
                if (mapping.missionId == missionId)
                {
                    if (mapping.missionFlow == null)
                    {
                        Debug.LogError($"[MissionMappingConfig] MissionFlowConfigSO is null for mission ID: {missionId}");
                        return null;
                    }
                    
                    return mapping.missionFlow;
                }
            }
            
            Debug.LogError($"[MissionMappingConfig] No mapping found for mission ID: {missionId}");
            return null;
        }
        
        /// <summary>
        /// Ajoute ou met à jour un mapping
        /// </summary>
        public void AddOrUpdateMapping(string missionId, MissionFlowConfigSO flow)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.missionId == missionId)
                {
                    mapping.missionFlow = flow;
                    return;
                }
            }
            
            mappings.Add(new MissionMapping
            {
                missionId = missionId,
                missionFlow = flow
            });
        }
        
        /// <summary>
        /// Supprime un mapping
        /// </summary>
        public void RemoveMapping(string missionId)
        {
            mappings.RemoveAll(m => m.missionId == missionId);
        }
        
        /// <summary>
        /// Récupère tous les mappings
        /// </summary>
        public List<MissionMapping> GetAllMappings()
        {
            return new List<MissionMapping>(mappings);
        }
    }
}
