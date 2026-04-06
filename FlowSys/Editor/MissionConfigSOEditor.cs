using UnityEngine;
using UnityEditor;
using System.IO;

namespace GAME.FlowSys.Editor
{
    [CustomEditor(typeof(MissionConfigSO))]
    public class MissionConfigSOEditor : UnityEditor.Editor
    {
        private MissionConfigSO missionConfig;
        
        private void OnEnable()
        {
            missionConfig = (MissionConfigSO)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Mission Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create New Step", GUILayout.Height(30)))
            {
                CreateNewStep();
            }
            
            if (GUILayout.Button("Test Mission", GUILayout.Height(30)))
            {
                TestMission();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Open Mission Folder", GUILayout.Height(25)))
            {
                OpenMissionFolder();
            }
            
            EditorGUILayout.Space(10);
            
            if (missionConfig.steps != null && missionConfig.steps.Count > 0)
            {
                EditorGUILayout.LabelField($"Steps: {missionConfig.steps.Count}", EditorStyles.helpBox);
            }
            else
            {
                EditorGUILayout.HelpBox("No steps defined. Click 'Create New Step' to add one.", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateNewStep()
        {
            string assetPath = AssetDatabase.GetAssetPath(missionConfig);
            string folderPath = Path.GetDirectoryName(assetPath);
            
            string stepName = $"Step_{missionConfig.steps.Count + 1:00}";
            string stepPath = Path.Combine(folderPath, $"{stepName}.asset");
            
            stepPath = AssetDatabase.GenerateUniqueAssetPath(stepPath);
            
            MissionStepConfigSO newStep = CreateInstance<MissionStepConfigSO>();
            newStep.stepId = stepName;
            newStep.stepName = $"Step {missionConfig.steps.Count + 1}";
            
            AssetDatabase.CreateAsset(newStep, stepPath);
            AssetDatabase.SaveAssets();
            
            missionConfig.steps.Add(newStep);
            EditorUtility.SetDirty(missionConfig);
            AssetDatabase.SaveAssets();
            
            EditorGUIUtility.PingObject(newStep);
            Selection.activeObject = newStep;
            
            Debug.Log($"[FlowSys] Created new step: {stepPath}");
        }
        
        private void TestMission()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[FlowSys] MissionConfigSO is no longer the primary runtime mission asset. Launch the mission from MissionFlowConfigSO via MissionMappingConfigSO/GameFlowManager.");
            }
            else
            {
                Debug.LogWarning("[FlowSys] Enter Play Mode to test the mission");
            }
        }
        
        private void OpenMissionFolder()
        {
            string assetPath = AssetDatabase.GetAssetPath(missionConfig);
            string folderPath = Path.GetDirectoryName(assetPath);
            
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            if (folder != null)
            {
                EditorGUIUtility.PingObject(folder);
                Selection.activeObject = folder;
            }
        }
    }
}
