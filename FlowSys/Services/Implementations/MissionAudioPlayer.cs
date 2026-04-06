using System;
using System.Collections;
using UnityEngine;

namespace GAME.FlowSys
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
                Debug.LogWarning("[FlowSys] Cannot play audio: clip is null");
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
                Debug.LogWarning("[FlowSys] Cannot play voice over: key is null or empty");
                onComplete?.Invoke();
                return;
            }

            // Key-based audio lookup requires an external audio manager.
            // Override this method or assign an AudioClip via Play(clip, onComplete) instead.
            Debug.LogWarning($"[FlowSys] PlayByKey '{voiceOverKey}': no key-based audio system is connected. Override MissionAudioPlayer or use Play(AudioClip) directly.");
            onComplete?.Invoke();
        }
        
        private IEnumerator PlayCoroutine(AudioClip clip, Action onComplete)
        {
            Debug.Log($"[FlowSys] Playing audio: {clip.name}");
            
            audioSource.clip = clip;
            audioSource.Play();
            
            if (onComplete != null)
            {
                yield return new WaitForSeconds(clip.length);
                Debug.Log($"[FlowSys] Audio completed: {clip.name}");
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
