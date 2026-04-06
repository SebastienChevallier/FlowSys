using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Scene")]
    [Serializable]
    public sealed class DelayMissionActionData : MissionActionData
    {
        public float delaySeconds;

        public override string GetDisplayName()
        {
            return $"Delay {delaySeconds}s";
        }

        public override string GetTypeName()
        {
            return nameof(DelayMissionActionData);
        }

        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            if (context == null)
            {
                onComplete?.Invoke();
                return;
            }

            context.StartCoroutine(DelayCoroutine(onComplete));
        }

        private System.Collections.IEnumerator DelayCoroutine(Action onComplete)
        {
            yield return new WaitForSeconds(delaySeconds);
            onComplete?.Invoke();
        }
    }
}
