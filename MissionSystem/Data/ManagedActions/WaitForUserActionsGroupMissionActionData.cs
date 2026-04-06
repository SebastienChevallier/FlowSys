using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Evaveo.nsVorta.HighLevel;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Interaction")]
    [Serializable]
    public sealed class WaitForUserActionsGroupMissionActionData : MissionActionData
    {
        public bool autoEnableUserActions = true;
        public bool waitForVoiceOverCompletion;
        public ManagedUserActionVoiceOverItem[] items = Array.Empty<ManagedUserActionVoiceOverItem>();

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName() => $"Wait UAs Group ({(items != null ? items.Length : 0)})";
        public override string GetTypeName() => nameof(WaitForUserActionsGroupMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (items == null || items.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            List<ManagedUserActionVoiceOverItem> validItems = new List<ManagedUserActionVoiceOverItem>();
            HashSet<string> completedIds = new HashSet<string>();
            int remainingCount = 0;
            bool completed = false;
            Dictionary<UserAction, UnityAction<object>> handlers = new Dictionary<UserAction, UnityAction<object>>();

            for (int i = 0; i < items.Length; i++)
            {
                ManagedUserActionVoiceOverItem item = items[i];
                if (item == null || string.IsNullOrEmpty(item.userActionId))
                    continue;

                UserAction ua = UserActionsManager.GetUserAction(item.userActionId);
                if (ua == null)
                    continue;

                validItems.Add(item);
                if (autoEnableUserActions)
                    UserActionsManager.SetActionCanDo(item.userActionId, true);

                if (ua.doneTimes.Count > 0)
                    completedIds.Add(item.userActionId);
                else
                    remainingCount++;
            }

            if (validItems.Count == 0 || remainingCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Action finish = () =>
            {
                if (completed)
                    return;
                completed = true;
                foreach (KeyValuePair<UserAction, UnityAction<object>> pair in handlers)
                {
                    if (pair.Key != null && pair.Value != null)
                        pair.Key.onFire.RemoveListener(pair.Value);
                }
                RunRuntimeCleanup();
                onComplete?.Invoke();
            };

            for (int i = 0; i < validItems.Count; i++)
            {
                ManagedUserActionVoiceOverItem item = validItems[i];
                if (completedIds.Contains(item.userActionId))
                    continue;

                UserAction ua = UserActionsManager.GetUserAction(item.userActionId);
                if (ua == null)
                    continue;

                UnityAction<object> handler = null;
                handler = _ =>
                {
                    if (completed || completedIds.Contains(item.userActionId))
                        return;

                    completedIds.Add(item.userActionId);
                    ua.onFire.RemoveListener(handler);
                    handlers.Remove(ua);

                    Action afterVoiceOver = () =>
                    {
                        remainingCount--;
                        if (remainingCount <= 0)
                            finish();
                    };

                    if (!string.IsNullOrEmpty(item.voiceOverKey))
                    {
                        if (waitForVoiceOverCompletion)
                            context.PlayVoiceOverByKey(item.voiceOverKey, afterVoiceOver);
                        else
                        {
                            context.PlayVoiceOverByKey(item.voiceOverKey);
                            afterVoiceOver();
                        }
                    }
                    else
                    {
                        afterVoiceOver();
                    }
                };

                handlers[ua] = handler;
                ua.onFire.AddListener(handler);
            }

            AddRuntimeCleanup(() =>
            {
                foreach (KeyValuePair<UserAction, UnityAction<object>> pair in handlers)
                {
                    if (pair.Key != null && pair.Value != null)
                        pair.Key.onFire.RemoveListener(pair.Value);
                }
            });
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
            callbacks?.Invoke();
        }
    }
}
