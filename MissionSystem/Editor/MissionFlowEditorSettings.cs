using System.IO;
using UnityEditor;
using UnityEngine;

namespace GAME.MissionSystem.Editor
{
    public enum MissionFlowBackgroundStyle
    {
        Lines,
        Dots
    }

    public enum MissionFlowActionDensity
    {
        Compact,
        Normal,
        Comfortable
    }

    public class MissionFlowEditorSettings : ScriptableObject
    {
        public const string AssetPath = "Assets/GAME/Settings/Editor/MissionFlowEditorSettings.asset";

        [Header("Node Layout")]
        public float nodeWidth = 240f;
        public float nodeMinHeight = 110f;
        public float nodeLineHeight = 16f;
        public int titleFontSize = 12;
        public int contentFontSize = 11;

        [Header("Node Colors")]
        public Color nodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public Color selectedNodeColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        public Color activeNodeColor = new Color(0.2f, 0.65f, 0.3f, 1f);
        public Color unreachableNodeColor = new Color(0.45f, 0.22f, 0.22f, 1f);
        public Color textColor = Color.white;

        [Header("Connections")]
        public MissionFlowBackgroundStyle backgroundStyle = MissionFlowBackgroundStyle.Lines;
        public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        public Color connectionColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        public Color selectedConnectionColor = new Color(0.3f, 0.7f, 1f, 1f);
        public Color activeConnectionColor = new Color(1f, 0.75f, 0.2f, 1f);
        public float connectionThickness = 2f;
        public float activeConnectionThickness = 4f;

        [Header("Play Mode Visuals")]
        public bool showPlayModeOverlay = true;
        public bool animateActiveConnection = true;
        public float activeConnectionAnimationSpeed = 1.5f;
        public float activeConnectionDotSize = 5f;
        public int activeConnectionDotCount = 4;

        [Header("Action Editor Display")]
        public bool showActionPhase = false;
        public bool showControlTrainerVoiceOver = false;
        public MissionFlowActionDensity actionDensity = MissionFlowActionDensity.Normal;
        public bool alternateActionRowColors = true;
        public Color actionRowEvenColor = new Color(1f, 1f, 1f, 0.02f);
        public Color actionRowOddColor = new Color(1f, 1f, 1f, 0.06f);
        public Color actionHeaderColor = new Color(1f, 1f, 1f, 0.04f);

        [Header("Graph Visuals")]
        public bool drawNodeBorder = true;
        public Color nodeBorderColor = new Color(1f, 1f, 1f, 0.08f);
        public Color selectedNodeBorderColor = new Color(0.35f, 0.75f, 1f, 0.9f);
        public bool showNodeIndices = true;
        public bool showNodeActionPreview = true;
        public bool showNodeConditionPreview = true;
        public float nodeInnerSpacing = 3f;

        [Header("Mini Map")]
        public bool showMiniMap = true;
        public Color miniMapBackgroundColor = new Color(0f, 0f, 0f, 0.28f);
        public Color miniMapNodeColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        public Color miniMapSelectedNodeColor = new Color(0.3f, 0.7f, 1f, 1f);

        public static MissionFlowEditorSettings GetOrCreateSettings()
        {
            MissionFlowEditorSettings settings = AssetDatabase.LoadAssetAtPath<MissionFlowEditorSettings>(AssetPath);
            if (settings != null)
                return settings;

            string directory = Path.GetDirectoryName(AssetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            settings = CreateInstance<MissionFlowEditorSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
