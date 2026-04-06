using System.IO;
using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    public class MissionFlowMigrationTool : EditorWindow
    {
        private MissionConfigSO sourceMission;
        private bool autoLayoutNodes = true;
        private float nodeSpacingX = 300f;
        private float nodeSpacingY = 150f;
        
        [MenuItem("Window/Mission System/Migration Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionFlowMigrationTool>();
            window.titleContent = new GUIContent("Mission Flow Migration");
            window.minSize = new Vector2(400, 300);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Migration: MissionConfigSO → MissionFlowConfigSO", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "Cet outil convertit vos missions existantes (MissionConfigSO) vers le nouveau format GraphView (MissionFlowConfigSO).\n\n" +
                "Les données existantes ne seront PAS modifiées. Un nouveau fichier MissionFlowConfigSO sera créé.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            sourceMission = (MissionConfigSO)EditorGUILayout.ObjectField(
                "Mission Source",
                sourceMission,
                typeof(MissionConfigSO),
                false
            );
            
            EditorGUILayout.Space(10);
            
            autoLayoutNodes = EditorGUILayout.Toggle("Auto Layout Nodes", autoLayoutNodes);
            
            if (autoLayoutNodes)
            {
                EditorGUI.indentLevel++;
                nodeSpacingX = EditorGUILayout.FloatField("Spacing X", nodeSpacingX);
                nodeSpacingY = EditorGUILayout.FloatField("Spacing Y", nodeSpacingY);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            GUI.enabled = sourceMission != null;
            if (GUILayout.Button("Migrer vers MissionFlowConfigSO", GUILayout.Height(40)))
            {
                MigrateMission();
            }
            GUI.enabled = true;
            
            EditorGUILayout.Space(10);
            
            if (sourceMission != null && sourceMission.steps != null)
            {
                EditorGUILayout.LabelField($"Steps à migrer: {sourceMission.steps.Count}", EditorStyles.helpBox);
            }
        }
        
        private void MigrateMission()
        {
            if (sourceMission == null)
            {
                EditorUtility.DisplayDialog("Erreur", "Aucune mission source sélectionnée", "OK");
                return;
            }
            
            string sourcePath = AssetDatabase.GetAssetPath(sourceMission);
            string sourceFolder = Path.GetDirectoryName(sourcePath);
            string flowName = sourceMission.name + "_Flow";
            string flowPath = Path.Combine(sourceFolder, flowName + ".asset");
            
            flowPath = AssetDatabase.GenerateUniqueAssetPath(flowPath);
            
            var flow = CreateInstance<MissionFlowConfigSO>();
            flow.missionId = sourceMission.missionId;
            flow.missionName = sourceMission.missionName;
            flow.missionMode = sourceMission.missionMode;
            flow.sceneArt = sourceMission.sceneArt;
            flow.sceneInteraction = sourceMission.sceneInteraction;
            flow.playerSpawnPosition = sourceMission.playerSpawnPosition;
            flow.playerSpawnRotation = sourceMission.playerSpawnRotation;
            flow.steps = new System.Collections.Generic.List<MissionStepConfigSO>(sourceMission.steps);
            
            if (sourceMission.steps != null && sourceMission.steps.Count > 0)
            {
                flow.startStep = sourceMission.steps[0];
            }
            
            if (autoLayoutNodes)
            {
                AutoLayoutSteps(flow);
            }
            
            AssetDatabase.CreateAsset(flow, flowPath);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog(
                "Migration Réussie",
                $"MissionFlowConfigSO créé:\n{flowPath}\n\n" +
                $"Steps migrés: {flow.steps.Count}\n\n" +
                "Ouvrez maintenant 'GAME > Mission System > Mission Flow Graph Editor (New)' pour visualiser le flow.",
                "OK"
            );
            
            EditorGUIUtility.PingObject(flow);
            Selection.activeObject = flow;
        }
        
        private void AutoLayoutSteps(MissionFlowConfigSO flow)
        {
            if (flow.steps == null || flow.steps.Count == 0)
                return;
            
            float currentX = 100f;
            float currentY = 100f;
            int stepsPerRow = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(flow.steps.Count)));
            
            for (int i = 0; i < flow.steps.Count; i++)
            {
                var step = flow.steps[i];
                if (step == null)
                    continue;
                
                step.editorPosition = new Vector2(currentX, currentY);
                EditorUtility.SetDirty(step);
                
                currentX += nodeSpacingX;
                
                if ((i + 1) % stepsPerRow == 0)
                {
                    currentX = 100f;
                    currentY += nodeSpacingY;
                }
            }
            
            AssetDatabase.SaveAssets();
        }
    }
}
