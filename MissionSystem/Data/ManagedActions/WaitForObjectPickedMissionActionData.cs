using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class WaitForObjectPickedMissionActionData : MissionActionData
    {
        public string objectId;
        public bool enablePickingOnStart = true;
        [SerializeReference]
        public MissionActionData[] onPickedActions = Array.Empty<MissionActionData>();

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return onPickedActions != null && onPickedActions.Length > 0
                ? $"Wait Pick '{objectId}' ({onPickedActions.Length} reactions)"
                : $"Wait Pick '{objectId}'";
        }

        public override string GetTypeName() => nameof(WaitForObjectPickedMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            GameObject obj = context != null ? context.GetObjectById(objectId) : null;
            if (obj == null)
            {
                Debug.LogError($"[MissionSystem] Cannot wait for object picked: '{objectId}' not found in registry");
                onComplete?.Invoke();
                return;
            }

            if (!obj.activeInHierarchy)
                obj.SetActive(true);

            IPickable pickable = obj.GetComponent<IPickable>();
            if (pickable == null)
            {
                Debug.LogError($"[MissionSystem] Object '{objectId}' ({obj.name}) does not have IPickable component");
                onComplete?.Invoke();
                return;
            }

            pickable.ResetPickState();
            if (enablePickingOnStart)
                pickable.EnablePicking(true);

            Action handler = null;
            handler = () =>
            {
                pickable.OnPicked -= handler;
                RunRuntimeCleanup();
                ExecuteManagedActionSequence(context, onPickedActions, onComplete);
            };

            pickable.OnPicked += handler;
            AddRuntimeCleanup(() => pickable.OnPicked -= handler);
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
