using UnityEngine;
using UnityEditor;

namespace GAME.MissionSystem.Editor
{
    public class MissionFlowGraphSettingsWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private MissionFlowGraphSettings settings;
        
        [MenuItem("Window/Mission System/Graph Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionFlowGraphSettingsWindow>();
            window.titleContent = new GUIContent("Mission Graph Settings");
            window.minSize = new Vector2(400, 600);
        }
        
        private void OnEnable()
        {
            settings = MissionFlowGraphSettings.Instance;
        }
        
        private void OnGUI()
        {
            if (settings == null)
            {
                settings = MissionFlowGraphSettings.Instance;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Mission Flow Graph Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
            settings.nodeBackgroundColor = EditorGUILayout.ColorField("Background Color", settings.nodeBackgroundColor);
            settings.nodeBorderColor = EditorGUILayout.ColorField("Border Color", settings.nodeBorderColor);
            settings.nodeBorderWidth = EditorGUILayout.Slider("Border Width", settings.nodeBorderWidth, 1f, 10f);
            settings.nodeMinWidth = EditorGUILayout.Slider("Min Width", settings.nodeMinWidth, 150f, 1000f);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
            settings.nodeTitleFontSize = EditorGUILayout.Slider("Title Font Size", settings.nodeTitleFontSize, 10f, 24f);
            settings.nodeTitleColor = EditorGUILayout.ColorField("Title Color", settings.nodeTitleColor);
            settings.nodeSummaryFontSize = EditorGUILayout.Slider("Summary Font Size", settings.nodeSummaryFontSize, 8f, 18f);
            settings.nodeSummaryTextColor = EditorGUILayout.ColorField("Summary Text Color", settings.nodeSummaryTextColor);
            settings.headerFontSize = EditorGUILayout.Slider("Header Font Size", settings.headerFontSize, 8f, 16f);
            settings.headerTextColor = EditorGUILayout.ColorField("Header Text Color", settings.headerTextColor);
            settings.actionFontSize = EditorGUILayout.Slider("Action Font Size", settings.actionFontSize, 8f, 16f);
            settings.itemTextColor = EditorGUILayout.ColorField("Item Text Color", settings.itemTextColor);
            settings.parameterFontSize = EditorGUILayout.Slider("Parameter Font Size", settings.parameterFontSize, 6f, 14f);
            settings.parameterTextColor = EditorGUILayout.ColorField("Parameter Text Color", settings.parameterTextColor);
            settings.emptyStateTextColor = EditorGUILayout.ColorField("Empty State Text Color", settings.emptyStateTextColor);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Sections", EditorStyles.boldLabel);
            settings.actionOnEnterColor = EditorGUILayout.ColorField("OnEnter Body Color", settings.actionOnEnterColor);
            settings.actionOnExitColor = EditorGUILayout.ColorField("OnExit Body Color", settings.actionOnExitColor);
            settings.conditionColor = EditorGUILayout.ColorField("Conditions Body Color", settings.conditionColor);
            settings.reactionColor = EditorGUILayout.ColorField("Transitions Body Color", settings.reactionColor);
            settings.headerOnEnterColor = EditorGUILayout.ColorField("OnEnter Header Color", settings.headerOnEnterColor);
            settings.headerOnExitColor = EditorGUILayout.ColorField("OnExit Header Color", settings.headerOnExitColor);
            settings.headerConditionsColor = EditorGUILayout.ColorField("Conditions Header Color", settings.headerConditionsColor);
            settings.headerTransitionsColor = EditorGUILayout.ColorField("Transitions Header Color", settings.headerTransitionsColor);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Graph", EditorStyles.boldLabel);
            settings.graphBackgroundColor = EditorGUILayout.ColorField("Background Color", settings.graphBackgroundColor);
            settings.gridColor = EditorGUILayout.ColorField("Grid Color", settings.gridColor);
            
            EditorGUILayout.Space(10);
            
            // Connections
            EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);
            settings.connectionColor = EditorGUILayout.ColorField("Connection Color", settings.connectionColor);
            settings.connectionWidth = EditorGUILayout.Slider("Connection Width", settings.connectionWidth, 1f, 10f);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
            settings.addButtonColor = EditorGUILayout.ColorField("Add Button Color", settings.addButtonColor);
            settings.addButtonTextColor = EditorGUILayout.ColorField("Add Button Text Color", settings.addButtonTextColor);
            settings.deleteButtonColor = EditorGUILayout.ColorField("Delete Button Color", settings.deleteButtonColor);
            settings.deleteButtonTextColor = EditorGUILayout.ColorField("Delete Button Text Color", settings.deleteButtonTextColor);
            settings.buttonSize = EditorGUILayout.Slider("Button Size", settings.buttonSize, 16f, 32f);
            
            EditorGUILayout.Space(20);
            
            if (EditorGUI.EndChangeCheck())
            {
                settings.Save();
                RepaintAllGraphWindows();
            }
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to default values?", 
                    "Reset", "Cancel"))
                {
                    settings.ResetToDefaults();
                    RepaintAllGraphWindows();
                }
            }
            
            if (GUILayout.Button("Apply to All Windows", GUILayout.Height(30)))
            {
                RepaintAllGraphWindows();
                EditorUtility.DisplayDialog("Settings Applied", 
                    "Settings have been applied to all open Mission Flow Graph windows.", 
                    "OK");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.EndScrollView();
        }
        
        private void RepaintAllGraphWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<MissionFlowGraphWindow>();
            foreach (var window in windows)
            {
                window.ApplySettings();
                window.Repaint();
            }
        }
    }
}
