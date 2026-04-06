using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class ShowMultipleChoiceDialogMissionActionData : MissionActionData
    {
        public string questionText;
        public string[] choiceTexts = Array.Empty<string>();
        public int correctChoiceIndex;
        public string wrongSound;
        public string correctSound;
        public string transformReferenceId;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(questionText)
                ? $"Multiple Choice: '{questionText.Substring(0, Math.Min(30, questionText.Length))}...' ({choiceTexts?.Length ?? 0} choices)"
                : "Show Multiple Choice Dialog";
        }

        public override string GetTypeName() => nameof(ShowMultipleChoiceDialogMissionActionData);
        public override bool IsAsync => true;

        private static void PositionInFrontOfCamera()
        {
            Camera cam = Camera.main;
            if (cam == null || MissionTextUIManager.Instance == null)
                return;

            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            Vector3 pos = cam.transform.position + forward * 1.5f;
            pos.y = cam.transform.position.y;
            Quaternion rot = Quaternion.LookRotation(-forward);
            MissionTextUIManager.Instance.MoveTo(pos, rot);
        }

        public override void Execute(IMissionContext context, Action onComplete)
        {
            Debug.Log($"[MissionSystem] ShowMultipleChoiceDialog executing: question='{questionText}', choices={choiceTexts?.Length ?? 0}, transformRef='{transformReferenceId}'");

            if (!string.IsNullOrEmpty(transformReferenceId))
            {
                MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
                if (targetTransform != null)
                    MissionTextUIManager.Instance?.MoveTo(targetTransform.GetPosition(), targetTransform.GetRotation());
                else
                {
                    Debug.LogWarning($"[MissionSystem] ShowMultipleChoiceDialog: transform reference '{transformReferenceId}' not found — positioning in front of camera");
                    PositionInFrontOfCamera();
                }
            }
            else
            {
                PositionInFrontOfCamera();
            }

            MissionTextUIManager uiManager = MissionTextUIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogError("[MissionSystem] Cannot show multiple choice dialog: UIManager not found");
                onComplete?.Invoke();
                return;
            }

            MultipleChoiceDialog dialog = uiManager.GetMultipleChoiceDialog();
            if (dialog == null)
            {
                Debug.LogError("[MissionSystem] Cannot show multiple choice dialog: MultipleChoiceDialog component not found in UIManager");
                onComplete?.Invoke();
                return;
            }

            uiManager.EnsureCanvasActive();

            Debug.Log($"[MissionSystem] ShowMultipleChoiceDialog: using UIManager '{uiManager.gameObject.name}'");

            dialog.Show(questionText, choiceTexts, correctChoiceIndex, correctSound, wrongSound, _ => onComplete?.Invoke());
            Debug.Log($"[MissionSystem] ShowMultipleChoiceDialog: dialog '{dialog.gameObject.name}' activeSelf={dialog.gameObject.activeSelf} activeInHierarchy={dialog.gameObject.activeInHierarchy}");
        }
    }
}
