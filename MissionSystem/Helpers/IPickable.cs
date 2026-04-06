using System;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Interface pour les objets sélectionnables/pickables
    /// </summary>
    public interface IPickable
    {
        event Action OnPicked;
        void EnablePicking(bool enable);
        void ResetPickState();
    }
}
