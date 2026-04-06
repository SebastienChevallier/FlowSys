using UnityEditor;
using UnityEditor.Callbacks;
using GAME.FlowSys;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    public class MissionAssetPostprocessor : AssetPostprocessor
    {
        private static bool hasRun = false;

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (hasRun)
                return;

            hasRun = true;

            EditorApplication.delayCall += () =>
            {
                TryFixPLSPresentation();
            };
        }

        private static void TryFixPLSPresentation()
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

            Debug.Log("[FlowSys] ✓ Automatically fixed PLS Presentation - added StopVoiceOver to OnExit actions");
        }
    }
}
