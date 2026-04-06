using UnityEngine;
using UnityEditor;
using GAME.MissionSystem;

namespace GAME.MissionSystem.Editor
{
    [CustomEditor(typeof(MissionMappingConfigSO))]
    public class MissionMappingConfigSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MissionMappingConfigSO config = (MissionMappingConfigSO)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Ce ScriptableObject mappe les IDs de missions (EnumMission) vers les MissionFlowConfigSO.\n" +
                "Assurez-vous que les IDs correspondent exactement aux valeurs de l'enum EnumMission.",
                MessageType.Info
            );
            EditorGUILayout.Space(10);
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Validate All Mappings", GUILayout.Height(30)))
            {
                ValidateMappings(config);
            }
        }
        
        private void ValidateMappings(MissionMappingConfigSO config)
        {
            var mappings = config.GetAllMappings();
            bool hasErrors = false;
            
            foreach (var mapping in mappings)
            {
                if (string.IsNullOrEmpty(mapping.missionId))
                {
                    Debug.LogError("[MissionMappingConfig] Found mapping with empty mission ID");
                    hasErrors = true;
                }
                
                if (mapping.missionFlow == null)
                {
                    Debug.LogError($"[MissionMappingConfig] Mission '{mapping.missionId}' has no MissionFlowConfigSO assigned");
                    hasErrors = true;
                }
            }
            
            if (!hasErrors)
            {
                Debug.Log($"[MissionMappingConfig] All {mappings.Count} mappings are valid!");
            }
        }
    }
}
