using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class WaitForObjectManipulationMissionActionData : MissionActionData
    {
        public string objectId;
        public string transformReferenceId;
        public bool enableGhostGuide = true;
        public float manipulationThreshold = 0.1f;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(objectId) && !string.IsNullOrEmpty(transformReferenceId)
                ? $"Wait Manipulate '{objectId}' to '{transformReferenceId}' {(enableGhostGuide ? "(+Ghost)" : string.Empty)}"
                : "Wait For Object Manipulation";
        }

        public override string GetTypeName()
        {
            return nameof(WaitForObjectManipulationMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (string.IsNullOrEmpty(objectId) || string.IsNullOrEmpty(transformReferenceId))
            {
                Debug.LogError("[MissionSystem] Cannot wait for object manipulation: objectId or transformReferenceId is null or empty");
                onComplete?.Invoke();
                return;
            }

            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj == null)
            {
                Debug.LogError($"[MissionSystem] Cannot wait for object manipulation: object '{objectId}' not found");
                onComplete?.Invoke();
                return;
            }

            MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
            if (targetTransform == null)
            {
                Debug.LogError($"[MissionSystem] Cannot wait for object manipulation: transform reference '{transformReferenceId}' not found");
                onComplete?.Invoke();
                return;
            }

            Coroutine routine = context.StartCoroutine(WaitForObjectManipulationCoroutine(obj, targetTransform.GetPosition(), onComplete));
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

        private IEnumerator WaitForObjectManipulationCoroutine(GameObject obj, Vector3 targetPosition, Action onComplete)
        {
            while (obj != null)
            {
                if (Vector3.Distance(obj.transform.position, targetPosition) <= manipulationThreshold)
                {
                    RunRuntimeCleanup();
                    onComplete?.Invoke();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

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
