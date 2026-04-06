using System;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Interface pour le chargement de scènes de mission
    /// Respecte Interface Segregation Principle (SOLID)
    /// </summary>
    public interface IMissionSceneLoader
    {
        void LoadMissionScenes(string sceneArt, string sceneInteraction, Action onComplete);
        void UnloadMissionScenes();
    }
}
