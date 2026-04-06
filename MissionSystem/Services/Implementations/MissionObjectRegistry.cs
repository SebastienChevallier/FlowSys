using System.Collections.Generic;
using EPOOutline;
using UnityEngine;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Registre pour les objets de mission accessibles par ID
    /// </summary>
    public class MissionObjectRegistry : MonoBehaviour, IMissionObjectRegistry
    {
        public static MissionObjectRegistry Instance { get; private set; }

        private Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Debug.LogWarning("[MissionSystem] Multiple MissionObjectRegistry instances detected");

            EnsureOutlinersOnCameras();
        }

        private static void EnsureOutlinersOnCameras()
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                if (cam.GetComponent<Outliner>() == null)
                    cam.gameObject.AddComponent<Outliner>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        public void RegisterObject(string id, GameObject obj)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[MissionSystem] Cannot register object: id is empty");
                return;
            }
            
            if (obj == null)
            {
                Debug.LogWarning($"[MissionSystem] Cannot register object '{id}': object is null");
                return;
            }
            
            if (objects.ContainsKey(id))
            {
                Debug.LogWarning($"[MissionSystem] Object '{id}' already registered, overwriting");
            }
            
            objects[id] = obj;
            Debug.Log($"[MissionSystem] Registered object: {id}");
        }

        public void UnregisterObject(string id, GameObject obj)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (!objects.TryGetValue(id, out GameObject registeredObject))
                return;

            if (registeredObject != obj)
                return;

            objects.Remove(id);
        }
        
        public GameObject GetObject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[MissionSystem] Cannot get object: id is empty");
                return null;
            }
            
            if (objects.TryGetValue(id, out GameObject obj))
            {
                return obj;
            }

            MissionObjectRegistrar[] registrars = Resources.FindObjectsOfTypeAll<MissionObjectRegistrar>();
            foreach (MissionObjectRegistrar registrar in registrars)
            {
                if (registrar == null)
                    continue;

                if (!registrar.gameObject.scene.IsValid())
                    continue;

                if (!string.Equals(registrar.objectId, id, System.StringComparison.Ordinal))
                    continue;

                GameObject foundObject = registrar.gameObject;
                objects[id] = foundObject;
                Debug.Log($"[MissionSystem] Resolved object '{id}' via MissionObjectRegistrar fallback");
                return foundObject;
            }
            
            Debug.LogWarning($"[MissionSystem] Object '{id}' not found in registry");
            return null;
        }
        
        public void Clear()
        {
            objects.Clear();
            Debug.Log("[MissionSystem] Object registry cleared");
        }
    }
}
