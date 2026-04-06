using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class WaitForPlayerPositionMissionActionData : MissionActionData
    {
        public string transformReferenceId;
        public float validationRadius = 0.3f;
        public string bodyPartToTrack;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(bodyPartToTrack)
                ? $"Wait Player Position '{bodyPartToTrack}' at '{transformReferenceId}' (r={validationRadius}m)"
                : "Wait For Player Position";
        }

        public override string GetTypeName()
        {
            return nameof(WaitForPlayerPositionMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (string.IsNullOrEmpty(transformReferenceId))
            {
                Debug.LogError("[MissionSystem] Cannot wait for player position: transformReferenceId is null or empty");
                onComplete?.Invoke();
                return;
            }

            MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
            if (targetTransform == null)
            {
                Debug.LogError($"[MissionSystem] Cannot wait for player position: transform reference '{transformReferenceId}' not found");
                onComplete?.Invoke();
                return;
            }

            Coroutine routine = context.StartCoroutine(WaitForPlayerPositionCoroutine(targetTransform.GetPosition(), onComplete));
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

        private IEnumerator WaitForPlayerPositionCoroutine(Vector3 targetPosition, Action onComplete)
        {
            Transform playerTransform = Camera.main != null ? Camera.main.transform : null;
            if (playerTransform == null)
            {
                Debug.LogError("[MissionSystem] Cannot find player camera transform");
                onComplete?.Invoke();
                yield break;
            }

            while (true)
            {
                if (Vector3.Distance(playerTransform.position, targetPosition) <= validationRadius)
                {
                    RunRuntimeCleanup();
                    onComplete?.Invoke();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
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
