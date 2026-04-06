using System.Collections.Generic;
using UnityEngine;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Composant pour référencer une position/rotation dans la scène
    /// Utilisé pour déplacer le formateur ou d'autres objets
    /// </summary>
    public class MissionTransformReference : MonoBehaviour
    {
        [Header("Reference ID")]
        [Tooltip("ID unique pour retrouver cette référence de transform")]
        public string referenceId;

        [Header("Gizmo Settings")]
        [SerializeField] private Color gizmoColor = Color.cyan;
        [SerializeField] private float gizmoSize = 0.5f;
        [SerializeField] private bool showGizmo = true;

        private void OnEnable()
        {
            TryRegister();
        }

        private void Start()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            MissionTransformReferenceRegistry registry = MissionTransformReferenceRegistry.Current;
            if (registry != null)
                registry.UnregisterReference(referenceId, this);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(referenceId))
            {
                referenceId = gameObject.name;
            }
        }

        private void TryRegister()
        {
            if (string.IsNullOrWhiteSpace(referenceId))
            {
                Debug.LogWarning($"[MissionSystem] MissionTransformReference on '{gameObject.name}' has no referenceId");
                return;
            }

            MissionTransformReferenceRegistry.Instance.RegisterReference(referenceId, this);
        }

        private void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoSize * 2f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoSize * 1.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoSize * 1.5f);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        public Vector3 GetEulerAngles()
        {
            return transform.eulerAngles;
        }
    }

    public class MissionTransformReferenceRegistry : MonoBehaviour
    {
        private static MissionTransformReferenceRegistry instance;

        public static MissionTransformReferenceRegistry Current => instance;

        public static MissionTransformReferenceRegistry Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MissionTransformReferenceRegistry>();
                    if (instance == null)
                    {
                        GameObject registryObject = new GameObject("MissionTransformReferenceRegistry");
                        instance = registryObject.AddComponent<MissionTransformReferenceRegistry>();
                    }
                }

                return instance;
            }
        }

        private readonly Dictionary<string, MissionTransformReference> references = new Dictionary<string, MissionTransformReference>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                return;
            }

            if (instance != this)
                Debug.LogWarning("[MissionSystem] Multiple MissionTransformReferenceRegistry instances detected");
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void RegisterReference(string id, MissionTransformReference reference)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[MissionSystem] Cannot register transform reference: id is empty");
                return;
            }

            if (reference == null)
            {
                Debug.LogWarning($"[MissionSystem] Cannot register transform reference '{id}': reference is null");
                return;
            }

            if (references.ContainsKey(id) && references[id] != reference)
                Debug.LogWarning($"[MissionSystem] Transform reference '{id}' already registered, overwriting");

            references[id] = reference;
        }

        public void UnregisterReference(string id, MissionTransformReference reference)
        {
            if (string.IsNullOrWhiteSpace(id) || reference == null)
                return;

            if (!references.TryGetValue(id, out MissionTransformReference registeredReference))
                return;

            if (registeredReference != reference)
                return;

            references.Remove(id);
        }

        public MissionTransformReference GetReference(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[MissionSystem] Cannot get transform reference: id is empty");
                return null;
            }

            if (references.TryGetValue(id, out MissionTransformReference reference) && reference != null)
                return reference;

            MissionTransformReference[] loadedReferences = Resources.FindObjectsOfTypeAll<MissionTransformReference>();
            foreach (MissionTransformReference loadedReference in loadedReferences)
            {
                if (loadedReference == null)
                    continue;

                if (!loadedReference.gameObject.scene.IsValid() || !loadedReference.gameObject.scene.isLoaded)
                    continue;

                if (loadedReference.hideFlags != HideFlags.None)
                    continue;

                if (!string.Equals(loadedReference.referenceId, id, System.StringComparison.Ordinal))
                    continue;

                references[id] = loadedReference;
                return loadedReference;
            }

            return null;
        }

        public void Clear()
        {
            references.Clear();
        }
    }
}
