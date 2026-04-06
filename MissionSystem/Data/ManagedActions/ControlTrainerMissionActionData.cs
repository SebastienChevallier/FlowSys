using System;
using System.Collections;
using UnityEngine;
using Evaveo.nsVorta.HighLevel;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class ControlTrainerMissionActionData : MissionActionData
    {
        public string objectId;
        public TrainerVisibilityMode trainerVisibilityMode = TrainerVisibilityMode.NoChange;
        public string transformReferenceId;
        public bool useCustomTrainerAnimation;
        public string animationName;
        public bool syncTrainerWithVoiceOver;
        public string voiceOverKey;
        public bool waitForCompletion = true;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return !string.IsNullOrEmpty(objectId)
                ? $"Trainer '{objectId}' ({trainerVisibilityMode})"
                : "Control Trainer";
        }

        public override string GetTypeName()
        {
            return nameof(ControlTrainerMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject trainerObject = context != null ? context.GetObjectById(objectId) : null;
            if (trainerObject == null)
            {
                Debug.LogError($"[MissionSystem] Cannot control trainer: object '{objectId}' not found");
                onComplete?.Invoke();
                return;
            }

            ApplyTrainerVisibility(trainerObject, trainerVisibilityMode);

            if (!string.IsNullOrEmpty(transformReferenceId))
            {
                MissionTransformReference targetTransform = MissionRuntimeResolver.FindTransformReference(transformReferenceId);
                if (targetTransform == null)
                {
                    Debug.LogError($"[MissionSystem] Cannot control trainer: transform reference '{transformReferenceId}' not found");
                    onComplete?.Invoke();
                    return;
                }

                trainerObject.transform.SetPositionAndRotation(targetTransform.GetPosition(), targetTransform.GetRotation());
            }

            MissionAnimatorController animatorController = trainerObject.GetComponent<MissionAnimatorController>();
            MissionTrainerController trainerController = trainerObject.GetComponent<MissionTrainerController>();

            if (!string.IsNullOrEmpty(voiceOverKey))
            {
                StartTrainerVoicePlayback(animatorController, trainerController, trainerObject);

                Action afterVoiceOver = () =>
                {
                    StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);
                    onComplete?.Invoke();
                };

                if (waitForCompletion)
                {
                    context.PlayVoiceOverByKey(voiceOverKey, afterVoiceOver);
                }
                else
                {
                    context.PlayVoiceOverByKey(voiceOverKey, () =>
                    {
                        StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);
                    });
                    onComplete?.Invoke();
                }

                return;
            }

            if (syncTrainerWithVoiceOver || !useCustomTrainerAnimation)
            {
                Action unsubscribe = SubscribeTrainerToVoiceOverEvents(animatorController, trainerController, trainerObject);
                AddRuntimeCleanup(() =>
                {
                    unsubscribe?.Invoke();
                    StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);
                });
                onComplete?.Invoke();
                return;
            }

            if (useCustomTrainerAnimation && !string.IsNullOrEmpty(animationName))
                PlayTrainerAnimation(animatorController, trainerObject, animationName);
            else
                StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);

            onComplete?.Invoke();
        }

        public override void HandleStepExit(IMissionContext context)
        {
            RunRuntimeCleanup();
        }

        private void AddRuntimeCleanup(Action cleanup)
        {
            runtimeCleanupActions += cleanup;
        }

        private void RunRuntimeCleanup()
        {
            Action callbacks = runtimeCleanupActions;
            runtimeCleanupActions = null;
            if (callbacks == null)
                return;

            Delegate[] handlers = callbacks.GetInvocationList();
            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                try
                {
                    if (handlers[i] is Action action)
                        action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void ApplyTrainerVisibility(GameObject trainerObject, TrainerVisibilityMode visibilityMode)
        {
            if (trainerObject == null || visibilityMode == TrainerVisibilityMode.NoChange)
                return;

            bool visible = visibilityMode == TrainerVisibilityMode.Show;
            LODGroup lodGroup = trainerObject.GetComponent<LODGroup>();
            if (lodGroup != null)
                lodGroup.enabled = visible;

            Renderer[] renderers = trainerObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }

            Collider[] colliders = trainerObject.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = visible;
            }
        }

        private void PlayTrainerAnimation(MissionAnimatorController animatorController, GameObject trainerObject, string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName) || trainerObject == null)
                return;

            if (animatorController != null)
            {
                animatorController.PlayAnimation(triggerName);
                return;
            }

            Animator animator = trainerObject.GetComponent<Animator>();
            if (animator == null)
                animator = trainerObject.GetComponentInChildren<Animator>(true);

            if (animator != null)
                animator.SetTrigger(triggerName);
        }

        private void StartTrainerTalking(MissionAnimatorController animatorController, MissionTrainerController trainerController, GameObject trainerObject)
        {
            if (trainerController != null)
            {
                trainerController.StartTalking();
                return;
            }

            if (animatorController != null)
            {
                animatorController.StartTalking();
                return;
            }

            PlayTrainerAnimation(animatorController, trainerObject, "Idle");
        }

        private void StopTrainerTalking(MissionAnimatorController animatorController, MissionTrainerController trainerController, GameObject trainerObject)
        {
            if (trainerController != null)
            {
                trainerController.StopTalking();
                trainerController.PlayIdle();
                return;
            }

            if (animatorController != null)
            {
                animatorController.StopTalking();
                PlayTrainerAnimation(animatorController, trainerObject, "Idle");
                return;
            }

            PlayTrainerAnimation(animatorController, trainerObject, "Idle");
        }

        private void StartTrainerVoicePlayback(MissionAnimatorController animatorController, MissionTrainerController trainerController, GameObject trainerObject)
        {
            if (useCustomTrainerAnimation && !string.IsNullOrEmpty(animationName))
            {
                PlayTrainerAnimation(animatorController, trainerObject, animationName);
                return;
            }

            StartTrainerTalking(animatorController, trainerController, trainerObject);
        }

        private void StopTrainerVoicePlayback(MissionAnimatorController animatorController, MissionTrainerController trainerController, GameObject trainerObject)
        {
            StopTrainerTalking(animatorController, trainerController, trainerObject);
        }

        private Action SubscribeTrainerToVoiceOverEvents(MissionAnimatorController animatorController, MissionTrainerController trainerController, GameObject trainerObject)
        {
            bool isSubscribed = true;

            void HandleVoiceOverStarted(string _)
            {
                if (!isSubscribed)
                    return;

                StartTrainerVoicePlayback(animatorController, trainerController, trainerObject);
            }

            void HandleVoiceOverFinished(string _, bool __)
            {
                if (!isSubscribed)
                    return;

                StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);
            }

            VoixOffManager.VoiceOverStarted += HandleVoiceOverStarted;
            VoixOffManager.VoiceOverFinished += HandleVoiceOverFinished;

            if (VoixOffManager.IsVoiceOverPlaying)
                StartTrainerVoicePlayback(animatorController, trainerController, trainerObject);
            else
                StopTrainerVoicePlayback(animatorController, trainerController, trainerObject);

            return () =>
            {
                if (!isSubscribed)
                    return;

                isSubscribed = false;
                VoixOffManager.VoiceOverStarted -= HandleVoiceOverStarted;
                VoixOffManager.VoiceOverFinished -= HandleVoiceOverFinished;
            };
        }
    }
}
