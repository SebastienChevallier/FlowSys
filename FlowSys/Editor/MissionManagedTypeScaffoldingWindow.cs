using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GAME.FlowSys.Editor
{
    internal sealed class MissionManagedTypeScaffoldingWindow : EditorWindow
    {
        private enum GeneratedKind
        {
            Action,
            Condition
        }

        private string typeName = "NewMissionAction";
        private GeneratedKind kind = GeneratedKind.Action;

        [MenuItem("Window/Mission System/Create Managed Type Pair", false, 126)]
        private static void ShowWindow()
        {
            MissionManagedTypeScaffoldingWindow window = GetWindow<MissionManagedTypeScaffoldingWindow>("Managed Type Pair");
            window.minSize = new Vector2(420f, 150f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create Managed Type Pair", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            typeName = EditorGUILayout.TextField("Type Name", typeName);
            kind = (GeneratedKind)EditorGUILayout.EnumPopup("Kind", kind);

            EditorGUILayout.Space(12);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(typeName)))
            {
                if (GUILayout.Button("Generate Runtime + Editor Files"))
                    Generate();
            }
        }

        private void Generate()
        {
            string sanitized = SanitizeTypeName(typeName);
            if (string.IsNullOrEmpty(sanitized))
            {
                EditorUtility.DisplayDialog("Managed Type Pair", "Type name is invalid.", "OK");
                return;
            }

            string runtimeFolder = kind == GeneratedKind.Action
                ? "Assets/GAME/Scripts/MissionSystem/Data/ManagedActions"
                : "Assets/GAME/Scripts/MissionSystem/Data/ManagedConditions";

            string editorFolder = kind == GeneratedKind.Action
                ? "Assets/GAME/Scripts/MissionSystem/Editor/ManagedActionEditors"
                : "Assets/GAME/Scripts/MissionSystem/Editor/ManagedConditionEditors";

            string runtimeClassName = kind == GeneratedKind.Action
                ? $"{sanitized}MissionActionData"
                : $"{sanitized}MissionConditionData";

            string editorClassName = kind == GeneratedKind.Action
                ? $"{sanitized}MissionActionEditorRenderer"
                : $"{sanitized}MissionConditionEditorRenderer";

            string runtimePath = Path.Combine(Application.dataPath, runtimeFolder.Replace("Assets/", string.Empty), runtimeClassName + ".cs");
            string editorPath = Path.Combine(Application.dataPath, editorFolder.Replace("Assets/", string.Empty), editorClassName + ".cs");

            Directory.CreateDirectory(Path.GetDirectoryName(runtimePath));
            Directory.CreateDirectory(Path.GetDirectoryName(editorPath));

            if (File.Exists(runtimePath) || File.Exists(editorPath))
            {
                EditorUtility.DisplayDialog("Managed Type Pair", "At least one target file already exists.", "OK");
                return;
            }

            File.WriteAllText(runtimePath, kind == GeneratedKind.Action
                ? BuildActionRuntimeTemplate(runtimeClassName)
                : BuildConditionRuntimeTemplate(runtimeClassName));

            File.WriteAllText(editorPath, kind == GeneratedKind.Action
                ? BuildActionEditorTemplate(runtimeClassName, editorClassName)
                : BuildConditionEditorTemplate(runtimeClassName, editorClassName));

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Managed Type Pair", "Files created successfully.", "OK");
        }

        private static string SanitizeTypeName(string value)
        {
            string trimmed = value?.Trim() ?? string.Empty;
            trimmed = trimmed.Replace("MissionActionData", string.Empty).Replace("MissionConditionData", string.Empty).Trim();
            return trimmed.Replace(" ", string.Empty);
        }

        private static string BuildActionRuntimeTemplate(string className)
        {
            return "using System;\r\n\r\nnamespace GAME.FlowSys\r\n{\r\n    [Serializable]\r\n    public sealed class " + className + " : MissionActionData\r\n    {\r\n        public override string GetDisplayName()\r\n        {\r\n            return \"" + className.Replace("MissionActionData", string.Empty) + "\";\r\n        }\r\n\r\n        public override string GetTypeName()\r\n        {\r\n            return nameof(" + className + ");\r\n        }\r\n\r\n        public override bool IsAsync => false;\r\n\r\n        public override void Execute(IMissionContext context, Action onComplete)\r\n        {\r\n            onComplete?.Invoke();\r\n        }\r\n    }\r\n}\r\n";
        }

        private static string BuildConditionRuntimeTemplate(string className)
        {
            return "using System;\r\n\r\nnamespace GAME.FlowSys\r\n{\r\n    [Serializable]\r\n    public sealed class " + className + " : MissionConditionData\r\n    {\r\n        public override string GetDisplayName()\r\n        {\r\n            return \"" + className.Replace("MissionConditionData", string.Empty) + "\";\r\n        }\r\n\r\n        public override string GetTypeName()\r\n        {\r\n            return nameof(" + className + ");\r\n        }\r\n\r\n        public override bool Evaluate(IMissionContext context)\r\n        {\r\n            return true;\r\n        }\r\n    }\r\n}\r\n";
        }

        private static string BuildActionEditorTemplate(string runtimeClassName, string editorClassName)
        {
            return "using UnityEditor;\r\n\r\nnamespace GAME.FlowSys.Editor\r\n{\r\n    [MissionActionEditorRenderer(typeof(" + runtimeClassName + "))]\r\n    internal sealed class " + editorClassName + " : IMissionActionEditorRenderer\r\n    {\r\n        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)\r\n        {\r\n            if (action?.managedAction is not " + runtimeClassName + " data)\r\n                return false;\r\n\r\n            EditorGUI.BeginChangeCheck();\r\n\r\n            if (!EditorGUI.EndChangeCheck())\r\n                return false;\r\n\r\n            context?.SaveGraphPositions?.Invoke();\r\n            context?.Repaint?.Invoke();\r\n            return true;\r\n        }\r\n    }\r\n}\r\n";
        }

        private static string BuildConditionEditorTemplate(string runtimeClassName, string editorClassName)
        {
            return "using UnityEditor;\r\n\r\nnamespace GAME.FlowSys.Editor\r\n{\r\n    [MissionConditionEditorRenderer(typeof(" + runtimeClassName + "))]\r\n    internal sealed class " + editorClassName + " : IMissionConditionEditorRenderer\r\n    {\r\n        public bool Draw(MissionStepConditionEntry condition, MissionInlineEditorContext context)\r\n        {\r\n            if (condition?.managedCondition is not " + runtimeClassName + " data)\r\n                return false;\r\n\r\n            EditorGUI.BeginChangeCheck();\r\n\r\n            if (!EditorGUI.EndChangeCheck())\r\n                return false;\r\n\r\n            context?.SaveGraphPositions?.Invoke();\r\n            context?.Repaint?.Invoke();\r\n            return true;\r\n        }\r\n    }\r\n}\r\n";
        }
    }
}
