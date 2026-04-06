using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Base class for singleton MonoBehaviours in the Mission System package.
    /// Replaces the Evaveo framework's Singleton<T> dependency.
    /// </summary>
    public abstract class MissionSystemSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        protected virtual void Awake()
        {
            if (instance != null && instance != this as T)
            {
                Debug.LogWarning($"[FlowSys] Duplicate singleton {typeof(T).Name} detected. Destroying extra instance.");
                Destroy(gameObject);
                return;
            }
            instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this as T)
                instance = null;
        }
    }
}
