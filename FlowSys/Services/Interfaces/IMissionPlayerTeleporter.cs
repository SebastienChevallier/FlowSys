using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Interface pour la téléportation du joueur
    /// Respecte Interface Segregation Principle (SOLID)
    /// </summary>
    public interface IMissionPlayerTeleporter
    {
        void Teleport(Vector3 position, Vector3 rotation);
    }
}
