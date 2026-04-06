using System.Collections.Generic;
using UnityEngine;
using Evaveo.EvaveoToolbox;

namespace GAME.MissionSystem
{
    public class MissionTimerManager : Singleton<MissionTimerManager>
    {
        private Dictionary<string, TimerData> activeTimers = new Dictionary<string, TimerData>();
        
        private class TimerData
        {
            public float startTime;
            public float duration;
            public bool isRunning;
            
            public float GetRemainingTime()
            {
                if (!isRunning) return 0f;
                float elapsed = Time.time - startTime;
                return Mathf.Max(0f, duration - elapsed);
            }
            
            public bool IsExpired()
            {
                return isRunning && GetRemainingTime() <= 0f;
            }
        }
        
        public void StartTimer(string timerId, float duration)
        {
            if (string.IsNullOrEmpty(timerId))
            {
                Debug.LogError("[MissionTimerManager] Cannot start timer with empty ID");
                return;
            }
            
            if (activeTimers.ContainsKey(timerId))
            {
                Debug.LogWarning($"[MissionTimerManager] Timer '{timerId}' already exists. Restarting with new duration.");
                activeTimers.Remove(timerId);
            }
            
            activeTimers[timerId] = new TimerData
            {
                startTime = Time.time,
                duration = duration,
                isRunning = true
            };
            
            Debug.Log($"[MissionTimerManager] Timer '{timerId}' started: {duration}s");
        }
        
        public void StopTimer(string timerId)
        {
            if (activeTimers.ContainsKey(timerId))
            {
                activeTimers[timerId].isRunning = false;
                Debug.Log($"[MissionTimerManager] Timer '{timerId}' stopped");
            }
        }
        
        public void ResetTimer(string timerId)
        {
            if (activeTimers.ContainsKey(timerId))
            {
                activeTimers.Remove(timerId);
                Debug.Log($"[MissionTimerManager] Timer '{timerId}' reset");
            }
        }
        
        public void ResetAllTimers()
        {
            activeTimers.Clear();
            Debug.Log("[MissionTimerManager] All timers reset");
        }
        
        public float GetRemainingTime(string timerId)
        {
            if (activeTimers.TryGetValue(timerId, out TimerData timer))
            {
                return timer.GetRemainingTime();
            }
            return 0f;
        }
        
        public bool IsTimerExpired(string timerId)
        {
            if (activeTimers.TryGetValue(timerId, out TimerData timer))
            {
                return timer.IsExpired();
            }
            return false;
        }
        
        public bool IsTimerRunning(string timerId)
        {
            if (activeTimers.TryGetValue(timerId, out TimerData timer))
            {
                return timer.isRunning && !timer.IsExpired();
            }
            return false;
        }
    }
}
