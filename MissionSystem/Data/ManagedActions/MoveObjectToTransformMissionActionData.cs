using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class MoveObjectToTransformMissionActionData : MissionActionData
    {
        public string objectId;
        public string transformReferenceId;
        public float moveDuration = 1f;
        public bool useLocalSpace;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return $"Move '{objectId}' to '{transformReferenceId}' ({moveDuration}s)";
        }

        public override string GetTypeName()
        {
            return nameof(MoveObjectToTransformMissionActionData);
        }

        public override bool IsAsync => moveDuration > 0f;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj == null)
            {
                Debug.LogError($"[MissionSystem] Cannot move object: '{objectId}' not found");
                onComplete?.Invoke();
                return;
            }

            MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
            if (targetTransform == null)
            {
                Debug.LogError($"[MissionSystem] Cannot move object: transform reference '{transformReferenceId}' not found");
                onComplete?.Invoke();
                return;
            }

            Vector3 targetPosition = targetTransform.GetPosition();
            Quaternion targetRotation = targetTransform.GetRotation();
            if (useLocalSpace && obj.transform.parent != null)
            {
                targetPosition = obj.transform.parent.InverseTransformPoint(targetPosition);
                targetRotation = Quaternion.Inverse(obj.transform.parent.rotation) * targetRotation;
            }

            if (moveDuration <= 0f)
            {
                if (useLocalSpace)
                {
                    obj.transform.localPosition = targetPosition;
                    obj.transform.localRotation = targetRotation;
                }
                else
                {
                    obj.transform.position = targetPosition;
                    obj.transform.rotation = targetRotation;
                }
                onComplete?.Invoke();
                return;
            }

            Coroutine routine = context.StartCoroutine(MoveObjectCoroutine(obj.transform, targetPosition, targetRotation, moveDuration, useLocalSpace, () =>
            {
                RunRuntimeCleanup();
                onComplete?.Invoke();
            }));
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

        private IEnumerator MoveObjectCoroutine(Transform objTransform, Vector3 targetPosition, Quaternion targetRotation, float duration, bool localSpace, Action onComplete)
        {
            Vector3 startPosition = localSpace ? objTransform.localPosition : objTransform.position;
            Quaternion startRotation = localSpace ? objTransform.localRotation : objTransform.rotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (localSpace)
                {
                    objTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                    objTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                }
                else
                {
                    objTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                    objTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                }
                yield return null;
            }

            if (localSpace)
            {
                objTransform.localPosition = targetPosition;
                objTransform.localRotation = targetRotation;
            }
            else
            {
                objTransform.position = targetPosition;
                objTransform.rotation = targetRotation;
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
