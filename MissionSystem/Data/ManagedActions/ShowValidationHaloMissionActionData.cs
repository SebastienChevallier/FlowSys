using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class ShowValidationHaloMissionActionData : MissionActionData
    {
        public string objectId;
        public bool isValidPosition = true;
        public float haloDuration = 2f;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(objectId)
                ? $"Show {(isValidPosition ? "Green" : "Red")} Halo on '{objectId}' ({haloDuration}s)"
                : "Show Validation Halo";
        }

        public override string GetTypeName() => nameof(ShowValidationHaloMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj == null)
            {
                Debug.LogError($"[MissionSystem] Cannot show validation halo: object '{objectId}' not found");
                onComplete?.Invoke();
                return;
            }

            Coroutine routine = context.StartCoroutine(HideHaloAfterDelay(haloDuration, onComplete));
            AddRuntimeCleanup(() =>
            {
                if (routine != null)
                    context.StopCoroutine(routine);
            });
        }

        public override void HandleStepExit(IMissionContext context)
        {
            RunRuntimeCleanup();
        }

        private IEnumerator HideHaloAfterDelay(float delay, Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            onComplete?.Invoke();
        }

        private void AddRuntimeCleanup(Action cleanup)
        {
            runtimeCleanupActions += cleanup;
        }

        private void RunRuntimeCleanup()
        {
            Action callbacks = runtimeCleanupActions;
            runtimeCleanupActions = null;
            callbacks?.Invoke();
        }
    }
}
