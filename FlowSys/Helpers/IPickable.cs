using System;

namespace GAME.FlowSys
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
