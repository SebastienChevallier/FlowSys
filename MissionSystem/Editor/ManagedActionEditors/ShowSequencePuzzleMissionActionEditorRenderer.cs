using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.MissionSystem.Editor
{
    [MissionActionEditorRenderer(typeof(ShowSequencePuzzleMissionActionData))]
    internal sealed class ShowSequencePuzzleMissionActionEditorRenderer : IMissionActionEditorRenderer, IMissionUIElementRenderer
    {
        public bool Draw(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowSequencePuzzleMissionActionData data)
                return false;

            bool changed = false;

            EditorGUI.BeginChangeCheck();
            data.maxAttempts = EditorGUILayout.IntField("Max Attempts", data.maxAttempts);
            if (EditorGUI.EndChangeCheck())
                changed = true;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Vignettes", EditorStyles.boldLabel);

            string[] vignetteIds = data.vignetteIds ?? Array.Empty<string>();
            Sprite[] vignetteSprites = data.vignetteSprites ?? Array.Empty<Sprite>();
            int maxVignettes = Mathf.Max(vignetteIds.Length, vignetteSprites.Length);
            EnsureMatchingLength(ref vignetteIds, ref vignetteSprites, maxVignettes);

            for (int i = 0; i < maxVignettes; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Vignette {i}", EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    RemoveAt(ref vignetteIds, i);
                    RemoveAt(ref vignetteSprites, i);
                    data.vignetteIds = vignetteIds;
                    data.vignetteSprites = vignetteSprites;
                    TrimCorrectSequence(data, vignetteIds.Length);
                    changed = true;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                vignetteIds[i] = EditorGUILayout.TextField("Vignette ID", vignetteIds[i]);
                vignetteSprites[i] = (Sprite)EditorGUILayout.ObjectField("Sprite", vignetteSprites[i], typeof(Sprite), false);
                if (EditorGUI.EndChangeCheck())
                {
                    data.vignetteIds = vignetteIds;
                    data.vignetteSprites = vignetteSprites;
                    changed = true;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Vignette"))
            {
                Append(ref vignetteIds, string.Empty);
                Append(ref vignetteSprites, null);
                data.vignetteIds = vignetteIds;
                data.vignetteSprites = vignetteSprites;
                changed = true;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Correct Sequence", EditorStyles.boldLabel);

            int[] correctSequence = data.correctSequence ?? Array.Empty<int>();
            for (int i = 0; i < correctSequence.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                correctSequence[i] = EditorGUILayout.IntField($"Step {i}", correctSequence[i]);
                if (EditorGUI.EndChangeCheck())
                {
                    data.correctSequence = correctSequence;
                    changed = true;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    RemoveAt(ref correctSequence, i);
                    data.correctSequence = correctSequence;
                    changed = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Sequence Step"))
            {
                Append(ref correctSequence, 0);
                data.correctSequence = correctSequence;
                changed = true;
            }

            return MissionGraphEditorFieldUtility.ApplyChange(changed, context);
        }

        private static void EnsureMatchingLength(ref string[] ids, ref Sprite[] sprites, int length)
        {
            if (ids == null)
                ids = Array.Empty<string>();
            if (sprites == null)
                sprites = Array.Empty<Sprite>();

            if (ids.Length != length)
                Array.Resize(ref ids, length);
            if (sprites.Length != length)
                Array.Resize(ref sprites, length);
        }

        private static void TrimCorrectSequence(ShowSequencePuzzleMissionActionData data, int maxLength)
        {
            if (data.correctSequence == null)
            {
                data.correctSequence = Array.Empty<int>();
                return;
            }

            for (int i = 0; i < data.correctSequence.Length; i++)
                data.correctSequence[i] = Mathf.Clamp(data.correctSequence[i], 0, Mathf.Max(0, maxLength - 1));
        }

        private static void Append(ref string[] array, string value)
        {
            array ??= Array.Empty<string>();
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = value;
        }

        private static void Append(ref Sprite[] array, Sprite value)
        {
            array ??= Array.Empty<Sprite>();
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = value;
        }

        private static void Append(ref int[] array, int value)
        {
            array ??= Array.Empty<int>();
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = value;
        }

        private static void RemoveAt(ref string[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return;

            string[] result = new string[array.Length - 1];
            if (index > 0)
                Array.Copy(array, 0, result, 0, index);
            if (index < array.Length - 1)
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            array = result;
        }

        private static void RemoveAt(ref Sprite[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return;

            Sprite[] result = new Sprite[array.Length - 1];
            if (index > 0)
                Array.Copy(array, 0, result, 0, index);
            if (index < array.Length - 1)
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            array = result;
        }

        private static void RemoveAt(ref int[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
                return;

            int[] result = new int[array.Length - 1];
            if (index > 0)
                Array.Copy(array, 0, result, 0, index);
            if (index < array.Length - 1)
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            array = result;
        }

        public VisualElement BuildElement(MissionStepActionEntry action, MissionInlineEditorContext context)
        {
            if (action?.managedAction is not ShowSequencePuzzleMissionActionData data)
                return null;

            var paramTextColor = MissionGraphEditorUIUtility.GetParameterTextColor();
            int paramFontSize = MissionGraphEditorUIUtility.GetParameterFontSize();

            VisualElement root = new VisualElement();

            root.Add(MissionGraphEditorUIComponents.CreateIntField(
                "Max Attempts",
                data.maxAttempts,
                value =>
                {
                    data.maxAttempts = value;
                    context?.SaveGraphPositions?.Invoke();
                    context?.Repaint?.Invoke();
                },
                paramTextColor,
                paramFontSize));

            return root;
        }
    }
}
