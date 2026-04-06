using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    internal static class MissionFlowEditorSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Mission Flow Editor", SettingsScope.Project)
            {
                label = "Mission Flow Editor",
                guiHandler = _ =>
                {
                    MissionFlowEditorSettings settingsAsset = MissionFlowEditorSettings.GetOrCreateSettings();
                    SerializedObject serializedObject = new SerializedObject(settingsAsset);
                    serializedObject.Update();

                    EditorGUILayout.LabelField("Theme", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedNodeColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeNodeColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("unreachableNodeColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("textColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundStyle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gridColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("connectionColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedConnectionColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConnectionColor"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeWidth"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeMinHeight"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeLineHeight"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("titleFontSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("contentFontSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("connectionThickness"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConnectionThickness"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Action Editor Display", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showActionPhase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionDensity"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("alternateActionRowColors"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionRowEvenColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionRowOddColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionHeaderColor"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Graph Visuals", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("drawNodeBorder"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeBorderColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedNodeBorderColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showNodeIndices"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showNodeActionPreview"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showNodeConditionPreview"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeInnerSpacing"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Play Mode", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showPlayModeOverlay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animateActiveConnection"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConnectionAnimationSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConnectionDotSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConnectionDotCount"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Mini Map", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("showMiniMap"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("miniMapBackgroundColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("miniMapNodeColor"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("miniMapSelectedNodeColor"));

                    serializedObject.ApplyModifiedProperties();

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("These settings control the Mission Flow Editor theme, action editor density and visibility, graph readability, and play mode visualization.", MessageType.Info);
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[]
                {
                    "Mission", "Flow", "Graph", "Node", "Color", "Theme", "Play Mode", "Action", "Display", "Mini Map"
                })
            };
        }
    }
}
