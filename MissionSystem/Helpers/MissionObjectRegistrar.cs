using EPOOutline;
using UnityEngine;

using Evaveo.nsVorta.HighLevel;
using VortaWrapperPickableObject = Evaveo.nsVorta.RUNTIME.Wrappers.PickableObject;
using VortaSelectableVisualEffect = Evaveo.nsVorta.RUNTIME.Wrappers.SelectableVisualEffect;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Composant à ajouter sur les objets de scène pour les enregistrer dans le registre
    /// </summary>
    public class MissionObjectRegistrar : MonoBehaviour
    {
        [Header("Object ID")]
        [Tooltip("ID unique pour cet objet (utilisé dans les actions/conditions)")]
        public string objectId;

        [Header("Auto Setup")]
        public bool isPickable;
        public bool isGrabbable;
        public bool isSnappable;
        public bool isOutlinable;

        [Header("Registration")]
        public bool hideAfterRegistration;

        [SerializeField, HideInInspector] private bool createdMissionPickable;
        [SerializeField, HideInInspector] private bool createdVortaPickable;
        [SerializeField, HideInInspector] private bool createdSelectableVisualEffect;
        [SerializeField, HideInInspector] private bool createdGrabable;
        [SerializeField, HideInInspector] private bool createdSnapable;
        [SerializeField, HideInInspector] private bool createdOutlinable;
        [SerializeField, HideInInspector] private bool createdCollider;

        private bool hasAppliedHideAfterRegistration;

        private void Awake()
        {
            if (isOutlinable)
                SyncOutlineComponents();
        }

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
            {
                registry.UnregisterObject(objectId, gameObject);
            }
        }

        private void TryRegister()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                Debug.LogWarning($"[MissionSystem] MissionObjectRegistrar on '{gameObject.name}' has no objectId");
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
                Debug.LogWarning($"[MissionSystem] No MissionObjectRegistry found in scene");
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = gameObject.name;
            }

            SyncComponentSetup();
        }

        private void Reset()
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = gameObject.name;
            }

            SyncComponentSetup();
        }

        private void SyncComponentSetup()
        {
            if (isGrabbable)
            {
                isSnappable = true;
            }

            SyncPickableComponents();
            SyncGrabableComponents();
            SyncSnapableComponents();
            SyncOutlineComponents();
            ApplyHideAfterRegistrationIfNeeded();
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

            PickableObject missionPickable = GetComponent<PickableObject>();
            if (missionPickable != null)
                missionPickable.EnablePicking(false);

            Grabable grabable = GetComponent<Grabable>();
            if (grabable != null)
                grabable.enabled = false;

            Snapable snapable = GetComponent<Snapable>();
            if (snapable != null)
                snapable.enabled = false;

            Outlinable outlinable = GetComponent<Outlinable>();
            if (outlinable != null)
                outlinable.enabled = false;

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

            PickableObject missionPickable = GetComponent<PickableObject>();
            if (missionPickable != null && isPickable)
                missionPickable.EnablePicking(true);

            Grabable grabable = GetComponent<Grabable>();
            if (grabable != null && isGrabbable)
                grabable.enabled = true;

            Snapable snapable = GetComponent<Snapable>();
            if (snapable != null && (isSnappable || isGrabbable))
                snapable.enabled = true;
        }

        private void SyncPickableComponents()
        {
            if (isPickable)
            {
                EnsureComponent(ref createdMissionPickable, GetComponent<PickableObject>());
                EnsureComponent(ref createdVortaPickable, GetComponent<VortaWrapperPickableObject>());
                EnsureComponent(ref createdSelectableVisualEffect, GetComponent<VortaSelectableVisualEffect>());
                EnsureCollider();
                return;
            }

            RemoveManagedComponent(ref createdMissionPickable, GetComponent<PickableObject>());
            RemoveManagedComponent(ref createdVortaPickable, GetComponent<VortaWrapperPickableObject>());
            RemoveManagedComponent(ref createdSelectableVisualEffect, GetComponent<VortaSelectableVisualEffect>());
            RemoveManagedCollider();
        }

        private void SyncGrabableComponents()
        {
            if (isGrabbable)
            {
                EnsureComponent(ref createdGrabable, GetComponent<Grabable>());
                return;
            }

            RemoveManagedComponent(ref createdGrabable, GetComponent<Grabable>());
        }

        private void SyncSnapableComponents()
        {
            if (isSnappable || isGrabbable)
            {
                EnsureComponent(ref createdSnapable, GetComponent<Snapable>());
                return;
            }

            if (GetComponent<Grabable>() != null)
            {
                isSnappable = true;
                return;
            }

            RemoveManagedComponent(ref createdSnapable, GetComponent<Snapable>());
        }

        private void SyncOutlineComponents()
        {
            if (isOutlinable)
            {
                EnsureComponent(ref createdOutlinable, GetComponent<Outlinable>());
                Outlinable outlinable = GetComponent<Outlinable>();
                if (outlinable != null)
                {
                    outlinable.AddAllChildRenderersToRenderingList(
                        RenderersAddingMode.MeshRenderer |
                        RenderersAddingMode.SkinnedMeshRenderer |
                        RenderersAddingMode.SpriteRenderer);
                    outlinable.enabled = false;
                }
                return;
            }

            RemoveManagedComponent(ref createdOutlinable, GetComponent<Outlinable>());
        }

        private void EnsureComponent<T>(ref bool createdFlag, T component) where T : Component
        {
            if (component != null)
            {
                return;
            }

            gameObject.AddComponent<T>();
            createdFlag = true;
        }

        private void EnsureCollider()
        {
            Collider existingCollider = GetComponent<Collider>();
            if (existingCollider != null)
            {
                return;
            }

            gameObject.AddComponent<BoxCollider>();
            createdCollider = true;
        }

        private void RemoveManagedComponent<T>(ref bool createdFlag, T component) where T : Component
        {
            if (!createdFlag)
            {
                return;
            }

            if (component == null)
            {
                createdFlag = false;
                return;
            }

            DestroyImmediate(component);
            createdFlag = false;
        }

        private void RemoveManagedCollider()
        {
            if (!createdCollider)
            {
                return;
            }

            Collider collider = GetComponent<Collider>();
            if (collider == null)
            {
                createdCollider = false;
                return;
            }

            DestroyImmediate(collider);
            createdCollider = false;
        }
    }
}
