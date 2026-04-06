using System.Collections;
using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Interface pour le contexte de mission - permet l'accès aux services
    /// Respecte le principe de Dependency Inversion (SOLID)
    /// </summary>
    public interface IMissionContext
    {
        MissionFlowConfigSO CurrentMission { get; }
        MissionStepConfigSO CurrentStep { get; }

        GameObject GetObjectById(string id);
        void TeleportPlayer(Vector3 position, Vector3 rotation);
        void PlayVoiceOver(AudioClip clip, System.Action onComplete = null);
        void PlayVoiceOverByKey(string voiceOverKey, System.Action onComplete = null);
        void StopVoiceOver();
        void InterruptAsyncOperations();
        void ReturnToMainMenu();
        void ShowMissionUI(string text, bool showCanvas, bool showTextPanel, bool showButtonsPanel, MissionUIButtonConfiguration[] buttonConfigurations, bool enableTypewriterEffect = true);
        void HideMissionUI(bool hideCanvas, bool hideTextPanel, bool hideButtonsPanel);
        bool BindMissionUIButton(MissionUIButtonType buttonType, System.Action callback, string userActionId = null);

        IVoiceRecognitionService VoiceRecognitionService { get; }

        bool AreStepOnEnterActionsComplete();

        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
    }
}
