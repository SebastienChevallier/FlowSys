using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GAME.FlowSys.Editor
{
    public static class MissionDebugStepJumperSetup
    {
        [MenuItem("GAME/FlowSys/Create Debug Step Jumper")]
        public static void CreateDebugStepJumper()
        {
            // Canvas
            GameObject canvasGO = new GameObject("[MissionDebugStepJumper]");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(600, 800);
            canvasRect.localScale = Vector3.one * 0.001f;
            canvasGO.transform.position = new Vector3(0, 1.5f, 1f);

            // Panel background
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = "DEBUG - Step Jump";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // ScrollView
            GameObject scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(panelGO.transform, false);
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            Image scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0);
            RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectTransform.anchorMax = new Vector2(1, 0.9f);
            scrollRectTransform.offsetMin = new Vector2(10, 10);
            scrollRectTransform.offsetMax = new Vector2(-10, 0);

            // Viewport
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            Mask mask = viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            scrollRect.viewport = viewportRect;

            // Content (button container)
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            RectTransform contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Button prefab
            GameObject btnPrefabGO = new GameObject("ButtonPrefab");
            btnPrefabGO.transform.SetParent(canvasGO.transform, false);
            btnPrefabGO.SetActive(false);
            Button btn = btnPrefabGO.AddComponent<Button>();
            Image btnImg = btnPrefabGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            btn.targetGraphic = btnImg;
            RectTransform btnRect = btnPrefabGO.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0, 50);
            LayoutElement le = btnPrefabGO.AddComponent<LayoutElement>();
            le.minHeight = 50;
            le.preferredHeight = 50;

            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnPrefabGO.transform, false);
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 22;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleLeft;
            RectTransform btnTextRect = btnTextGO.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = new Vector2(10, 0);
            btnTextRect.offsetMax = new Vector2(-10, 0);

            // MissionDebugStepJumper component
            MissionDebugStepJumper jumper = canvasGO.AddComponent<MissionDebugStepJumper>();
            SerializedObject so = new SerializedObject(jumper);
            so.FindProperty("panel").objectReferenceValue = panelGO;
            so.FindProperty("buttonContainer").objectReferenceValue = contentGO.transform;
            so.FindProperty("buttonPrefab").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            panelGO.SetActive(false);

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create MissionDebugStepJumper");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[FlowSys] MissionDebugStepJumper created in scene. Assign it to MainScene and save.");
        }
    }
}
