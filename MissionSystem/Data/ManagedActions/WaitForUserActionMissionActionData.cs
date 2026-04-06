using System;
using UnityEngine;
using UnityEngine.Events;
using Evaveo.nsVorta.HighLevel;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Interaction")]
    [Serializable]
    public sealed class WaitForUserActionMissionActionData : MissionActionData
    {
        public string userActionId;

        [NonSerialized]
        private Action runtimeCleanupActions;

        public override string GetDisplayName()
        {
            return $"Wait UA '{userActionId}'";
        }

        public override string GetTypeName()
        {
            return nameof(WaitForUserActionMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            UserAction ua = UserActionsManager.GetUserAction(userActionId);
            if (ua == null)
            {
                Debug.LogError($"[MissionSystem] UserAction '{userActionId}' not found");
                onComplete?.Invoke();
                return;
            }

            if (ua.doneTimes.Count > 0)
            {
                Debug.Log($"[MissionSystem] UserAction '{userActionId}' already done");
                onComplete?.Invoke();
                return;
            }

            UnityAction<object> handler = null;
            handler = _ =>
            {
                ua.onFire.RemoveListener(handler);
                RunRuntimeCleanup();
                onComplete?.Invoke();
            };

            ua.onFire.AddListener(handler);
            AddRuntimeCleanup(() => ua.onFire.RemoveListener(handler));
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
