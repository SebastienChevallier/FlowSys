using UnityEngine;
using UnityEditor;

namespace GAME.FlowSys.Editor
{
    [System.Serializable]
    public class MissionFlowGraphSettings : ScriptableObject
    {
        private static MissionFlowGraphSettings instance;
        
        public static MissionFlowGraphSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<MissionFlowGraphSettings>("MissionFlowGraphSettings");
                    if (instance == null)
                    {
                        instance = CreateInstance<MissionFlowGraphSettings>();
                        instance.ResetToDefaults();
                        
#if UNITY_EDITOR
                        string resourcesPath = "Assets/GAME/Scripts/MissionSystem/Editor/Resources";
                        if (!AssetDatabase.IsValidFolder(resourcesPath))
                        {
                            string[] folders = resourcesPath.Split('/');
                            string currentPath = folders[0];
                            for (int i = 1; i < folders.Length; i++)
                            {
                                string newPath = currentPath + "/" + folders[i];
                                if (!AssetDatabase.IsValidFolder(newPath))
                                {
                                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                                }
                                currentPath = newPath;
                            }
                        }
                        
                        AssetDatabase.CreateAsset(instance, resourcesPath + "/MissionFlowGraphSettings.asset");
                        AssetDatabase.SaveAssets();
#endif
                    }
                }
                return instance;
            }
        }
        
        [Header("Node")]
        [Tooltip("Couleur de fond des nodes")]
        public Color nodeBackgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        [Tooltip("Couleur de bordure des nodes")]
        public Color nodeBorderColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        [Tooltip("Épaisseur de la bordure des nodes")]
        [Range(1f, 10f)]
        public float nodeBorderWidth = 2f;

        [Tooltip("Largeur minimale des nodes")]
        [Range(150f, 1000f)]
        public float nodeMinWidth = 250f;

        [Header("Text")]
        [Tooltip("Taille du texte du titre")]
        [Range(10f, 24f)]
        public float nodeTitleFontSize = 14f;

        [Tooltip("Couleur du texte du titre")]
        public Color nodeTitleColor = Color.white;

        [Tooltip("Taille du texte du résumé")]
        [Range(8f, 18f)]
        public float nodeSummaryFontSize = 9f;

        [Tooltip("Couleur du texte du résumé")]
        public Color nodeSummaryTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Tooltip("Taille du texte des headers")]
        [Range(8f, 16f)]
        public float headerFontSize = 11f;

        [Tooltip("Couleur du texte des headers")]
        public Color headerTextColor = Color.white;

        [Tooltip("Taille du texte des éléments")]
        [Range(8f, 16f)]
        public float actionFontSize = 10f;

        [Tooltip("Couleur du texte des éléments")]
        public Color itemTextColor = Color.white;

        [Tooltip("Taille du texte des paramètres")]
        [Range(6f, 14f)]
        public float parameterFontSize = 9f;

        [Tooltip("Couleur du texte des paramètres")]
        public Color parameterTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Tooltip("Couleur du texte vide / informatif")]
        public Color emptyStateTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        [Header("Sections")]
        [Tooltip("Couleur de fond des actions OnEnter")]
        public Color actionOnEnterColor = new Color(0.2f, 0.3f, 0.6f, 1f);

        [Tooltip("Couleur de fond des actions OnExit")]
        public Color actionOnExitColor = new Color(0.6f, 0.3f, 0.2f, 1f);

        [Tooltip("Couleur de fond des conditions")]
        public Color conditionColor = new Color(0.6f, 0.2f, 0.2f, 1f);

        [Tooltip("Couleur de fond des reactions")]
        public Color reactionColor = new Color(0.4f, 0.2f, 0.6f, 1f);

        [Tooltip("Couleur du header Actions OnEnter")]
        public Color headerOnEnterColor = new Color(1f, 0.5f, 0f, 1f);

        [Tooltip("Couleur du header Actions OnExit")]
        public Color headerOnExitColor = new Color(1f, 0.5f, 0f, 1f);

        [Tooltip("Couleur du header Conditions")]
        public Color headerConditionsColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Tooltip("Couleur du header Transitions")]
        public Color headerTransitionsColor = new Color(0.55f, 0.25f, 0.75f, 1f);

        [Header("Graph")]
        [Tooltip("Couleur de fond du graphe")]
        public Color graphBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);

        [Tooltip("Couleur de la grille")]
        public Color gridColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Header("Connections")]
        [Tooltip("Couleur des connexions")]
        public Color connectionColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        [Tooltip("Épaisseur des connexions")]
        [Range(1f, 10f)]
        public float connectionWidth = 2f;

        [Header("Buttons")]
        [Tooltip("Couleur du bouton d'ajout (+)")]
        public Color addButtonColor = new Color(0.2f, 0.7f, 0.2f, 1f);

        [Tooltip("Couleur du texte des boutons d'ajout / déplacement")]
        public Color addButtonTextColor = Color.white;

        [Tooltip("Couleur du bouton de suppression (X)")]
        public Color deleteButtonColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        [Tooltip("Couleur du texte des boutons de suppression")]
        public Color deleteButtonTextColor = Color.white;

        [Tooltip("Taille des boutons de gestion")]
        [Range(16f, 32f)]
        public float buttonSize = 20f;
        
        public void ResetToDefaults()
        {
            nodeBackgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            nodeBorderColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            nodeBorderWidth = 2f;
            nodeMinWidth = 250f;
            nodeTitleFontSize = 14f;
            nodeTitleColor = Color.white;
            nodeSummaryFontSize = 9f;
            nodeSummaryTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            headerFontSize = 11f;
            headerTextColor = Color.white;
            actionFontSize = 10f;
            itemTextColor = Color.white;
            parameterFontSize = 9f;
            parameterTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            emptyStateTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

            actionOnEnterColor = new Color(0.2f, 0.3f, 0.6f, 1f);
            actionOnExitColor = new Color(0.6f, 0.3f, 0.2f, 1f);
            conditionColor = new Color(0.6f, 0.2f, 0.2f, 1f);
            reactionColor = new Color(0.4f, 0.2f, 0.6f, 1f);
            headerOnEnterColor = new Color(1f, 0.5f, 0f, 1f);
            headerOnExitColor = new Color(1f, 0.5f, 0f, 1f);
            headerConditionsColor = new Color(1f, 0.3f, 0.3f, 1f);
            headerTransitionsColor = new Color(0.55f, 0.25f, 0.75f, 1f);

            graphBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
            gridColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            connectionColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            connectionWidth = 2f;

            addButtonColor = new Color(0.2f, 0.7f, 0.2f, 1f);
            addButtonTextColor = Color.white;
            deleteButtonColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            deleteButtonTextColor = Color.white;
            buttonSize = 20f;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}
