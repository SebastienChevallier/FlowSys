using UnityEditor;
using GAME.FlowSys;

namespace GAME.FlowSys.Editor
{
    public class FixPLSPresentationAction
    {
        [MenuItem("Window/Mission System/Fix Tools/[RUN] Fix PLS Presentation - Add Stop VoiceOver")]
        public static void FixPLSPresentation()
        {
            const string assetPath = "Assets/GAME/Data/Missions/PLS/Steps/Step_Presentation.asset";
            MissionStepConfigSO stepConfig = AssetDatabase.LoadAssetAtPath<MissionStepConfigSO>(assetPath);

            if (stepConfig == null)
            {
                UnityEngine.Debug.LogError($"[FlowSys] Could not load Step_Presentation asset at {assetPath}");
                return;
            }

            UnityEngine.Debug.Log($"[FlowSys] Loaded Step_Presentation successfully");

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
            {
                UnityEngine.Debug.Log("[FlowSys] StopVoiceOver already exists in OnExit actions. No changes needed.");
                return;
            }

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
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("[FlowSys] ✓ Successfully added StopVoiceOver action to Step_Presentation OnExit");
        }
    }
}
