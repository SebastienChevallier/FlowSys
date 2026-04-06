using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Interface pour la lecture audio de mission
    /// Respecte Interface Segregation Principle (SOLID)
    /// </summary>
    public interface IMissionAudioPlayer
    {
        void Play(AudioClip clip, Action onComplete = null);
        void PlayByKey(string voiceOverKey, Action onComplete = null);
        void Stop();
    }
}
