using UnityEngine;

namespace GAME.FlowSys
{
    /// <summary>
    /// Composant à ajouter sur les objets de scène pour les enregistrer dans le registre par ID.
    /// </summary>
    public class MissionObjectRegistrar : MonoBehaviour
    {
        [Header("Object ID")]
        [Tooltip("ID unique pour cet objet (utilisé dans les actions/conditions)")]
        public string objectId;

        [Header("Registration")]
        public bool hideAfterRegistration;

        private bool hasAppliedHideAfterRegistration;

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
            MissionObjectRegistry registry = MissionObjectRegistry.Instance;
            if (registry != null)
                registry.UnregisterObject(objectId, gameObject);
        }

        private void TryRegister()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                Debug.LogWarning($"[FlowSys] MissionObjectRegistrar on '{gameObject.name}' has no objectId");
                return;
            }

            MissionObjectRegistry registry = MissionObjectRegistry.Instance;
            if (registry != null)
            {
                registry.RegisterObject(objectId, gameObject);
                ApplyHideAfterRegistrationIfNeeded();
            }
            else
            {
                Debug.LogWarning($"[FlowSys] No MissionObjectRegistry found in scene");
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(objectId))
                objectId = gameObject.name;
        }

        private void Reset()
        {
            if (string.IsNullOrEmpty(objectId))
                objectId = gameObject.name;
        }

        private void ApplyHideAfterRegistrationIfNeeded()
        {
            if (!hideAfterRegistration)
            {
                RestoreAfterHideRegistrationIfNeeded();
                hasAppliedHideAfterRegistration = false;
                return;
            }

            if (hasAppliedHideAfterRegistration)
                return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = false;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = false;
            }

            hasAppliedHideAfterRegistration = true;
        }

        public void ApplyRuntimeHiddenState()
        {
            gameObject.SetActive(true);
            ApplyHideAfterRegistrationIfNeeded();
        }

        public void RestoreRuntimeVisibleState()
        {
            gameObject.SetActive(true);
            RestoreAfterHideRegistrationIfNeeded();
            hasAppliedHideAfterRegistration = false;
        }

        private void RestoreAfterHideRegistrationIfNeeded()
        {
            if (!hasAppliedHideAfterRegistration)
                return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = true;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = true;
            }
        }
    }
}
