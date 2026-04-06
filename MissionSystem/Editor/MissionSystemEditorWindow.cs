using UnityEngine;
using UnityEditor;
using System.IO;

namespace GAME.MissionSystem.Editor
{
    public class MissionSystemEditorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string newMissionName = "NewMission";
        private string basePath = "Assets/GAME/Data/Missions";
        
        [MenuItem("Window/Mission System/Mission Manager Window")]
        public static void ShowWindow()
        {
            MissionSystemEditorWindow window = GetWindow<MissionSystemEditorWindow>("Mission System");
            window.minSize = new Vector2(400, 300);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Mission System Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            DrawCreateMissionSection();
            
            EditorGUILayout.Space(20);
            
            DrawQuickAccessSection();
            
            EditorGUILayout.Space(20);
            
            DrawHelpSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawCreateMissionSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Create New Mission", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            newMissionName = EditorGUILayout.TextField("Mission Name", newMissionName);
            basePath = EditorGUILayout.TextField("Base Path", basePath);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Create Mission with Folder Structure", GUILayout.Height(35)))
            {
                CreateMissionWithFolders();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawQuickAccessSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Quick Access", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Open Missions Folder", GUILayout.Height(30)))
            {
                OpenMissionsFolder();
            }
            
            if (GUILayout.Button("Find Mission Manager in Scene", GUILayout.Height(30)))
            {
                FindMissionManager();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawHelpSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Help & Documentation", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "1. Create a mission using the button above\n" +
                "2. Select the mission asset and click 'Create New Step'\n" +
                "3. Configure steps with actions and conditions\n" +
                "4. Add MissionManager to your scene\n" +
                "5. Test your mission in Play Mode",
                MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateMissionWithFolders()
        {
            if (string.IsNullOrEmpty(newMissionName))
            {
                EditorUtility.DisplayDialog("Error", "Mission name cannot be empty", "OK");
                return;
            }
            
            string missionFolderPath = Path.Combine(basePath, newMissionName);
            
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                string[] folders = basePath.Split('/');
                string currentPath = folders[0];
                
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = Path.Combine(currentPath, folders[i]);
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            if (!AssetDatabase.IsValidFolder(missionFolderPath))
            {
                AssetDatabase.CreateFolder(basePath, newMissionName);
            }
            
            string missionAssetPath = Path.Combine(missionFolderPath, $"{newMissionName}.asset");
            
            if (File.Exists(missionAssetPath))
            {
                EditorUtility.DisplayDialog("Error", "Mission already exists at this path", "OK");
                return;
            }
            
            MissionConfigSO newMission = CreateInstance<MissionConfigSO>();
            newMission.missionId = newMissionName;
            newMission.missionName = newMissionName;
            
            AssetDatabase.CreateAsset(newMission, missionAssetPath);
            AssetDatabase.SaveAssets();
            
            EditorGUIUtility.PingObject(newMission);
            Selection.activeObject = newMission;
            
            Debug.Log($"[MissionSystem] Created mission: {missionAssetPath}");
            
            EditorUtility.DisplayDialog("Success", 
                $"Mission '{newMissionName}' created successfully!\n\nPath: {missionAssetPath}", 
                "OK");
        }
        
        private void OpenMissionsFolder()
        {
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                EditorUtility.DisplayDialog("Error", $"Folder does not exist: {basePath}", "OK");
                return;
            }
            
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(basePath);
            if (folder != null)
            {
                EditorGUIUtility.PingObject(folder);
                Selection.activeObject = folder;
            }
        }
        
        private void FindMissionManager()
        {
            MissionManager manager = FindObjectOfType<MissionManager>();
            
            if (manager != null)
            {
                Selection.activeGameObject = manager.gameObject;
                EditorGUIUtility.PingObject(manager.gameObject);
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found", 
                    "MissionManager not found in scene.\n\nCreate one from: GameObject > GAME > Mission System > Mission Manager", 
                    "OK");
            }
        }
    }
}
