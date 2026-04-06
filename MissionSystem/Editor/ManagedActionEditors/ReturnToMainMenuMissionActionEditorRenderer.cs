using UnityEditor;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ReturnToMainMenuMissionActionData))]
    internal sealed class ReturnToMainMenuMissionActionEditorRenderer : IMissionActionEditorRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ReturnToMainMenuMissionActionData)
                return false;

            EditorGUILayout.HelpBox("No editable parameters.", MessageType.Info);
            return false;
        }
    }
}
