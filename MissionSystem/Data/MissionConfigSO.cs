using System.Collections.Generic;
using UnityEngine;
using Evaveo.EvaveoToolbox;

namespace GAME.MissionSystem
{
    public enum EMissionMode
    {
        Formation,
        Evaluation
    }

    [CreateAssetMenu(fileName = "Mission_New", menuName = "GAME/Mission System/Mission Config", order = 1)]
    public class MissionConfigSO : ScriptableObject
    {
        [Header("Mission Information")]
        public string missionId;
        public string missionName;
        public EMissionMode missionMode = EMissionMode.Formation;
        
        [Header("Scenes")]
        [Scene] public string sceneArt;
        [Scene] public string sceneInteraction;
        
        [Header("Player Spawn")]
        public Vector3 playerSpawnPosition;
        public Vector3 playerSpawnRotation;
        
        [Header("Mission Steps")]
        public List<MissionStepConfigSO> steps = new List<MissionStepConfigSO>();
    }
}
