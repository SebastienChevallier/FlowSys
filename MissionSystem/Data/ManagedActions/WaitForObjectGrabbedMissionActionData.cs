using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class WaitForObjectGrabbedMissionActionData : MissionActionData
    {
        public string objectId;
        [SerializeReference]
        public MissionActionData[] onGrabbedActions = Array.Empty<MissionActionData>();

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return onGrabbedActions != null && onGrabbedActions.Length > 0
                ? $"Wait Grab '{objectId}' ({onGrabbedActions.Length} reactions)"
                : $"Wait Grab '{objectId}'";
        }

        public override string GetTypeName() => nameof(WaitForObjectGrabbedMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionInteractionStateManager interactionStateManager = MissionInteractionStateManager.Instance;
            if (interactionStateManager == null)
            {
                Debug.LogError($"[MissionSystem] Cannot wait for object grabbed: MissionInteractionStateManager is unavailable for '{objectId}'.");
                onComplete?.Invoke();
                return;
            }

            if (!interactionStateManager.TrySubscribeToLogicalGrab(objectId, () =>
            {
                RunRuntimeCleanup();
                ExecuteManagedActionSequence(context, onGrabbedActions, onComplete);
            }, out Action cleanup))
            {
                Debug.LogError($"[MissionSystem] Cannot wait for object grabbed: no runtime grabbable resolved for logical id '{objectId}'.");
                onComplete?.Invoke();
                return;
            }

            AddRuntimeCleanup(cleanup);
        }

        public override void HandleStepExit(IMissionContext context)
        {
            RunRuntimeCleanup();
        }

        private static void ExecuteManagedActionSequence(IMissionContext context, MissionActionData[] actions, Action onComplete)
        {
            if (actions == null || actions.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            ExecuteManagedActionAtIndex(context, actions, 0, onComplete);
        }

        private static void ExecuteManagedActionAtIndex(IMissionContext context, MissionActionData[] actions, int index, Action onComplete)
        {
            if (actions == null || index >= actions.Length)
            {
                onComplete?.Invoke();
                return;
            }

            MissionActionData action = actions[index];
            if (action == null)
            {
                ExecuteManagedActionAtIndex(context, actions, index + 1, onComplete);
                return;
            }

            action.Execute(context, () => ExecuteManagedActionAtIndex(context, actions, index + 1, onComplete));
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
