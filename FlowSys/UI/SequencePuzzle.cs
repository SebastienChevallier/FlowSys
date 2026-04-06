using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace GAME.FlowSys
{
    public class SequencePuzzle : MonoBehaviour
    {
        [Header("UI References")]
        public Transform vignetteContainer;
        public GameObject vignettePrefab;
        public TextMeshProUGUI feedbackText;
        public TextMeshProUGUI attemptsText;
        
        [Header("Settings")]
        public float vignetteSpacing = 0.15f;
        
        private List<SequenceVignette> vignettes = new List<SequenceVignette>();
        private List<int> playerSequence = new List<int>();
        private int[] correctSequence;
        private int currentAttempt = 0;
        private int maxAttempts = 2;
        private Action<bool> onPuzzleComplete;
        
        public void Setup(string[] vignetteIds, Sprite[] vignetteSprites, int[] correctSeq, int maxAttempts, Action<bool> callback)
        {
            this.correctSequence = correctSeq;
            this.maxAttempts = maxAttempts;
            this.onPuzzleComplete = callback;
            this.currentAttempt = 0;
            
            ClearVignettes();
            CreateVignettes(vignetteIds, vignetteSprites);
            UpdateAttemptsDisplay();
            
            gameObject.SetActive(true);
        }
        
        private void ClearVignettes()
        {
            foreach (var vignette in vignettes)
            {
                if (vignette != null)
                    Destroy(vignette.gameObject);
            }
            vignettes.Clear();
            playerSequence.Clear();
        }
        
        private void CreateVignettes(string[] vignetteIds, Sprite[] vignetteSprites)
        {
            for (int i = 0; i < vignetteIds.Length; i++)
            {
                GameObject vignetteObj = Instantiate(vignettePrefab, vignetteContainer);
                
                SequenceVignette vignette = vignetteObj.GetComponent<SequenceVignette>();
                if (vignette != null)
                {
                    Sprite vignetteSprite = vignetteSprites != null && i < vignetteSprites.Length ? vignetteSprites[i] : null;
                    vignette.Setup(vignetteIds[i], i, vignetteSprite, OnVignetteClicked);
                }
                
                vignettes.Add(vignette);
            }
        }
        
        private void OnVignetteClicked(SequenceVignette vignette)
        {
            if (vignette == null) return;
            
            playerSequence.Add(vignette.orderIndex);
            vignette.SetNumber(playerSequence.Count);
            
            if (playerSequence.Count == correctSequence.Length)
            {
                CheckSequence();
            }
        }
        
        private void CheckSequence()
        {
            bool isCorrect = true;
            for (int i = 0; i < correctSequence.Length; i++)
            {
                if (playerSequence[i] != correctSequence[i])
                {
                    isCorrect = false;
                    break;
                }
            }
            
            if (isCorrect)
            {
                if (feedbackText != null)
                    feedbackText.text = GetLocalizedString("SequencePuzzle_Correct", "Correct !");
                onPuzzleComplete?.Invoke(true);
                Invoke(nameof(Hide), 1f);
            }
            else
            {
                currentAttempt++;
                playerSequence.Clear();
                
                if (currentAttempt >= maxAttempts)
                {
                    if (feedbackText != null)
                        feedbackText.text = GetLocalizedString("SequencePuzzle_Failed", "Échec - Tentatives épuisées");
                    onPuzzleComplete?.Invoke(false);
                    Invoke(nameof(Hide), 2f);
                }
                else
                {
                    if (feedbackText != null)
                    {
                        string template = GetLocalizedString("SequencePuzzle_Retry", "Incorrect - Réessayez ({0} tentative(s) restante(s))");
                        feedbackText.text = string.Format(template, maxAttempts - currentAttempt);
                    }
                    UpdateAttemptsDisplay();
                }
            }
        }
        
        private string GetLocalizedString(string key, string fallback)
        {
            var localized = LocalizationSettings.StringDatabase.GetLocalizedString("UI", key);
            return string.IsNullOrEmpty(localized) ? fallback : localized;
        }
        
        private void UpdateAttemptsDisplay()
        {
            if (attemptsText != null)
            {
                string template = GetLocalizedString("SequencePuzzle_Attempts", "Tentatives: {0}/{1}");
                attemptsText.text = string.Format(template, currentAttempt, maxAttempts);
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
