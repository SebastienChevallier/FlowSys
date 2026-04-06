using System.Collections.Generic;
using UnityEngine;

namespace GAME.FlowSys
{
    [CreateAssetMenu(fileName = "MissionFlow_New", menuName = "GAME/FlowSys/Mission Flow", order = 1)]
    public class MissionFlowConfigSO : ScriptableObject
    {
        [Header("Mission Information")]
        public string missionId;
        public string missionName;
        public EMissionMode missionMode = EMissionMode.Formation;

        [Header("Scenes")]
        public string sceneArt;
        public string sceneInteraction;

        [Header("Player Spawn")]
        public Vector3 playerSpawnPosition;
        public Vector3 playerSpawnRotation;
        
        [Header("Steps")]
        public List<MissionStepConfigSO> steps = new List<MissionStepConfigSO>();
        
        [Header("Start Configuration")]
        public MissionStepConfigSO startStep;
        
        public void OnValidate()
        {
            steps ??= new List<MissionStepConfigSO>();
        }
    }
}
