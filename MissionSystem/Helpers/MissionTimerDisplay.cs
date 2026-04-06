using UnityEngine;
using TMPro;

namespace GAME.MissionSystem
{
    public class MissionTimerDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject timerPanel;
        
        [Header("Timer Settings")]
        [SerializeField] private string timerId = "MainTimer";
        [SerializeField] private bool autoHideWhenExpired = true;
        
        [Header("Display Format")]
        [SerializeField] private string formatMinutesSeconds = "{0:00}:{1:00}";
        [SerializeField] private string formatSecondsOnly = "{0:0.0}s";
        [SerializeField] private float switchToSecondsThreshold = 60f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float warningThreshold = 30f;
        [SerializeField] private float criticalThreshold = 10f;
        
        private MissionTimerManager timerManager;
        private bool isVisible = false;
        
        private void Awake()
        {
            if (timerPanel != null)
                timerPanel.SetActive(false);
        }
        
        private void Update()
        {
            if (timerManager == null)
                timerManager = MissionTimerManager.instance;
            
            if (timerManager == null)
                return;
            
            if (!timerManager.IsTimerRunning(timerId))
            {
                if (autoHideWhenExpired && isVisible)
                    HideTimer();
                return;
            }
            
            if (!isVisible)
                ShowTimer();
            
            UpdateTimerDisplay();
        }
        
        private void UpdateTimerDisplay()
        {
            if (timerText == null)
                return;
            
            float remainingTime = timerManager.GetRemainingTime(timerId);
            
            if (remainingTime >= switchToSecondsThreshold)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = string.Format(formatMinutesSeconds, minutes, seconds);
            }
            else
            {
                timerText.text = string.Format(formatSecondsOnly, remainingTime);
            }
            
            if (remainingTime <= criticalThreshold)
            {
                timerText.color = criticalColor;
            }
            else if (remainingTime <= warningThreshold)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
        
        public void ShowTimer()
        {
            if (timerPanel != null)
                timerPanel.SetActive(true);
            isVisible = true;
        }
        
        public void HideTimer()
        {
            if (timerPanel != null)
                timerPanel.SetActive(false);
            isVisible = false;
        }
        
        public void SetTimerId(string newTimerId)
        {
            timerId = newTimerId;
        }
    }
}
