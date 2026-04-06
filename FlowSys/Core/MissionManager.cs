using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Singleton qui orchestre tout le système de missions
    /// Respecte Single Responsibility Principle (SOLID)
    /// </summary>
    public class MissionManager : MissionSystemSingleton<MissionManager>, IMissionContext
    {
        public static MissionManager Instance => instance;

        [Header("Services (Dependency Injection)")]
        [SerializeField] private MissionSceneLoader sceneLoader;
        [SerializeField] private VortaPlayerTeleporter playerTeleporter;
        [SerializeField] private MissionObjectRegistry objectRegistry;
        [SerializeField] private MissionTextUIManager textUIManager;

        private AndroidVoiceRecognitionService _voiceRecognitionService;
        public IVoiceRecognitionService VoiceRecognitionService
        {
            get
            {
                if (_voiceRecognitionService == null)
                    _voiceRecognitionService = GetComponentInChildren<AndroidVoiceRecognitionService>(true)
                        ?? gameObject.AddComponent<AndroidVoiceRecognitionService>();
                return _voiceRecognitionService;
            }
        }

        private MissionStateMachine stateMachine;
        private MissionFlowConfigSO currentMissionConfig;
        private MissionStepConfigSO currentStepConfig;
        private MissionStepState currentStepState;
        private int currentStepIndex = -1;
        private readonly HashSet<MissionStepConfigSO> visitedSteps = new HashSet<MissionStepConfigSO>();
        private System.Action pendingMissionReadyCallback;
        private bool isWaitingForMissionStart;
        private bool isReturningToMainMenu;

        public MissionFlowConfigSO CurrentMission => currentMissionConfig;
        public MissionStepConfigSO CurrentStep => currentStepConfig;

        /// <summary>
        /// Callback invoked when the mission system finishes cleanup and is ready to return to main menu.
        /// Assign this from your game's flow manager instead of using GameFlowManager directly.
        /// </summary>
        public System.Action OnReturnToMainMenuRequested;

        protected override void Awake()
        {
            base.Awake();
            if (sceneLoader == null)
                sceneLoader = FindObjectOfType<MissionSceneLoader>();
            if (playerTeleporter == null)
                playerTeleporter = FindObjectOfType<VortaPlayerTeleporter>();
            if (objectRegistry == null)
                objectRegistry = FindObjectOfType<MissionObjectRegistry>();
            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();
            stateMachine = new MissionStateMachine(this);
        }

        private void LateUpdate()
        {
            stateMachine?.Update();
        }

        /// <summary>
        /// Démarre une mission
        /// </summary>
        public void StartMission(MissionFlowConfigSO missionConfig, System.Action onScenesLoadedAndReady = null)
        {
            if (missionConfig == null)
            {
                Debug.LogError("[FlowSys] Cannot start mission: config is null");
                return;
            }

            if (missionConfig.steps == null || missionConfig.steps.Count == 0)
            {
                Debug.LogError("[FlowSys] Cannot start mission: no steps defined");
                return;
            }

            Debug.Log($"[FlowSys] Starting mission: {missionConfig.missionName}");
            Debug.Log($"[FlowSys] Mission config id: {missionConfig.missionId}, step count: {missionConfig.steps.Count}");

            currentMissionConfig = missionConfig;
            currentStepConfig = null;
            currentStepIndex = -1;
            visitedSteps.Clear();
            pendingMissionReadyCallback = onScenesLoadedAndReady;
            isWaitingForMissionStart = onScenesLoadedAndReady != null;

            sceneLoader.LoadMissionScenes(
                missionConfig.sceneArt,
                missionConfig.sceneInteraction,
                onComplete: OnScenesLoaded);
        }

        private void OnScenesLoaded()
        {
            Debug.Log("[FlowSys] Scenes loaded");

            playerTeleporter.Teleport(
                currentMissionConfig.playerSpawnPosition,
                currentMissionConfig.playerSpawnRotation);

            if (isWaitingForMissionStart)
            {
                Debug.Log("[FlowSys] Mission is ready. Waiting for external flow before starting first step");
                pendingMissionReadyCallback?.Invoke();
                return;
            }

            Debug.Log("[FlowSys] Starting first mission step");
            StartFirstStep();
        }

        public void BeginCurrentMission()
        {
            if (currentMissionConfig == null)
            {
                Debug.LogError("[FlowSys] Cannot begin mission: no current mission is loaded");
                return;
            }

            if (!isWaitingForMissionStart)
            {
                Debug.LogWarning("[FlowSys] BeginCurrentMission called, but mission is not waiting to start");
                return;
            }

            isWaitingForMissionStart = false;
            pendingMissionReadyCallback = null;
            Debug.Log("[FlowSys] Starting first mission step");
            StartFirstStep();
        }

        private void StartFirstStep()
        {
            if (currentMissionConfig == null || currentMissionConfig.steps == null || currentMissionConfig.steps.Count == 0)
            {
                OnMissionComplete();
                return;
            }

            StartStep(currentMissionConfig.steps[0], 0);
        }

        private void StartStep(MissionStepConfigSO stepConfig, int stepIndex)
        {
            currentStepConfig = stepConfig;
            currentStepIndex = stepIndex;

            Debug.Log($"[FlowSys] Advancing to step index: {currentStepIndex}");

            if (currentStepConfig == null)
            {
                Debug.LogError($"[FlowSys] Step at index {currentStepIndex} is null");
                OnMissionComplete();
                return;
            }

            if (!visitedSteps.Add(currentStepConfig))
            {
                Debug.LogWarning($"[FlowSys] Re-entering step '{currentStepConfig.stepName}'. This may be expected for looped flows.");
            }

            Debug.Log($"[FlowSys] Current step: {currentStepConfig.stepName} (Id: {currentStepConfig.stepId})");
            Debug.Log($"[FlowSys] Step summary - OnEnter: {GetStepActionCount(currentStepConfig, MissionStepActionPhase.OnEnter)}, OnExit: {GetStepActionCount(currentStepConfig, MissionStepActionPhase.OnExit)}, Conditions: {GetStepConditionCount(currentStepConfig)}, Transitions: {currentStepConfig.transitions?.Count ?? 0}");

            currentStepState = new MissionStepState(currentStepConfig);
            currentStepState.OnStepComplete += OnStepComplete;

            stateMachine.ChangeState(currentStepState);
        }

        private void OnStepComplete(string transitionId)
        {
            Debug.Log($"[FlowSys] Step completed: {currentStepConfig.stepName}");

            MissionStepConfigSO nextStep = ResolveNextStep(currentStepConfig, transitionId);
            if (nextStep == null)
            {
                Debug.Log($"[FlowSys] No next transition configured for step '{currentStepConfig.stepName}'. Completing mission.");
                OnMissionComplete();
                return;
            }

            int nextIndex = currentMissionConfig != null && currentMissionConfig.steps != null
                ? currentMissionConfig.steps.IndexOf(nextStep)
                : -1;

            if (nextIndex < 0)
            {
                Debug.LogError($"[FlowSys] Next step '{nextStep.stepName}' is not present in mission.steps. Completing mission to avoid invalid flow.");
                OnMissionComplete();
                return;
            }

            StartStep(nextStep, nextIndex);
        }

        private MissionStepConfigSO ResolveNextStep(MissionStepConfigSO step, string transitionId)
        {
            if (step == null || step.transitions == null || step.transitions.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(transitionId))
            {
                for (int i = 0; i < step.transitions.Count; i++)
                {
                    MissionStepTransition transition = step.transitions[i];
                    if (transition != null && transition.targetStep != null && transition.transitionId == transitionId)
                        return transition.targetStep;
                }
            }

            foreach (var transition in step.transitions)
            {
                if (transition != null && transition.targetStep != null)
                    return transition.targetStep;
            }

            return null;
        }

        private int GetStepActionCount(MissionStepConfigSO step, MissionStepActionPhase phase)
        {
            return step != null ? step.GetStructuredActionCount(phase) : 0;
        }

        private int GetStepConditionCount(MissionStepConfigSO step)
        {
            return step != null ? step.GetStructuredConditionCount() : 0;
        }

        private void OnMissionComplete()
        {
            string missionName = currentMissionConfig != null ? currentMissionConfig.missionName : "<none>";
            Debug.Log($"[FlowSys] Mission completed: {missionName}");
            ReturnToMainMenu();
        }

        public void JumpToStep(MissionStepConfigSO stepConfig)
        {
            if (stepConfig == null || currentMissionConfig == null)
                return;

            int index = currentMissionConfig.steps.IndexOf(stepConfig);
            if (index < 0)
            {
                Debug.LogWarning($"[FlowSys] JumpToStep: step '{stepConfig.stepName}' not found in current mission.");
                return;
            }

            Debug.Log($"[FlowSys] JumpToStep: jumping to '{stepConfig.stepName}'");
            InterruptAsyncOperations();
            visitedSteps.Clear();
            stateMachine.ClearStateWithoutExit();
            StartStep(stepConfig, index);
        }

        public GameObject GetObjectById(string id)
        {
            if (objectRegistry == null)
                objectRegistry = FindObjectOfType<MissionObjectRegistry>();

            if (objectRegistry == null)
            {
                Debug.LogWarning("[FlowSys] MissionObjectRegistry not found in scene");
                return null;
            }

            return objectRegistry.GetObject(id);
        }

        public void TeleportPlayer(Vector3 position, Vector3 rotation)
        {
            playerTeleporter.Teleport(position, rotation);
        }

        public void PlayVoiceOver(AudioClip clip, System.Action onComplete = null)
        {
            MissionAudioPlayer missionAudioPlayer = FindObjectOfType<MissionAudioPlayer>();
            if (missionAudioPlayer != null && clip != null)
            {
                missionAudioPlayer.Play(clip, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void PlayVoiceOverByKey(string voiceOverKey, System.Action onComplete = null)
        {
            Debug.Log($"[FlowSys] PlayVoiceOverByKey: '{voiceOverKey}'");
            MissionAudioPlayer missionAudioPlayer = FindObjectOfType<MissionAudioPlayer>();
            if (missionAudioPlayer != null)
            {
                missionAudioPlayer.PlayByKey(voiceOverKey, onComplete);
            }
            else
            {
                Debug.LogWarning("[FlowSys] No MissionAudioPlayer found. Voice over skipped.");
                onComplete?.Invoke();
            }
        }

        public void StopVoiceOver()
        {
            MissionAudioPlayer missionAudioPlayer = FindObjectOfType<MissionAudioPlayer>();
            missionAudioPlayer?.Stop();
        }

        public void InterruptAsyncOperations()
        {
            Debug.Log($"[FlowSys] Interrupting async operations");
            MissionAudioPlayer missionAudioPlayer = FindObjectOfType<MissionAudioPlayer>();
            missionAudioPlayer?.Stop();
        }

        public void ReturnToMainMenu()
        {
            if (isReturningToMainMenu)
            {
                Debug.Log("[FlowSys] ReturnToMainMenu already in progress");
                return;
            }

            isReturningToMainMenu = true;
            Debug.Log("[FlowSys] Returning to main menu with mission cleanup");

            InterruptAsyncOperations();

            pendingMissionReadyCallback = null;
            isWaitingForMissionStart = false;

            if (stateMachine != null && stateMachine.CurrentState != null)
                stateMachine.ChangeState(null);

            StopAllCoroutines();

            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();
            textUIManager?.ResetForMissionCleanup();

            if (sceneLoader == null)
                sceneLoader = FindObjectOfType<MissionSceneLoader>();

            if (sceneLoader != null)
            {
                sceneLoader.UnloadMissionScenes();
                sceneLoader.ResetAfterMissionCleanup();
            }

            if (objectRegistry == null)
                objectRegistry = FindObjectOfType<MissionObjectRegistry>();
            objectRegistry?.Clear();

            MissionTransformReferenceRegistry.Current?.Clear();
            MissionTimerManager.instance?.ResetAllTimers();

            currentStepConfig = null;
            currentStepIndex = -1;
            currentMissionConfig = null;
            visitedSteps.Clear();

            OnReturnToMainMenuRequested?.Invoke();

            isReturningToMainMenu = false;
        }

        public void ShowMissionUI(string text, bool showCanvas, bool showTextPanel, bool showButtonsPanel, MissionUIButtonConfiguration[] buttonConfigurations)
        {
            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();

            if (textUIManager == null)
            {
                Debug.LogError("[FlowSys] MissionTextUIManager not found in scene.");
                return;
            }

            textUIManager.ShowUI(text, showCanvas, showTextPanel, showButtonsPanel, buttonConfigurations);
        }

        public void ShowMissionUI(string text, bool showCanvas, bool showTextPanel, bool showButtonsPanel, MissionUIButtonConfiguration[] buttonConfigurations, bool enableTypewriterEffect)
        {
            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();

            if (textUIManager == null)
            {
                Debug.LogError("[FlowSys] MissionTextUIManager not found in scene.");
                return;
            }

            textUIManager.ShowUI(text, showCanvas, showTextPanel, showButtonsPanel, buttonConfigurations, enableTypewriterEffect);
        }

        public void HideMissionUI(bool hideCanvas, bool hideTextPanel, bool hideButtonsPanel)
        {
            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();

            if (textUIManager == null)
            {
                Debug.LogWarning("[FlowSys] MissionTextUIManager not found in scene.");
                return;
            }

            textUIManager.HideUI(hideCanvas, hideTextPanel, hideButtonsPanel);
        }

        public bool BindMissionUIButton(MissionUIButtonType buttonType, System.Action callback, string userActionId = null)
        {
            if (textUIManager == null)
                textUIManager = FindObjectOfType<MissionTextUIManager>();

            if (textUIManager == null)
            {
                Debug.LogError("[FlowSys] MissionTextUIManager not found in scene.");
                return false;
            }

            return textUIManager.BindButton(buttonType, callback, userActionId);
        }

        public bool AreStepOnEnterActionsComplete()
        {
            return currentStepState?.AreOnEnterActionsComplete ?? true;
        }

        public new Coroutine StartCoroutine(IEnumerator routine)
        {
            return base.StartCoroutine(routine);
        }

        public new void StopCoroutine(Coroutine routine)
        {
            base.StopCoroutine(routine);
        }
    }
}
