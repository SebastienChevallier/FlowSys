using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    public static class MissionSceneDiagnostic
    {
        [MenuItem("GAME/FlowSys/List All Root GameObjects")]
        public static void ListAllRootObjects()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                Debug.Log($"[Diagnostic] Scene '{scene.name}' root objects:");
                foreach (var go in scene.GetRootGameObjects())
                    Debug.Log($"  - '{go.name}' active={go.activeSelf}");
            }
        }

        [MenuItem("GAME/FlowSys/Diagnose Scene Setup")]
        public static void DiagnoseScene()
        {
            // MissionTextUIManager
            MissionTextUIManager uiMgr = Object.FindObjectOfType<MissionTextUIManager>(true);
            if (uiMgr != null)
            {
                var so = new UnityEditor.SerializedObject(uiMgr);
                var targetCanvasProp = so.FindProperty("targetCanvas");
                var dialogProp = so.FindProperty("multipleChoiceDialog");

                string canvasName = targetCanvasProp?.objectReferenceValue != null
                    ? ((UnityEngine.Canvas)targetCanvasProp.objectReferenceValue).gameObject.name
                    : "NULL";
                string dialogName = dialogProp?.objectReferenceValue != null
                    ? ((UnityEngine.GameObject)((MultipleChoiceDialog)dialogProp.objectReferenceValue).gameObject).name
                    : "NULL";

                Debug.Log($"[Diagnostic] MissionTextUIManager on '{uiMgr.gameObject.name}' | targetCanvas={canvasName} | multipleChoiceDialog={dialogName}");

                // Also check Canvas child hierarchy
                Canvas canvas = uiMgr.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                {
                    Debug.Log($"[Diagnostic] Canvas found in children: '{canvas.gameObject.name}' active={canvas.gameObject.activeSelf}");
                    var dlgTransform = canvas.transform.Find("PLS_MultipleChoiceDialog");
                    Debug.Log(dlgTransform != null
                        ? $"[Diagnostic] PLS_MultipleChoiceDialog child FOUND under canvas, active={dlgTransform.gameObject.activeSelf}"
                        : "[Diagnostic] PLS_MultipleChoiceDialog NOT found as direct child of Canvas");
                }
                else
                    Debug.LogWarning("[Diagnostic] No Canvas found in children of UIManager");
            }
            else
                Debug.LogWarning("[Diagnostic] MissionTextUIManager NOT FOUND in scene");
        }
    }
}
