using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Object")]
    [Serializable]
    public sealed class WaitForObjectsPickedGroupMissionActionData : MissionActionData
    {
        public bool enablePickingOnStart = true;
        public bool waitForVoiceOverCompletion;
        public float completionDelaySeconds;
        public ManagedObjectPickReactionItem[] items = Array.Empty<ManagedObjectPickReactionItem>();

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName() => $"Wait Picks Group ({(items != null ? items.Length : 0)})";
        public override string GetTypeName() => nameof(WaitForObjectsPickedGroupMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (items == null || items.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            List<(ManagedObjectPickReactionItem item, IPickable pickable)> validItems = new List<(ManagedObjectPickReactionItem, IPickable)>();
            HashSet<string> completedIds = new HashSet<string>();
            int remainingCount = 0;
            bool completed = false;
            Dictionary<IPickable, Action> handlers = new Dictionary<IPickable, Action>();

            for (int i = 0; i < items.Length; i++)
            {
                ManagedObjectPickReactionItem item = items[i];
                if (item == null || string.IsNullOrEmpty(item.objectId))
                    continue;

                GameObject obj = context.GetObjectById(item.objectId);
                if (obj == null)
                    continue;

                if (!obj.activeInHierarchy)
                    obj.SetActive(true);

                IPickable pickable = obj.GetComponent<IPickable>();
                if (pickable == null)
                    continue;

                validItems.Add((item, pickable));
                remainingCount++;
                pickable.ResetPickState();
                if (enablePickingOnStart)
                    pickable.EnablePicking(true);
            }

            if (validItems.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Action finish = () =>
            {
                if (completed)
                    return;
                completed = true;
                foreach (KeyValuePair<IPickable, Action> pair in handlers)
                {
                    if (pair.Key != null && pair.Value != null)
                        pair.Key.OnPicked -= pair.Value;
                }

                if (context != null)
                {
                    float clampedDelay = Mathf.Max(0f, completionDelaySeconds);
                    Coroutine finishCoroutine = context.StartCoroutine(FinishAfterDelay(clampedDelay, onComplete));
                    AddRuntimeCleanup(() =>
                    {
                        if (finishCoroutine != null)
                            context.StopCoroutine(finishCoroutine);
                    });
                    return;
                }

                RunRuntimeCleanup();
                onComplete?.Invoke();
            };

            for (int i = 0; i < validItems.Count; i++)
            {
                var entry = validItems[i];
                Action handler = null;
                handler = () =>
                {
                    if (completed || completedIds.Contains(entry.item.objectId))
                        return;

                    completedIds.Add(entry.item.objectId);
                    entry.pickable.OnPicked -= handler;
                    handlers.Remove(entry.pickable);

                    ExecuteObjectPickItemReactions(context, entry.item, waitForVoiceOverCompletion, () =>
                    {
                        remainingCount--;
                        if (remainingCount <= 0)
                            finish();
                    });
                };

                handlers[entry.pickable] = handler;
                entry.pickable.OnPicked += handler;
            }

            AddRuntimeCleanup(() =>
            {
                foreach (KeyValuePair<IPickable, Action> pair in handlers)
                {
                    if (pair.Key != null && pair.Value != null)
                        pair.Key.OnPicked -= pair.Value;
                }
            });
        }

        private IEnumerator FinishAfterDelay(float delaySeconds, Action onComplete)
        {
            if (delaySeconds > 0f)
                yield return new WaitForSeconds(delaySeconds);
            else
                yield return null;

            RunRuntimeCleanup();
            onComplete?.Invoke();
        }

        public override void HandleStepExit(IMissionContext context)
        {
            RunRuntimeCleanup();
        }

        private static void ExecuteObjectPickItemReactions(IMissionContext context, ManagedObjectPickReactionItem item, bool waitForVoiceOverCompletion, Action onComplete)
        {
            List<MissionActionData> actions = new List<MissionActionData>();
            if (item != null && !string.IsNullOrEmpty(item.voiceOverKey))
            {
                actions.Add(new PlayVoiceOverMissionActionData
                {
                    voiceOverKey = item.voiceOverKey,
                    waitForCompletion = waitForVoiceOverCompletion
                });
            }

            if (item != null && item.onPickedActions != null)
            {
                for (int i = 0; i < item.onPickedActions.Length; i++)
                {
                    MissionActionData action = item.onPickedActions[i];
                    if (action != null)
                        actions.Add(action);
                }
            }

            ExecuteManagedActionSequence(context, actions, onComplete);
        }

        private static void ExecuteManagedActionSequence(IMissionContext context, IList<MissionActionData> actions, Action onComplete)
        {
            if (actions == null || actions.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            ExecuteManagedActionAtIndex(context, actions, 0, onComplete);
        }

        private static void ExecuteManagedActionAtIndex(IMissionContext context, IList<MissionActionData> actions, int index, Action onComplete)
        {
            if (actions == null || index >= actions.Count)
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
