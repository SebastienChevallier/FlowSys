using UnityEditor;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(SpawnEmergencyTeamMissionActionData))]
    internal sealed class SpawnEmergencyTeamMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not SpawnEmergencyTeamMissionActionData data)
                return false;

            EditorGUI.BeginChangeCheck();
            data.npcPrefabId = EditorGUILayout.TextField("NPC Prefab ID", data.npcPrefabId);
            data.spawnPointId = EditorGUILayout.TextField("Spawn Point ID", data.spawnPointId);
            data.targetPointId = EditorGUILayout.TextField("Target Point ID", data.targetPointId);
            return MissionGraphEditorFieldUtility.ApplyChange(EditorGUI.EndChangeCheck(), context);
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not SpawnEmergencyTeamMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "NPC Prefab ID",
                data.npcPrefabId,
                value =>
                {
                    data.npcPrefabId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Spawn Point ID",
                data.spawnPointId,
                value =>
                {
                    data.spawnPointId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            root.Add(MissionGraphEditorUIComponents.CreateTextField(
                "Target Point ID",
                data.targetPointId,
                value =>
                {
                    data.targetPointId = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
