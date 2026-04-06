using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Evaveo.nsVorta.HighLevel;
using GAME.MissionSystem;

namespace GAME.MissionSystem
{
    /// <summary>
    /// État concret pour une étape de mission
    /// Gère l'exécution séquentielle des actions (sync et async)
    /// </summary>
    public class MissionStepState : IMissionState
    {
        private class PendingStepAction
        {
            public string Name;
            public string TypeName;
            public bool IsAsync;
            public Action<IMissionContext, Action> Execute;
        }

        private MissionStepConfigSO stepConfig;
        private Queue<PendingStepAction> pendingActions;
        private bool isExecutingActions = false;
        private bool actionsCompleted = false;
        private bool stepCompletionRequested = false;
        private int asyncExecutionVersion = 0;
        private bool wasTriggerPressedLastFrame = false;
        private bool forceAdvanceConsumedForCurrentStep = false;
        private bool forceAdvancePressLatched = false;
        private static bool globalForceAdvanceInProgress = false;
        private string completionTransitionId = null;

        public event Action<string> OnStepComplete;

        public bool AreOnEnterActionsComplete => actionsCompleted && !isExecutingActions;

        public MissionStepState(MissionStepConfigSO config)
        {
            stepConfig = config;
        }

        public void OnEnter(IMissionContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[MissionSystem] Entering step: {stepConfig.stepName}");
            Debug.Log($"[MissionSystem] Step '{stepConfig.stepName}' onEnterActions count: {GetPhaseActionCount(MissionStepActionPhase.OnEnter)}");
#endif

            pendingActions = BuildPendingActions(MissionStepActionPhase.OnEnter);
            actionsCompleted = false;
            isExecutingActions = false;
            stepCompletionRequested = false;
            asyncExecutionVersion++;
            completionTransitionId = null;
            forceAdvanceConsumedForCurrentStep = false;
            wasTriggerPressedLastFrame = IsAnyForceAdvanceTriggerPressed();

            if (!wasTriggerPressedLastFrame)
            {
                forceAdvancePressLatched = false;
            }

            globalForceAdvanceInProgress = false;

            ExecuteNextAction(context);
        }

        private int GetPhaseActionCount(MissionStepActionPhase phase)
        {
            return stepConfig != null ? stepConfig.GetStructuredActionCount(phase) : 0;
        }

        private Queue<PendingStepAction> BuildPendingActions(MissionStepActionPhase phase)
        {
            Queue<PendingStepAction> actions = new Queue<PendingStepAction>();

            List<MissionStepActionEntry> entries = stepConfig != null ? stepConfig.GetStructuredActionEntries(phase) : null;
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    MissionStepActionEntry capturedEntry = entry;
                    actions.Enqueue(new PendingStepAction
                    {
                        Name = GetEntryActionName(capturedEntry),
                        TypeName = GetEntryTypeName(capturedEntry),
                        IsAsync = GetEntryIsAsync(capturedEntry),
                        Execute = (ctx, onComplete) => ExecuteEntry(capturedEntry, ctx, onComplete)
                    });
                }
            }

            return actions;
        }

        private string GetEntryActionName(MissionStepActionEntry entry)
        {
            if (entry == null)
                return "<null>";

            if (entry.managedAction != null)
                return entry.managedAction.GetDisplayName();

            return "<unsupported-legacy-action>";
        }

        private string GetEntryTypeName(MissionStepActionEntry entry)
        {
            if (entry == null)
                return "<null>";

            if (entry.managedAction != null)
                return entry.managedAction.GetTypeName();

            return "<unsupported-legacy-action>";
        }

        private bool GetEntryIsAsync(MissionStepActionEntry entry)
        {
            if (entry == null)
                return false;

            if (entry.managedAction != null)
                return entry.managedAction.IsAsync;

            return false;
        }

        private void ExecuteEntry(MissionStepActionEntry entry, IMissionContext context, Action onComplete)
        {
            if (entry == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (entry.managedAction != null)
            {
                entry.managedAction.Execute(context, onComplete);
                return;
            }

            onComplete?.Invoke();
        }

        private void ExecuteNextAction(IMissionContext context)
        {
            if (pendingActions.Count == 0)
            {
                // Si aucune action à exécuter, vérifier s'il y a des conditions
                List<MissionStepConditionEntry> conditionEntries = stepConfig != null ? stepConfig.GetStructuredConditionEntries() : null;
                bool hasConditions = conditionEntries != null && conditionEntries.Count > 0;

                if (!hasConditions)
                {
                    // Pas d'actions ET pas de conditions = step terminée
                    OnActionsCompleted(context);
                }
                else
                {
                    // Pas d'actions mais des conditions existent = actions considérées comme terminées
                    // mais on ne complète pas la step immédiatement
                    isExecutingActions = false;
                    actionsCompleted = true;
                }
                return;
            }

            isExecutingActions = true;
            PendingStepAction action = pendingActions.Dequeue();

            if (action == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[MissionSystem] Null action in step {stepConfig.stepName}");
#endif
                ExecuteNextAction(context);
                return;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[MissionSystem] Executing action in step '{stepConfig.stepName}': {action.Name} (Type: {action.TypeName}, Async: {action.IsAsync})");
#endif

            if (action.IsAsync)
            {
                int callbackVersion = asyncExecutionVersion;
                action.Execute(context, () =>
                {
                    if (stepCompletionRequested || callbackVersion != asyncExecutionVersion)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log($"[MissionSystem] Ignoring stale async callback in step '{stepConfig.stepName}': {action.Name}");
#endif
                        return;
                    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[MissionSystem] Action completed in step '{stepConfig.stepName}': {action.Name}");
#endif
                    ExecuteNextAction(context);
                });
            }
            else
            {
                action.Execute(context, null);
                ExecuteNextAction(context);
            }
        }

        private void OnActionsCompleted(IMissionContext context)
        {
            if (stepCompletionRequested)
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[MissionSystem] All onEnter actions completed for step: {stepConfig.stepName}");
#endif
            foreach (var toggle in stepConfig.userActionToggles)
            {
                UserActionsManager.SetActionCanDo(toggle.actionId, toggle.enabled);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[MissionSystem] UserAction '{toggle.actionId}' set to {toggle.enabled}");
#endif
            }

            isExecutingActions = false;
            actionsCompleted = true;

            List<MissionStepConditionEntry> conditionEntries = stepConfig != null ? stepConfig.GetStructuredConditionEntries() : null;
            bool hasConditions = conditionEntries != null && conditionEntries.Count > 0;
            if (!hasConditions)
            {
                completionTransitionId = null;
                stepCompletionRequested = true;
                OnStepComplete?.Invoke(completionTransitionId);
            }
        }

        public void Update(IMissionContext context)
        {
            if (stepCompletionRequested)
                return;

            if (TryForceAdvance(context))
                return;

            if (!actionsCompleted || isExecutingActions)
                return;

            if (AllConditionsMet(context))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[MissionSystem] All exit conditions met for step: {stepConfig.stepName}");
#endif
                completionTransitionId = ResolveCompletionTransitionId(context);
                stepCompletionRequested = true;
                OnStepComplete?.Invoke(completionTransitionId);
            }
        }

        private bool TryForceAdvance(IMissionContext context)
        {
            if (globalForceAdvanceInProgress)
                return false;

            if (forceAdvanceConsumedForCurrentStep)
                return false;

            List<MissionStepConditionEntry> forceAdvanceConditions = GetForceAdvanceConditionEntries();
            if (forceAdvanceConditions == null || forceAdvanceConditions.Count == 0)
                return false;

            bool isAnyTriggerPressed = false;
            string triggeredTransitionId = null;
            for (int i = 0; i < forceAdvanceConditions.Count; i++)
            {
                MissionStepConditionEntry entry = forceAdvanceConditions[i];
                if (TryGetForceAdvanceTriggerSettings(entry, out XRControllerHand triggerHand, out float triggerThreshold)
                    && IsTriggerPressed(triggerHand, triggerThreshold))
                {
                    isAnyTriggerPressed = true;
                    triggeredTransitionId = entry.targetTransitionId;
                    break;
                }
            }

            if (!isAnyTriggerPressed)
            {
                wasTriggerPressedLastFrame = false;
                forceAdvancePressLatched = false;
                return false;
            }

            if (forceAdvancePressLatched)
            {
                wasTriggerPressedLastFrame = true;
                return false;
            }

            bool triggerJustPressed = isAnyTriggerPressed && !wasTriggerPressedLastFrame;
            wasTriggerPressedLastFrame = isAnyTriggerPressed;

            if (!triggerJustPressed)
                return false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[MissionSystem] Force advance triggered on step: {stepConfig.stepName}");
#endif
            globalForceAdvanceInProgress = true;
            forceAdvanceConsumedForCurrentStep = true;
            forceAdvancePressLatched = true;
            completionTransitionId = triggeredTransitionId;
            stepCompletionRequested = true;
            asyncExecutionVersion++;
            pendingActions?.Clear();
            isExecutingActions = false;
            actionsCompleted = true;
            context.InterruptAsyncOperations();
            OnStepComplete?.Invoke(completionTransitionId);
            return true;
        }

        private List<MissionStepConditionEntry> GetForceAdvanceConditionEntries()
        {
            List<MissionStepConditionEntry> entries = stepConfig != null ? stepConfig.GetStructuredConditionEntries() : null;
            if (entries == null || entries.Count == 0)
                return null;

            List<MissionStepConditionEntry> results = new List<MissionStepConditionEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                MissionStepConditionEntry entry = entries[i];
                if (IsForceAdvanceConditionEntry(entry))
                    results.Add(entry);
            }

            return results;
        }

        private bool IsAnyForceAdvanceTriggerPressed()
        {
            List<MissionStepConditionEntry> forceAdvanceConditions = GetForceAdvanceConditionEntries();
            if (forceAdvanceConditions == null || forceAdvanceConditions.Count == 0)
                return false;

            for (int i = 0; i < forceAdvanceConditions.Count; i++)
            {
                MissionStepConditionEntry entry = forceAdvanceConditions[i];
                if (TryGetForceAdvanceTriggerSettings(entry, out XRControllerHand triggerHand, out float triggerThreshold)
                    && IsTriggerPressed(triggerHand, triggerThreshold))
                    return true;
            }

            return false;
        }

        private static bool IsForceAdvanceConditionEntry(MissionStepConditionEntry entry)
        {
            if (entry == null)
                return false;

            return entry.managedCondition is ForceAdvanceOnTriggerPressedMissionConditionData;
        }

        private static bool TryGetForceAdvanceTriggerSettings(MissionStepConditionEntry entry, out XRControllerHand triggerHand, out float triggerThreshold)
        {
            triggerHand = XRControllerHand.Either;
            triggerThreshold = 0.5f;

            if (entry == null)
                return false;

            if (entry.managedCondition is ForceAdvanceOnTriggerPressedMissionConditionData managedForceAdvanceCondition)
            {
                triggerHand = managedForceAdvanceCondition.triggerHand;
                triggerThreshold = managedForceAdvanceCondition.triggerThreshold;
                return true;
            }

            return false;
        }

        private string ResolveCompletionTransitionId(IMissionContext context)
        {
            List<MissionStepConditionEntry> entries = stepConfig != null ? stepConfig.GetStructuredConditionEntries() : null;
            if (entries == null || entries.Count == 0)
                return null;

            for (int i = 0; i < entries.Count; i++)
            {
                MissionStepConditionEntry entry = entries[i];
                if (entry == null)
                    continue;

                if (TryResolveSpecialConditionTransitionId(entry, context, out string specialTransitionId))
                    return specialTransitionId;

                if (!EvaluateConditionEntry(entry, context))
                    continue;

                if (!string.IsNullOrEmpty(entry.targetTransitionId))
                    return entry.targetTransitionId;
            }

            return null;
        }

        private string ResolveTimerConditionTransitionId(MissionStepConditionEntry entry)
        {
            if (entry?.managedCondition is not TimerCountdownMissionConditionData timerCondition)
                return null;

            MissionTimerManager timerManager = MissionTimerManager.instance;
            if (timerManager == null)
                return null;

            bool timerExpired = timerManager.IsTimerExpired(timerCondition.timerId);
            if (timerExpired)
            {
                if (!string.IsNullOrEmpty(entry.secondaryTargetTransitionId))
                    return entry.secondaryTargetTransitionId;

                return entry.targetTransitionId;
            }

            if (EvaluateConditionEntry(entry, null))
                return entry.targetTransitionId;

            return null;
        }

        private bool TryResolveSpecialConditionTransitionId(MissionStepConditionEntry entry, IMissionContext context, out string transitionId)
        {
            transitionId = null;

            if (entry?.managedCondition is TimerCountdownMissionConditionData)
            {
                transitionId = ResolveTimerConditionTransitionId(entry);
                return !string.IsNullOrEmpty(transitionId);
            }

            return false;
        }

        private bool AllConditionsMet(IMissionContext context)
        {
            List<MissionStepConditionEntry> entries = stepConfig != null ? stepConfig.GetStructuredConditionEntries() : null;
            if (entries != null && entries.Count > 0)
            {
                foreach (var condition in entries)
                {
                    if (condition == null)
                    {
                        Debug.LogWarning($"[MissionSystem] Null inline/entry condition in step {stepConfig.stepName}");
                        continue;
                    }

                    if (condition.managedCondition is TimerCountdownMissionConditionData)
                    {
                        if (string.IsNullOrEmpty(ResolveTimerConditionTransitionId(condition)))
                            return false;
                        continue;
                    }

                    if (!EvaluateConditionEntry(condition, context))
                        return false;
                }

                return true;
            }
            return false;
        }

        private bool EvaluateConditionEntry(MissionStepConditionEntry entry, IMissionContext context)
        {
            if (entry == null)
                return true;

            if (entry.managedCondition != null)
            {
                WireConditionEvaluators(entry.managedCondition);
                return entry.managedCondition.Evaluate(context);
            }

            return true;
        }

        private void WireConditionEvaluators(MissionConditionData condition)
        {
            if (condition == null)
                return;

            if (condition is AllStepActionsCompletedMissionConditionData allActionsCompletedCondition)
                allActionsCompletedCondition.areActionsCompleted = AreStepActionsCompleted;

            if (condition is TriggerPressedMissionConditionData triggerPressedCondition)
                triggerPressedCondition.triggerEvaluator = IsTriggerPressed;

            if (condition is TimerCountdownMissionConditionData timerCountdownCondition)
                timerCountdownCondition.timerEvaluator = EvaluateTimerCountdown;

            // Wire new interaction condition evaluators using reflection
            string typeName = condition.GetTypeName();
            System.Type conditionType = condition.GetType();

            if (typeName == "SnapObjectMissionConditionData")
            {
                System.Reflection.FieldInfo field = conditionType.GetField("snapStateEvaluator",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    field.SetValue(condition, new System.Func<string, bool>(EvaluateSnapState));
            }
            else if (typeName == "GrabObjectMissionConditionData")
            {
                System.Reflection.FieldInfo field = conditionType.GetField("grabStateEvaluator",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    field.SetValue(condition, new System.Func<string, bool>(EvaluateGrabState));
            }
            else if (typeName == "InteractiveZoneEntryMissionConditionData")
            {
                System.Reflection.FieldInfo field = conditionType.GetField("zoneEntryEvaluator",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    System.Func<string, MissionControllerType, bool> evaluator = (zoneId, controllers) => EvaluateZoneEntry(zoneId, controllers);
                    field.SetValue(condition, evaluator);
                }
            }

            if (condition is CompoundMissionConditionData compound)
            {
                if (compound.SubConditions != null)
                {
                    foreach (var subEntry in compound.SubConditions)
                    {
                        if (subEntry?.managedCondition != null)
                            WireConditionEvaluators(subEntry.managedCondition);
                    }
                }
            }
        }

        private bool IsTriggerPressed(XRControllerHand hand, float threshold)
        {
            float clampedThreshold = Mathf.Clamp01(threshold);

            switch (hand)
            {
                case XRControllerHand.Left:
                    return GetTriggerValue(XRNode.LeftHand) > clampedThreshold;
                case XRControllerHand.Right:
                    return GetTriggerValue(XRNode.RightHand) > clampedThreshold;
                case XRControllerHand.Either:
                default:
                    return GetTriggerValue(XRNode.LeftHand) > clampedThreshold
                        || GetTriggerValue(XRNode.RightHand) > clampedThreshold;
            }
        }

        private float GetTriggerValue(XRNode handNode)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);
            if (!device.isValid)
                return 0f;

            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                return triggerValue;

            return 0f;
        }

        private bool AreStepActionsCompleted()
        {
            return actionsCompleted && !isExecutingActions;
        }

        private bool EvaluateTimerCountdown(string timerId, bool requireAllActionsCompleted)
        {
            MissionTimerManager timerManager = MissionTimerManager.instance;

            if (timerManager == null)
            {
                Debug.LogWarning("[MissionSystem] MissionTimerManager not found. Timer condition fails.");
                return false;
            }

            bool timerExpired = timerManager.IsTimerExpired(timerId);
            if (timerExpired)
            {
                Debug.Log($"[MissionSystem] Timer '{timerId}' expired (<=0). Condition fails.");
                return false;
            }

            if (requireAllActionsCompleted && !AreStepActionsCompleted())
                return false;

            float remainingTime = timerManager.GetRemainingTime(timerId);
            Debug.Log($"[MissionSystem] Timer '{timerId}' condition met ({remainingTime:F1}s remaining).");
            return true;
        }

        private bool EvaluateSnapState(string objectId)
        {
            return EvaluateInteractionState("IsObjectSnapped", objectId, "Snap");
        }

        private bool EvaluateGrabState(string objectId)
        {
            return EvaluateInteractionState("IsObjectGrabbed", objectId, "Grab");
        }

        private bool EvaluateZoneEntry(string objectId, MissionControllerType allowedControllers)
        {
            return EvaluateInteractionState("IsZoneEntered", objectId, allowedControllers, "Zone entry");
        }

        private bool EvaluateInteractionState(string methodName, string objectId, string conditionName)
        {
            return EvaluateInteractionState(methodName, objectId, null, conditionName);
        }

        private bool EvaluateInteractionState(string methodName, string objectId, MissionControllerType? allowedControllers, string conditionName)
        {
            try
            {
                // Use reflection to access MissionInteractionStateManager
                System.Type managerType = System.Type.GetType("GAME.MissionSystem.MissionInteractionStateManager");
                if (managerType == null)
                {
                    Debug.LogWarning($"[MissionSystem] MissionInteractionStateManager type not found. {conditionName} condition fails.");
                    return false;
                }

                // Get the Instance property
                System.Reflection.PropertyInfo instanceProp = managerType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProp == null)
                {
                    Debug.LogWarning($"[MissionSystem] MissionInteractionStateManager.Instance property not found. {conditionName} condition fails.");
                    return false;
                }

                object manager = instanceProp.GetValue(null);
                if (manager == null)
                {
                    Debug.LogWarning($"[MissionSystem] MissionInteractionStateManager instance is null. {conditionName} condition fails.");
                    return false;
                }

                // Get the appropriate method based on parameters
                System.Reflection.MethodInfo method = null;
                object[] methodParams = null;

                if (allowedControllers.HasValue)
                {
                    // Zone entry needs the MissionControllerType parameter
                    System.Type controllerEnumType = System.Type.GetType("GAME.MissionSystem.MissionControllerType");
                    method = managerType.GetMethod(methodName,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, new System.Type[] { typeof(string), controllerEnumType }, null);
                    methodParams = new object[] { objectId, allowedControllers.Value };
                }
                else
                {
                    // Snap/Grab need only objectId
                    method = managerType.GetMethod(methodName,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, new System.Type[] { typeof(string) }, null);
                    methodParams = new object[] { objectId };
                }

                if (method == null)
                {
                    Debug.LogWarning($"[MissionSystem] MissionInteractionStateManager.{methodName} method not found. {conditionName} condition fails.");
                    return false;
                }

                object result = method.Invoke(manager, methodParams);
                return result is bool && (bool)result;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MissionSystem] Error evaluating {conditionName} condition: {ex.Message}");
                return false;
            }
        }

        public void OnExit(IMissionContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[MissionSystem] Exiting step: {stepConfig.stepName}");
#endif

            stepCompletionRequested = true;
            asyncExecutionVersion++;
            pendingActions?.Clear();
            isExecutingActions = false;

            List<MissionStepActionEntry> enterActions = stepConfig != null ? stepConfig.GetStructuredActionEntries(MissionStepActionPhase.OnEnter) : null;
            if (enterActions != null)
            {
                for (int i = 0; i < enterActions.Count; i++)
                {
                    MissionStepActionEntry entry = enterActions[i];
                    if (entry == null)
                        continue;

                    if (entry.managedAction != null)
                    {
                        entry.managedAction.HandleStepExit(context);
                    }
                }
            }

            Queue<PendingStepAction> exitActions = BuildPendingActions(MissionStepActionPhase.OnExit);
            while (exitActions.Count > 0)
            {
                PendingStepAction action = exitActions.Dequeue();
                if (action != null)
                {
                    action.Execute(context, null);
                }
            }
        }
    }
}
