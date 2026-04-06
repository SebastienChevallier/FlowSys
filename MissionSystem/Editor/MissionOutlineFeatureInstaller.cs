using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GAME.MissionSystem.Editor
{
    [InitializeOnLoad]
    internal static class MissionOutlineFeatureInstaller
    {
        static MissionOutlineFeatureInstaller()
        {
            EditorApplication.delayCall += EnsureInstalled;
        }

        private static void EnsureInstalled()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Type featureType = Type.GetType("URPOutlineFeature, Assembly-CSharp-firstpass") ??
                               Type.GetType("URPOutlineFeature, Assembly-CSharp");
            if (featureType == null)
                return;

            string[] guids = AssetDatabase.FindAssets("t:UniversalRendererData", new[] { "Assets/GAME/Settings/Render" });
            bool anyDirty = false;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                if (rendererData == null)
                    continue;

                bool alreadyInstalled = rendererData.rendererFeatures.Any(f => f != null && f.GetType() == featureType);
                if (alreadyInstalled)
                    continue;

                ScriptableRendererFeature feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(featureType);
                feature.name = "URPOutlineFeature";
                AssetDatabase.AddObjectToAsset(feature, rendererData);
                rendererData.rendererFeatures.Add(feature);
                EditorUtility.SetDirty(feature);
                EditorUtility.SetDirty(rendererData);
                anyDirty = true;

                Debug.Log($"[MissionOutline] URPOutlineFeature ajoutée à {path}");
            }

            if (anyDirty)
                AssetDatabase.SaveAssets();
        }
    }
}
