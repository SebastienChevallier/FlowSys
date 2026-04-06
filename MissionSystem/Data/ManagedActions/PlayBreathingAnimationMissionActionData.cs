using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Animation")]
    [Serializable]
    public sealed class PlayBreathingAnimationMissionActionData : MissionActionData
    {
        public string objectId;
        public bool loopAnimation = true;
        public string breathingSound;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(objectId)
                ? $"Play Breathing on '{objectId}' {(loopAnimation ? "(loop)" : string.Empty)}"
                : "Play Breathing Animation";
        }

        public override string GetTypeName() => nameof(PlayBreathingAnimationMissionActionData);
        public override bool IsAsync => !loopAnimation;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj == null)
            {
                Debug.LogError($"[MissionSystem] Cannot play breathing animation: object '{objectId}' not found");
                onComplete?.Invoke();
                return;
            }

            Animator animator = obj.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"[MissionSystem] Cannot play breathing animation: no Animator found on '{objectId}'");
                onComplete?.Invoke();
                return;
            }

            animator.SetBool("IsBreathing", true);
            if (loopAnimation)
            {
                onComplete?.Invoke();
                return;
            }

            Coroutine routine = context.StartCoroutine(StopBreathingAfterDelay(animator, 2f, onComplete));
            AddRuntimeCleanup(() =>
            {
                if (routine != null)
                    context.StopCoroutine(routine);
                animator.SetBool("IsBreathing", false);
            });
        }

        public override void HandleStepExit(IMissionContext context)
        {
            RunRuntimeCleanup();
        }

        private IEnumerator StopBreathingAfterDelay(Animator animator, float delay, Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            animator.SetBool("IsBreathing", false);
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
