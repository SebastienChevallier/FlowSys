using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using Evaveo.nsVorta.HighLevel;

namespace GAME.MissionSystem
{
    public enum MissionUIButtonType
    {
        Suivant,
        Precedent,
        Ok,
        Quitter
    }

    [Serializable]
    public class MissionUIButtonConfiguration
    {
        public MissionUIButtonType buttonType;
        public bool visible = true;
        public string label;
        public string userActionId;
    }

    public class MissionTextUIManager : MonoBehaviour
    {
        private static MissionTextUIManager instance;
        public static MissionTextUIManager Instance
        {
            get
            {
                if (instance == null || !instance)
                    instance = ResolveBestInstance();

                return instance;
            }
        }

        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject panelText;
        [SerializeField] private GameObject panelButtons;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button buttonSuivant;
        [SerializeField] private Button buttonPrecedent;
        [SerializeField] private Button buttonOk;
        [SerializeField] private Button buttonQuitter;
        [SerializeField] private float canvasFadeDuration = 0.25f;
        [SerializeField] private float textCharactersPerSecond = 45f;
        [SerializeField] private float textPanelVerticalPadding = 40f;
        [SerializeField] private float minTextPanelHeight = 120f;
        
        [Header("PLS UI Components")]
        [SerializeField] private MultipleChoiceDialog multipleChoiceDialog;
        [SerializeField] private SequencePuzzle sequencePuzzle;
        [SerializeField] private VitalSignsIndicator vitalSignsIndicator;

        private readonly Dictionary<MissionUIButtonType, Button> buttons = new Dictionary<MissionUIButtonType, Button>();
        private readonly Dictionary<MissionUIButtonType, TextMeshProUGUI> buttonLabels = new Dictionary<MissionUIButtonType, TextMeshProUGUI>();
        private Coroutine canvasFadeCoroutine;
        private Coroutine textRevealCoroutine;
        private RectTransform panelTextRect;
        private RectTransform messageTextRect;

        private static MissionTextUIManager ResolveBestInstance()
        {
            MissionTextUIManager[] managers = FindObjectsOfType<MissionTextUIManager>(true);
            MissionTextUIManager fallback = null;

            for (int i = 0; i < managers.Length; i++)
            {
                MissionTextUIManager manager = managers[i];
                if (manager == null)
                    continue;

                if (fallback == null)
                    fallback = manager;

                if (!manager.gameObject.activeInHierarchy)
                    continue;

                if (manager.multipleChoiceDialog != null)
                    return manager;

                if (manager.targetCanvas != null)
                    return manager;
            }

            return fallback;
        }

        private void Awake()
        {
            instance = this;
            AutoAssignReferences();
            CacheButtons();
            ClearAllButtonBindings();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Reset()
        {
            AutoAssignReferences();
            CacheButtons();
        }

        public void ShowUI(string text, bool showCanvas, bool showTextPanel, bool showButtonsPanel, IList<MissionUIButtonConfiguration> buttonConfigurations, bool enableTypewriterEffect = true)
        {
            AutoAssignReferences();
            CacheButtons();

            if (targetCanvas == null)
            {
                Debug.LogError("[MissionSystem] MissionTextUIManager is not configured: Canvas not found.");
                return;
            }

            if (canvasFadeCoroutine != null)
            {
                StopCoroutine(canvasFadeCoroutine);
                canvasFadeCoroutine = null;
            }

            if (panelText != null)
                panelText.SetActive(showCanvas && showTextPanel);

            if (panelButtons != null)
                panelButtons.SetActive(showCanvas && showButtonsPanel);

            if (messageText != null)
            {
                UpdateTextPanelHeight(text ?? string.Empty);
                StartTextReveal(text ?? string.Empty, showCanvas && showTextPanel, enableTypewriterEffect);
            }

            if (showCanvas)
            {
                targetCanvas.gameObject.SetActive(true);
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    canvasFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 1f, canvasFadeDuration, false));
                }
            }
            else if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0f, canvasFadeDuration, true));
            }
            else
            {
                targetCanvas.gameObject.SetActive(false);
            }

            ApplyButtonConfigurations(buttonConfigurations, showCanvas && showButtonsPanel);
        }

        public void HideUI(bool hideCanvas, bool hideTextPanel, bool hideButtonsPanel)
        {
            if (hideTextPanel && panelText != null)
                panelText.SetActive(false);

            if (hideButtonsPanel && panelButtons != null)
                panelButtons.SetActive(false);

            ClearAllButtonBindings();

            if (hideTextPanel)
                StopTextReveal(clearText: true);

            if (hideCanvas && targetCanvas != null)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    if (canvasFadeCoroutine != null)
                    {
                        StopCoroutine(canvasFadeCoroutine);
                        canvasFadeCoroutine = null;
                    }

                    canvasFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0f, canvasFadeDuration, true));
                }
                else
                {
                    targetCanvas.gameObject.SetActive(false);
                }
            }
        }

        public bool BindButton(MissionUIButtonType buttonType, Action callback, string userActionId = null)
        {
            AutoAssignReferences();
            CacheButtons();

            if (!buttons.TryGetValue(buttonType, out Button button) || button == null)
            {
                Debug.LogError($"[MissionSystem] Cannot bind button '{buttonType}': button not found on MissionTextUIManager.");
                return false;
            }

            button.interactable = true;
            button.enabled = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(userActionId))
                    UserActionsManager.UserDoThis(userActionId, Time.time, buttonType);

                callback?.Invoke();
            });
            return true;
        }

        public void ClearAllButtonBindings()
        {
            CacheButtons();

            foreach (Button button in buttons.Values)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }
        }

        public bool HasButton(MissionUIButtonType buttonType)
        {
            AutoAssignReferences();
            CacheButtons();
            return buttons.TryGetValue(buttonType, out Button button) && button != null;
        }

        public void MoveTo(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);            
        }

        public void ResetForMissionCleanup()
        {
            AutoAssignReferences();
            CacheButtons();

            if (canvasFadeCoroutine != null)
            {
                StopCoroutine(canvasFadeCoroutine);
                canvasFadeCoroutine = null;
            }

            if (textRevealCoroutine != null)
            {
                StopCoroutine(textRevealCoroutine);
                textRevealCoroutine = null;
            }

            ClearAllButtonBindings();

            if (panelText != null)
                panelText.SetActive(false);

            if (panelButtons != null)
                panelButtons.SetActive(false);

            if (messageText != null)
            {
                messageText.text = string.Empty;
                messageText.maxVisibleCharacters = int.MaxValue;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (targetCanvas != null)
                targetCanvas.gameObject.SetActive(false);
        }

        private void ApplyButtonConfigurations(IList<MissionUIButtonConfiguration> buttonConfigurations, bool allowButtons)
        {
            foreach (KeyValuePair<MissionUIButtonType, Button> pair in buttons)
            {
                if (pair.Value != null)
                {
                    pair.Value.interactable = false;
                    pair.Value.gameObject.SetActive(false);
                }
            }

            ClearAllButtonBindings();

            if (!allowButtons || buttonConfigurations == null)
                return;

            foreach (MissionUIButtonConfiguration configuration in buttonConfigurations)
            {
                if (configuration == null)
                    continue;

                if (!buttons.TryGetValue(configuration.buttonType, out Button button) || button == null)
                    continue;

                button.gameObject.SetActive(configuration.visible);
                button.interactable = configuration.visible;
                button.enabled = true;

                if (buttonLabels.TryGetValue(configuration.buttonType, out TextMeshProUGUI label) && label != null && !string.IsNullOrEmpty(configuration.label))
                    label.text = ResolveLocalizedLabel(configuration.label);

                if (!string.IsNullOrEmpty(configuration.userActionId))
                {
                    button.onClick.AddListener(() => UserActionsManager.UserDoThis(configuration.userActionId, Time.time, configuration.buttonType));
                }
            }
        }

        private static string ResolveLocalizedLabel(string labelReference)
        {
            if (string.IsNullOrEmpty(labelReference))
                return string.Empty;

            string tableName = "PLS_Strings";
            string entryKey = labelReference;

            if (labelReference.Contains("/"))
            {
                string[] parts = labelReference.Split('/');
                if (parts.Length == 2)
                {
                    tableName = parts[0];
                    entryKey = parts[1];
                }
            }

            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, entryKey);
            return string.IsNullOrEmpty(localized) ? labelReference : localized;
        }

        private void AutoAssignReferences()
        {
            if (targetCanvas == null)
            {
                Transform canvasTransform = transform.Find("Canvas");
                if (canvasTransform != null)
                    targetCanvas = canvasTransform.GetComponent<Canvas>();
            }

            if (targetCanvas != null && canvasGroup == null)
                canvasGroup = targetCanvas.GetComponent<CanvasGroup>();

            if (panelText == null && targetCanvas != null)
            {
                Transform panelTransform = targetCanvas.transform.Find("Panel_Text");
                if (panelTransform != null)
                    panelText = panelTransform.gameObject;
            }

            if (panelButtons == null && targetCanvas != null)
            {
                Transform panelTransform = targetCanvas.transform.Find("Panel_Buttons");
                if (panelTransform != null)
                    panelButtons = panelTransform.gameObject;
            }

            if (messageText == null && panelText != null)
                messageText = panelText.GetComponentInChildren<TextMeshProUGUI>(true);

            if (panelTextRect == null && panelText != null)
                panelTextRect = panelText.GetComponent<RectTransform>();

            if (messageTextRect == null && messageText != null)
                messageTextRect = messageText.rectTransform;

            if (panelText != null)
                SetGraphicRaycastTarget(panelText.GetComponent<Graphic>(), false);

            SetGraphicRaycastTarget(messageText, false);

            buttonSuivant = FindButton(buttonSuivant, "Button_Suivant");
            buttonPrecedent = FindButton(buttonPrecedent, "Button_Precedent");
            buttonOk = FindButton(buttonOk, "Button_Ok");
            buttonQuitter = FindButton(buttonQuitter, "Button_Quitter");
        }

        private Button FindButton(Button current, string objectName)
        {
            if (current != null)
                return current;

            if (panelButtons == null)
                return null;

            Transform child = panelButtons.transform.Find(objectName);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private void CacheButtons()
        {
            buttons[MissionUIButtonType.Suivant] = buttonSuivant;
            buttons[MissionUIButtonType.Precedent] = buttonPrecedent;
            buttons[MissionUIButtonType.Ok] = buttonOk;
            buttons[MissionUIButtonType.Quitter] = buttonQuitter;

            CacheLabel(MissionUIButtonType.Suivant, buttonSuivant);
            CacheLabel(MissionUIButtonType.Precedent, buttonPrecedent);
            CacheLabel(MissionUIButtonType.Ok, buttonOk);
            CacheLabel(MissionUIButtonType.Quitter, buttonQuitter);
        }

        private void CacheLabel(MissionUIButtonType buttonType, Button button)
        {
            TextMeshProUGUI label = button != null ? button.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            buttonLabels[buttonType] = label;
            SetGraphicRaycastTarget(label, false);
        }

        private void SetGraphicRaycastTarget(Graphic graphic, bool value)
        {
            if (graphic != null)
                graphic.raycastTarget = value;
        }

        private void UpdateTextPanelHeight(string text)
        {
            if (panelTextRect == null || messageText == null || messageTextRect == null)
                return;

            float availableWidth = messageTextRect.rect.width;
            if (availableWidth <= 0f)
                availableWidth = messageTextRect.sizeDelta.x;

            if (availableWidth <= 0f)
                return;

            Vector2 preferredSize = messageText.GetPreferredValues(text, availableWidth, 0f);
            float targetHeight = Mathf.Max(minTextPanelHeight, preferredSize.y + textPanelVerticalPadding);
            panelTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }

        private void StartTextReveal(string text, bool animate, bool enableTypewriterEffect)
        {
            StopTextReveal(clearText: false);

            if (messageText == null)
                return;

            messageText.richText = true;
            messageText.text = text;

            if (!enableTypewriterEffect || !animate || string.IsNullOrEmpty(text) || textCharactersPerSecond <= 0f)
            {
                messageText.maxVisibleCharacters = int.MaxValue;
                return;
            }

            textRevealCoroutine = StartCoroutine(RevealTextCoroutine());
        }

        private void StopTextReveal(bool clearText)
        {
            if (textRevealCoroutine != null)
            {
                StopCoroutine(textRevealCoroutine);
                textRevealCoroutine = null;
            }

            if (messageText == null)
                return;

            messageText.maxVisibleCharacters = int.MaxValue;

            if (clearText)
                messageText.text = string.Empty;
        }

        private IEnumerator RevealTextCoroutine()
        {
            messageText.maxVisibleCharacters = 0;
            messageText.ForceMeshUpdate();

            int totalVisibleCharacters = messageText.textInfo.characterCount;
            if (totalVisibleCharacters <= 0)
            {
                messageText.maxVisibleCharacters = int.MaxValue;
                textRevealCoroutine = null;
                yield break;
            }

            float visibleCharacters = 0f;
            while (visibleCharacters < totalVisibleCharacters)
            {
                visibleCharacters += textCharactersPerSecond * Time.unscaledDeltaTime;
                messageText.maxVisibleCharacters = Mathf.Clamp(Mathf.FloorToInt(visibleCharacters), 0, totalVisibleCharacters);
                yield return null;
            }

            messageText.maxVisibleCharacters = int.MaxValue;
            textRevealCoroutine = null;
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration, bool deactivateOnComplete)
        {
            if (canvasGroup == null)
                yield break;

            if (duration <= 0f)
            {
                canvasGroup.alpha = to;
                if (deactivateOnComplete && targetCanvas != null)
                    targetCanvas.gameObject.SetActive(false);

                canvasFadeCoroutine = null;
                yield break;
            }

            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = to;

            if (deactivateOnComplete && targetCanvas != null)
                targetCanvas.gameObject.SetActive(false);

            canvasFadeCoroutine = null;
        }
        
        public void EnsureCanvasActive()
        {
            AutoAssignReferences();

            if (canvasFadeCoroutine != null)
            {
                StopCoroutine(canvasFadeCoroutine);
                canvasFadeCoroutine = null;
            }

            Canvas canvasToActivate = targetCanvas;

            // Fallback: find Canvas in children if targetCanvas still null
            if (canvasToActivate == null)
                canvasToActivate = GetComponentInChildren<Canvas>(true);

            if (canvasToActivate != null)
            {
                canvasToActivate.gameObject.SetActive(true);
                CanvasGroup cg = canvasGroup ?? canvasToActivate.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                Debug.Log($"[MissionSystem] EnsureCanvasActive: canvas '{canvasToActivate.gameObject.name}' activated");
            }
            else
            {
                Debug.LogWarning("[MissionSystem] EnsureCanvasActive: no Canvas found on UIManager — assign targetCanvas in the Inspector");
            }
        }

        public MultipleChoiceDialog GetMultipleChoiceDialog()
        {
            if (multipleChoiceDialog == null && targetCanvas != null)
            {
                Transform dialogTransform = targetCanvas.transform.Find("PLS_MultipleChoiceDialog");
                if (dialogTransform != null)
                    multipleChoiceDialog = dialogTransform.GetComponent<MultipleChoiceDialog>();
            }

            if (multipleChoiceDialog == null)
                multipleChoiceDialog = GetComponentInChildren<MultipleChoiceDialog>(true);

            if (multipleChoiceDialog == null)
                multipleChoiceDialog = FindObjectOfType<MultipleChoiceDialog>(true);

            if (multipleChoiceDialog != null)
            {
                Debug.Log($"[MissionSystem] GetMultipleChoiceDialog: using dialog '{multipleChoiceDialog.gameObject.name}' on manager '{gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[MissionSystem] GetMultipleChoiceDialog: not found. targetCanvas={(targetCanvas != null ? targetCanvas.gameObject.name : "NULL")}, manager={gameObject.name}");
            }

            return multipleChoiceDialog;
        }
        
        public SequencePuzzle GetSequencePuzzle()
        {
            if (sequencePuzzle == null && targetCanvas != null)
            {
                Transform puzzleTransform = targetCanvas.transform.Find("PLS_SequencePuzzle");
                if (puzzleTransform != null)
                    sequencePuzzle = puzzleTransform.GetComponent<SequencePuzzle>();
            }
            return sequencePuzzle;
        }
        
        public VitalSignsIndicator GetVitalSignsIndicator()
        {
            if (vitalSignsIndicator == null && targetCanvas != null)
            {
                Transform indicatorTransform = targetCanvas.transform.Find("PLS_VitalSignsIndicator");
                if (indicatorTransform != null)
                    vitalSignsIndicator = indicatorTransform.GetComponent<VitalSignsIndicator>();
            }
            return vitalSignsIndicator;
        }
    }
}
