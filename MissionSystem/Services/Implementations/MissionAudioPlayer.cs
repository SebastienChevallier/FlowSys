using System;
using System.Collections;
using UnityEngine;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Implémentation du lecteur audio pour les voix off de mission
    /// </summary>
    public class MissionAudioPlayer : MonoBehaviour, IMissionAudioPlayer
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;
        
        private Coroutine currentPlayCoroutine;
        
        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        
        public void Play(AudioClip clip, Action onComplete = null)
        {
            if (clip == null)
            {
                Debug.LogWarning("[MissionSystem] Cannot play audio: clip is null");
                onComplete?.Invoke();
                return;
            }
            
            if (currentPlayCoroutine != null)
            {
                StopCoroutine(currentPlayCoroutine);
            }
            
            currentPlayCoroutine = StartCoroutine(PlayCoroutine(clip, onComplete));
        }
        
        public void PlayByKey(string voiceOverKey, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(voiceOverKey))
            {
                Debug.LogWarning("[MissionSystem] Cannot play voice over: key is null or empty");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[MissionSystem] Playing voice over by key: {voiceOverKey}");
            
            VoixOffManager.PlayVoixOffImmediate(voiceOverKey, 
                cbStartAudio: () => Debug.Log($"[MissionSystem] Voice over started: {voiceOverKey}"),
                cbEndAudio: () =>
                {
                    Debug.Log($"[MissionSystem] Voice over completed: {voiceOverKey}");
                    onComplete?.Invoke();
                });
        }
        
        private IEnumerator PlayCoroutine(AudioClip clip, Action onComplete)
        {
            Debug.Log($"[MissionSystem] Playing audio: {clip.name}");
            
            audioSource.clip = clip;
            audioSource.Play();
            
            if (onComplete != null)
            {
                yield return new WaitForSeconds(clip.length);
                Debug.Log($"[MissionSystem] Audio completed: {clip.name}");
                onComplete.Invoke();
            }
            
            currentPlayCoroutine = null;
        }
        
        public void Stop()
        {
            if (currentPlayCoroutine != null)
            {
                StopCoroutine(currentPlayCoroutine);
                currentPlayCoroutine = null;
            }
            
            audioSource.Stop();
        }
    }
}
