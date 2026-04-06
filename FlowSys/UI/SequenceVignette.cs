using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GAME.FlowSys
{
    public class SequenceVignette : MonoBehaviour
    {
        [Header("UI References")]
        public Image backgroundImage;
        public Image iconImage;
        public TextMeshProUGUI numberText;
        public Button button;
        
        [Header("Visual Settings")]
        public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        public Color selectedColor = new Color(0.3f, 0.6f, 0.3f, 0.9f);
        public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        public Color incorrectColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        
        public string vignetteId { get; private set; }
        public int orderIndex { get; private set; }
        public bool isSelected { get; private set; }
        
        private System.Action<SequenceVignette> onVignetteClicked;
        
        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }
        
        public void Setup(string id, int order, Sprite icon, System.Action<SequenceVignette> clickCallback)
        {
            vignetteId = id;
            orderIndex = order;
            onVignetteClicked = clickCallback;
            
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }
            
            if (numberText != null)
            {
                numberText.text = "";
            }
            
            SetSelected(false);
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }
        }
        
        public void ShowAsCorrect()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = correctColor;
            }
        }
        
        public void ShowAsIncorrect()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = incorrectColor;
            }
        }
        
        public void SetNumber(int number)
        {
            if (numberText != null)
            {
                numberText.text = number.ToString();
            }
        }
        
        private void OnClicked()
        {
            onVignetteClicked?.Invoke(this);
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }
    }
}
