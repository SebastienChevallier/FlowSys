using UnityEditor;
using UnityEditor.SceneManagement;
using GAME.FlowSys;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    [InitializeOnLoad]
    public class MissionInitializer
    {
        static MissionInitializer()
        {
            // This runs once when the editor loads
            EditorApplication.delayCall += OnEditorLoad;
        }

        private static void OnEditorLoad()
        {
            // Only run once
            EditorApplication.delayCall -= OnEditorLoad;

            // Check if we need to fix the PLS Presentation step
            FixPLSPresentationIfNeeded();
        }

        private static void FixPLSPresentationIfNeeded()
        {
            const string assetPath = "Assets/GAME/Data/Missions/PLS/Steps/Step_Presentation.asset";
            MissionStepConfigSO stepConfig = AssetDatabase.LoadAssetAtPath<MissionStepConfigSO>(assetPath);

            if (stepConfig == null)
                return;

            // Check if StopVoiceOverMissionActionData already exists in OnExit
            var exitActions = stepConfig.GetStructuredActionEntries(MissionStepActionPhase.OnExit);
            bool hasStopVoiceOver = false;

            foreach (var action in exitActions)
            {
                if (action.managedAction is StopVoiceOverMissionActionData)
                {
                    hasStopVoiceOver = true;
                    break;
                }
            }

            if (hasStopVoiceOver)
                return; // Already fixed

            // Add StopVoiceOverMissionActionData to OnExit
            StopVoiceOverMissionActionData stopAction = new StopVoiceOverMissionActionData();
            MissionStepActionEntry entry = new MissionStepActionEntry
            {
                phase = MissionStepActionPhase.OnExit,
                managedAction = stopAction
            };

            stepConfig.AddStructuredAction(entry);
            EditorUtility.SetDirty(stepConfig);
            AssetDatabase.SaveAssets();

            Debug.Log("[FlowSys] Fixed PLS Presentation step - added StopVoiceOver to OnExit actions");
        }
    }
}
