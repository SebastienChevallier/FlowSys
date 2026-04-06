using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GAME.FlowSys
{
    /// <summary>
    /// Gère le chargement/déchargement additif des scènes de mission avec un fondu au noir.
    /// </summary>
    public class MissionSceneLoader : MonoBehaviour, IMissionSceneLoader
    {
        [Header("Loading UI (Optional)")]
        [SerializeField] private Slider loadingProgressSlider;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private GameObject missionImageLoading;

        [Header("Blackout Overlay")]
        [SerializeField] private Canvas blackoutCanvas;
        [SerializeField] private CanvasGroup blackoutCanvasGroup;
        [SerializeField] private Image blackoutImage;
        [SerializeField] private float fadeToBlackDuration = 0.35f;
        [SerializeField] private float fadeFromBlackDuration = 0.35f;

        private string currentSceneArt;
        private string currentSceneInteraction;

        private void Awake()
        {
            ResolveBlackoutReferences();
            ConfigureBlackoutOverlay();
        }

        private void ResolveBlackoutReferences()
        {
            if (blackoutCanvas == null)
            {
                Transform blackoutCanvasTransform = transform.Find("BlackoutCanvas");
                if (blackoutCanvasTransform != null)
                {
                    blackoutCanvas = blackoutCanvasTransform.GetComponent<Canvas>();
                    blackoutCanvasGroup = blackoutCanvasTransform.GetComponent<CanvasGroup>();

                    Transform blackoutImageTransform = blackoutCanvasTransform.Find("BlackoutImage");
                    if (blackoutImageTransform != null)
                        blackoutImage = blackoutImageTransform.GetComponent<Image>();
                }
            }
        }

        private void ConfigureBlackoutOverlay()
        {
            if (blackoutCanvas == null || blackoutCanvasGroup == null || blackoutImage == null)
                return;

            blackoutCanvasGroup.alpha = 0f;
            blackoutCanvasGroup.interactable = false;
            blackoutCanvasGroup.blocksRaycasts = false;
            blackoutImage.color = Color.black;
            blackoutImage.raycastTarget = false;
            blackoutCanvas.gameObject.SetActive(false);
        }

        private void SetBlackoutVisible(bool isVisible)
        {
            if (blackoutCanvas != null)
                blackoutCanvas.gameObject.SetActive(isVisible);
        }

        private void FadeBlackoutImmediate(float alpha)
        {
            if (blackoutCanvasGroup != null)
                blackoutCanvasGroup.alpha = alpha;
        }

        private IEnumerator FadeBlackoutCoroutine(float targetAlpha, float duration)
        {
            if (blackoutCanvasGroup == null)
                yield break;

            float startAlpha = blackoutCanvasGroup.alpha;
            float elapsedTime = 0f;

            if (duration <= 0f)
            {
                blackoutCanvasGroup.alpha = targetAlpha;
                yield break;
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                blackoutCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
                yield return null;
            }

            blackoutCanvasGroup.alpha = targetAlpha;
        }

        public void LoadMissionScenes(string sceneArt, string sceneInteraction, Action onComplete)
        {
            if (string.IsNullOrEmpty(sceneArt) || string.IsNullOrEmpty(sceneInteraction))
            {
                Debug.LogError("[FlowSys] Cannot load scenes: scene names are empty");
                onComplete?.Invoke();
                return;
            }

            int sceneArtIndex = SceneUtility.GetBuildIndexByScenePath(sceneArt);
            int sceneInteractionIndex = SceneUtility.GetBuildIndexByScenePath(sceneInteraction);

            if (sceneArtIndex < 0 || sceneInteractionIndex < 0)
            {
                Debug.LogError($"[FlowSys] Scenes not found in build settings: '{sceneArt}', '{sceneInteraction}'");
                onComplete?.Invoke();
                return;
            }

            currentSceneArt = sceneArt;
            currentSceneInteraction = sceneInteraction;

            Debug.Log($"[FlowSys] Loading scenes: {sceneArt} + {sceneInteraction}");
            SetBlackoutVisible(true);
            FadeBlackoutImmediate(0f);

            StartCoroutine(LoadScenesCoroutine(sceneArtIndex, sceneInteractionIndex, onComplete));
        }

        private IEnumerator LoadScenesCoroutine(int sceneArtIndex, int sceneInteractionIndex, Action onComplete)
        {
            yield return FadeBlackoutCoroutine(1f, fadeToBlackDuration);

            ShowLoadingUI(true);

            AsyncOperation loadArt = SceneManager.LoadSceneAsync(sceneArtIndex, LoadSceneMode.Additive);
            AsyncOperation loadInteraction = SceneManager.LoadSceneAsync(sceneInteractionIndex, LoadSceneMode.Additive);

            loadArt.allowSceneActivation = false;
            loadInteraction.allowSceneActivation = false;

            while (loadArt.progress < 0.9f || loadInteraction.progress < 0.9f)
            {
                float progress = (loadArt.progress + loadInteraction.progress) / 2f;
                UpdateLoadingProgress(progress);
                yield return null;
            }

            UpdateLoadingProgress(1f);
            loadArt.allowSceneActivation = true;
            loadInteraction.allowSceneActivation = true;

            while (!loadArt.isDone || !loadInteraction.isDone)
                yield return null;

            ShowLoadingUI(false);

            onComplete?.Invoke();

            yield return new WaitForEndOfFrame();
            yield return FadeBlackoutCoroutine(0f, fadeFromBlackDuration);
            SetBlackoutVisible(false);
        }

        private void ShowLoadingUI(bool show)
        {
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = show ? 1f : 0f;
                loadingCanvasGroup.interactable = show;
                loadingCanvasGroup.blocksRaycasts = show;
                loadingCanvasGroup.gameObject.SetActive(show);
            }

            if (loadingProgressSlider != null)
                loadingProgressSlider.gameObject.SetActive(show);

            if (missionImageLoading != null)
                missionImageLoading.SetActive(show);
        }

        private void UpdateLoadingProgress(float progress)
        {
            if (loadingProgressSlider != null)
                loadingProgressSlider.value = progress;
        }

        public void UnloadMissionScenes()
        {
            if (!string.IsNullOrEmpty(currentSceneArt))
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(currentSceneArt);
                if (sceneIndex >= 0)
                    SceneManager.UnloadSceneAsync(sceneIndex);
            }

            if (!string.IsNullOrEmpty(currentSceneInteraction))
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(currentSceneInteraction);
                if (sceneIndex >= 0)
                    SceneManager.UnloadSceneAsync(sceneIndex);
            }

            currentSceneArt = null;
            currentSceneInteraction = null;
        }

        public void ResetAfterMissionCleanup()
        {
            StopAllCoroutines();
            ShowLoadingUI(false);

            if (blackoutCanvasGroup != null)
            {
                blackoutCanvasGroup.alpha = 0f;
                blackoutCanvasGroup.interactable = false;
                blackoutCanvasGroup.blocksRaycasts = false;
            }

            SetBlackoutVisible(false);
            currentSceneArt = null;
            currentSceneInteraction = null;
        }

        public void FadeToBlack(float duration, Action onComplete = null)
        {
            if (blackoutCanvas == null || blackoutCanvasGroup == null)
            {
                Debug.LogWarning("[FlowSys] Cannot fade to black: blackout canvas not configured");
                onComplete?.Invoke();
                return;
            }

            SetBlackoutVisible(true);
            if (blackoutCanvasGroup.alpha < 0.01f)
                blackoutCanvasGroup.alpha = 0f;

            StartCoroutine(FadeBlackoutWithCallback(1f, duration, onComplete));
        }

        public void FadeFromBlack(float duration, Action onComplete = null)
        {
            if (blackoutCanvas == null || blackoutCanvasGroup == null)
            {
                Debug.LogWarning("[FlowSys] Cannot fade from black: blackout canvas not configured");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(FadeBlackoutWithCallback(0f, duration, () =>
            {
                SetBlackoutVisible(false);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator FadeBlackoutWithCallback(float targetAlpha, float duration, Action onComplete)
        {
            yield return FadeBlackoutCoroutine(targetAlpha, duration);
            onComplete?.Invoke();
        }
    }
}
