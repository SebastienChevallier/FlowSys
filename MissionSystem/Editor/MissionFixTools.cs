using UnityEditor;
using GAME.MissionSystem;
using UnityEngine;

namespace GAME.MissionSystem.Editor
{
    public class MissionFixTools
    {
        [MenuItem("Window/Mission System/Fix Tools/Add StopVoiceOver to PLS Presentation OnExit")]
        public static void AddStopVoiceOverToPLSPresentation()
        {
            const string assetPath = "Assets/GAME/Data/Missions/PLS/Steps/Step_Presentation.asset";
            MissionStepConfigSO stepConfig = AssetDatabase.LoadAssetAtPath<MissionStepConfigSO>(assetPath);

            if (stepConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not load Step_Presentation asset", "OK");
                return;
            }

            // Create a new StopVoiceOverMissionActionData
            StopVoiceOverMissionActionData stopAction = new StopVoiceOverMissionActionData();

            // Add it to the actions list with OnExit phase
            MissionStepActionEntry entry = new MissionStepActionEntry
            {
                phase = MissionStepActionPhase.OnExit,
                managedAction = stopAction
            };

            stepConfig.AddStructuredAction(entry);

            // Mark as dirty and save
            EditorUtility.SetDirty(stepConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Added StopVoiceOver to Step_Presentation OnExit actions", "OK");
        }

        [MenuItem("Window/Mission System/Fix Tools/Show Equipement Actions Order")]
        public static void ShowEquipementActionsOrder()
        {
            const string assetPath = "Assets/GAME/Data/Missions/PLS/Steps/Step_Equipement.asset";
            MissionStepConfigSO stepConfig = AssetDatabase.LoadAssetAtPath<MissionStepConfigSO>(assetPath);

            if (stepConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not load Step_Equipement asset", "OK");
                return;
            }

            var allActions = stepConfig.GetStructuredActionEntries();
            string message = "Step_Equipement Actions Order:\n\n";

            for (int i = 0; i < allActions.Count; i++)
            {
                var action = allActions[i];
                string phaseName = action.phase == MissionStepActionPhase.OnEnter ? "OnEnter" : "OnExit";
                message += $"{i}: [{phaseName}] {action.managedAction.GetDisplayName()}\n";
            }

            Debug.Log(message);
            EditorUtility.DisplayDialog("Actions Order", message, "OK");
        }
    }
}
