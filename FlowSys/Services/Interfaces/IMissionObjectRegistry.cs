using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Interface pour le registre d'objets de mission
    /// Respecte Interface Segregation Principle (SOLID)
    /// </summary>
    public interface IMissionObjectRegistry
    {
        void RegisterObject(string id, GameObject obj);
        GameObject GetObject(string id);
        void Clear();
    }
}
