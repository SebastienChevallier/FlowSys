using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Evaveo.EvaveoToolbox;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Implémentation du chargement de scènes utilisant ScenesManager de VORTA
    /// </summary>
    public class MissionSceneLoader : MonoBehaviour, IMissionSceneLoader
    {
        [Header("Loading UI (Optional - utilise l'UI par défaut de ScenesManager si vide)")]
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
                    {
                        blackoutImage = blackoutImageTransform.GetComponent<Image>();
                    }
                }
            }
        }

        private void ConfigureBlackoutOverlay()
        {
            if (blackoutCanvas == null || blackoutCanvasGroup == null || blackoutImage == null)
            {
                return;
            }            

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
            {
                blackoutCanvas.gameObject.SetActive(isVisible);
            }
        }

        private void FadeBlackoutImmediate(float alpha)
        {
            if (blackoutCanvasGroup == null)
            {
                return;
            }

            blackoutCanvasGroup.alpha = alpha;
        }

        private IEnumerator FadeBlackoutCoroutine(float targetAlpha, float duration)
        {
            if (blackoutCanvasGroup == null)
            {
                yield break;
            }

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

        private IEnumerator FadeToBlackBeforeSceneActivation()
        {
            yield return FadeBlackoutCoroutine(1f, fadeToBlackDuration);
        }

        private IEnumerator HideLoadingPanelAfterSceneActivation()
        {
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                loadingCanvasGroup.interactable = false;
                loadingCanvasGroup.blocksRaycasts = false;
                loadingCanvasGroup.gameObject.SetActive(false);
            }

            if (loadingProgressSlider != null)
            {
                loadingProgressSlider.gameObject.SetActive(false);
            }

            if (missionImageLoading != null)
            {
                missionImageLoading.SetActive(false);
            }

            yield break;
        }
        
        public void LoadMissionScenes(string sceneArt, string sceneInteraction, Action onComplete)
        {
            if (string.IsNullOrEmpty(sceneArt) || string.IsNullOrEmpty(sceneInteraction))
            {
                Debug.LogError("[MissionSystem] Cannot load scenes: scene names are empty");
                onComplete?.Invoke();
                return;
            }
            
            currentSceneArt = sceneArt;
            currentSceneInteraction = sceneInteraction;
            
            int sceneArtIndex = SceneUtility.GetBuildIndexByScenePath(sceneArt);
            int sceneInteractionIndex = SceneUtility.GetBuildIndexByScenePath(sceneInteraction);
            
            if (sceneArtIndex < 0 || sceneInteractionIndex < 0)
            {
                Debug.LogError($"[MissionSystem] Scenes not found in build settings: {sceneArt}, {sceneInteraction}");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[MissionSystem] Loading scenes: {sceneArt} + {sceneInteraction}");
            SetBlackoutVisible(true);
            FadeBlackoutImmediate(0f);
            
            ScenesManager.LoadTwoScenesAdditiveWithLoadingScreen(
                sceneArtIndex,
                sceneInteractionIndex,
                loadingProgressSlider,
                loadingCanvasGroup,
                missionImageLoading,
                beforeSceneActivation: FadeToBlackBeforeSceneActivation,
                afterSceneActivation: HideLoadingPanelAfterSceneActivation,
                callBackEndLoad: () =>
                {
                    StartCoroutine(FinishLoadingFlow(onComplete));
                });
        }

        private IEnumerator FinishLoadingFlow(Action onComplete)
        {
            onComplete?.Invoke();
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return FadeBlackoutCoroutine(0f, fadeFromBlackDuration);
            SetBlackoutVisible(false);
        }
        
        public void UnloadMissionScenes()
        {
            if (!string.IsNullOrEmpty(currentSceneArt))
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(currentSceneArt);
                if (sceneIndex >= 0)
                {
                    ScenesManager.UnLoadScene(sceneIndex);
                }
            }
            
            if (!string.IsNullOrEmpty(currentSceneInteraction))
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(currentSceneInteraction);
                if (sceneIndex >= 0)
                {
                    ScenesManager.UnLoadScene(sceneIndex);
                }
            }
            
            currentSceneArt = null;
            currentSceneInteraction = null;
        }

        public void ResetAfterMissionCleanup()
        {
            StopAllCoroutines();

            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                loadingCanvasGroup.interactable = false;
                loadingCanvasGroup.blocksRaycasts = false;
                loadingCanvasGroup.gameObject.SetActive(false);
            }

            if (loadingProgressSlider != null)
                loadingProgressSlider.gameObject.SetActive(false);

            if (missionImageLoading != null)
                missionImageLoading.SetActive(false);

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
                Debug.LogWarning("[MissionSystem] Cannot fade to black: blackout canvas not configured");
                onComplete?.Invoke();
                return;
            }

            SetBlackoutVisible(true);
            if (blackoutCanvasGroup.alpha < 0.01f)
            {
                blackoutCanvasGroup.alpha = 0f;
            }
            StartCoroutine(FadeBlackoutWithCallback(1f, duration, onComplete));
        }

        public void FadeFromBlack(float duration, Action onComplete = null)
        {
            if (blackoutCanvas == null || blackoutCanvasGroup == null)
            {
                Debug.LogWarning("[MissionSystem] Cannot fade from black: blackout canvas not configured");
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
