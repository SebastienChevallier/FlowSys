using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace GAME.FlowSys
{
    public class MultipleChoiceDialog : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject phonePanel;
        public GameObject choicesPanel;
        public TextMeshProUGUI questionText;
        public Button choice1Button;
        public Button choice2Button;
        public TextMeshProUGUI choice1Text;
        public TextMeshProUGUI choice2Text;
        
        private Action<int> onChoiceSelected;
        private int correctChoiceIndex;
        private string correctSoundId;
        private string wrongSoundId;
        
        private void Awake()
        {
            if (choice1Button != null)
                choice1Button.onClick.AddListener(() => OnChoiceClicked(0));
            if (choice2Button != null)
                choice2Button.onClick.AddListener(() => OnChoiceClicked(1));
                
            Hide();
        }
        
        public void Show(string questionLocKey, string[] choiceLocKeys, int correctIndex, string correctSnd, string wrongSnd, Action<int> callback)
        {
            correctChoiceIndex = correctIndex;
            correctSoundId = correctSnd;
            wrongSoundId = wrongSnd;
            onChoiceSelected = callback;

            Transform current = transform;
            while (current != null)
            {
                current.gameObject.SetActive(true);

                CanvasGroup parentCanvasGroup = current.GetComponent<CanvasGroup>();
                if (parentCanvasGroup != null)
                {
                    parentCanvasGroup.alpha = 1f;
                    parentCanvasGroup.interactable = true;
                    parentCanvasGroup.blocksRaycasts = true;
                }

                current = current.parent;
            }
            
            if (questionText != null)
                SetLocalizedText(questionText, questionLocKey);
            
            if (choiceLocKeys.Length >= 1 && choice1Text != null)
                SetLocalizedText(choice1Text, choiceLocKeys[0]);
            if (choiceLocKeys.Length >= 2 && choice2Text != null)
                SetLocalizedText(choice2Text, choiceLocKeys[1]);

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (phonePanel != null)
                phonePanel.SetActive(true);

            if (choicesPanel != null)
                choicesPanel.SetActive(true);

            if (choice1Button != null)
                choice1Button.gameObject.SetActive(true);

            if (choice2Button != null)
                choice2Button.gameObject.SetActive(true);

            if (questionText != null)
                questionText.gameObject.SetActive(true);

            if (choice1Text != null)
                choice1Text.gameObject.SetActive(true);

            if (choice2Text != null)
                choice2Text.gameObject.SetActive(true);

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            Canvas.ForceUpdateCanvases();

            string phonePanelState = phonePanel != null ? phonePanel.activeInHierarchy.ToString() : "NULL";
            string choicesPanelState = choicesPanel != null ? choicesPanel.activeInHierarchy.ToString() : "NULL";
            Debug.Log($"[FlowSys] MultipleChoiceDialog.Show: dialogActive={gameObject.activeInHierarchy}, phonePanel={phonePanelState}, choicesPanel={choicesPanelState}");
        }
        
        private void SetLocalizedText(TextMeshProUGUI textComponent, string localizationKey)
        {
            if (textComponent == null || string.IsNullOrEmpty(localizationKey))
                return;

            string tableName = "PLS_Strings";
            string entryKey = localizationKey;
            
            if (localizationKey.Contains("/"))
            {
                var parts = localizationKey.Split('/');
                if (parts.Length == 2)
                {
                    tableName = parts[0];
                    entryKey = parts[1];
                }
            }

            var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, entryKey);
            textComponent.text = string.IsNullOrEmpty(localizedString) ? localizationKey : localizedString;
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void OnChoiceClicked(int choiceIndex)
        {
            bool isCorrect = choiceIndex == correctChoiceIndex;
            
            if (isCorrect)
            {
                Hide();
                onChoiceSelected?.Invoke(choiceIndex);
            }
        }
    }
}
