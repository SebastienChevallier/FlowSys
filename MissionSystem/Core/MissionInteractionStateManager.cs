using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Evaveo.nsVorta.RUNTIME;
using Evaveo.nsVorta.HighLevel;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Manages interaction states (snap, grab, zone entry) for mission conditions.
    /// Listens to VORTA interaction events and tracks state per object ID.
    /// </summary>
    public sealed class MissionInteractionStateManager : MonoBehaviour
    {
        private static MissionInteractionStateManager _instance;

        private Dictionary<string, bool> _grabStates = new Dictionary<string, bool>();
        // Track which controllers are in each zone: zoneId -> (leftInZone, rightInZone, headInZone)
        private Dictionary<string, (bool left, bool right, bool head)> _zoneStates = new Dictionary<string, (bool, bool, bool)>();

        // Map objectId to Snapable/Grabable components
        private Dictionary<string, Snapable> _snapableMap = new Dictionary<string, Snapable>();
        private Dictionary<string, Grabable> _grabableMap = new Dictionary<string, Grabable>();
        private Dictionary<string, InteractiveZone> _zoneMap = new Dictionary<string, InteractiveZone>();
        private Dictionary<string, string> _snapableToSnapSystemId = new Dictionary<string, string>();
        // Zone colliders for head proximity polling
        private Dictionary<string, Collider[]> _zoneCollidersMap = new Dictionary<string, Collider[]>();
        private bool _headPollingRequired = false;

        private sealed class GrabSubscriptionCleanup
        {
            public Grabable Grabable;
            public UnityAction<Grabable, eControllerType> Handler;
        }

        public static MissionInteractionStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MissionInteractionStateManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("[MissionInteractionStateManager]");
                        _instance = obj.AddComponent<MissionInteractionStateManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }

        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDisable()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Start()
        {
            InitializeInteractionTracking();
        }

        private void InitializeInteractionTracking()
        {
            // Find all snapables and grabables with object IDs
            Snapable[] snapables = FindObjectsOfType<Snapable>();
            foreach (Snapable snapable in snapables)
            {
                RegisterSnapable(snapable);
            }

            Grabable[] grabables = FindObjectsOfType<Grabable>();
            foreach (Grabable grabable in grabables)
            {
                RegisterGrabbable(grabable);
            }

            InteractiveZone[] zones = FindObjectsOfType<InteractiveZone>();
            foreach (InteractiveZone zone in zones)
            {
                RegisterZone(zone);
            }
        }

        private void RegisterSnapable(Snapable snapable)
        {
            if (snapable == null)
                return;

            string objectId = GetObjectId(snapable.gameObject);
            if (string.IsNullOrEmpty(objectId))
                return;

            _snapableMap[objectId] = snapable;

            // Get the snap system ID from the Snapable's interactive zone
            if (snapable.interactiveZone != null)
            {
                // Try to infer snap system ID - this depends on VORTA's API
                _snapableToSnapSystemId[objectId] = "";
            }
        }

        private void RegisterGrabbable(Grabable grabable)
        {
            if (grabable == null)
                return;

            string objectId = GetObjectId(grabable.gameObject);
            if (string.IsNullOrEmpty(objectId))
                return;

            _grabableMap[objectId] = grabable;
            _grabStates[objectId] = false;

            // Subscribe to grab events (onGrab, onUngrab)
            grabable.onGrab.AddListener((g, hand) => OnGrabbed(objectId));
            grabable.onUngrab.AddListener((g, hand) => OnUngrabbed(objectId));
        }

        public bool TrySubscribeToLogicalGrab(string logicalObjectId, Action onGrabbed, out Action cleanup)
        {
            cleanup = null;

            if (string.IsNullOrEmpty(logicalObjectId))
                return false;

            Grabable grabable = ResolveLogicalGrabable(logicalObjectId);
            if (grabable == null)
            {
                Debug.LogWarning($"[MissionSystem] TrySubscribeToLogicalGrab: no Grabable resolved for logical id '{logicalObjectId}'.");
                return false;
            }

            EnsureGrabableInteractionEnabled(grabable);

            GrabSubscriptionCleanup state = new GrabSubscriptionCleanup();
            state.Grabable = grabable;

            UnityAction<Grabable, eControllerType> handler = null;
            handler = (grabbedObject, controllerType) =>
            {
                if (grabbedObject != null && grabbedObject != grabable)
                    return;

                grabable.onGrab.RemoveListener(handler);
                onGrabbed?.Invoke();
            };

            state.Handler = handler;
            grabable.onGrab.AddListener(handler);
            cleanup = () =>
            {
                if (state.Grabable != null && state.Handler != null)
                    state.Grabable.onGrab.RemoveListener(state.Handler);
            };

            Debug.Log($"[MissionSystem] Logical grab subscription ready: logicalId='{logicalObjectId}', runtimeGrabable='{grabable.gameObject.name}'.");
            return true;
        }

        public Grabable ResolveLogicalGrabable(string logicalObjectId)
        {
            if (string.IsNullOrEmpty(logicalObjectId))
                return null;

            string explicitGrabableId = $"Grabable_{logicalObjectId}";
            if (_grabableMap.TryGetValue(explicitGrabableId, out Grabable explicitGrabable) && explicitGrabable != null)
            {
                Debug.Log($"[MissionSystem] Resolved logical grab '{logicalObjectId}' to explicit grabable id '{explicitGrabableId}'.");
                return explicitGrabable;
            }

            if (_grabableMap.TryGetValue(logicalObjectId, out Grabable directGrabable) && directGrabable != null)
                return directGrabable;

            if (_zoneMap.TryGetValue(logicalObjectId, out InteractiveZone zone) && zone != null)
            {
                Grabable zoneGrabable = zone.GetComponentInParent<Grabable>(true);
                if (zoneGrabable != null)
                {
                    Debug.Log($"[MissionSystem] Resolved logical grab '{logicalObjectId}' via InteractiveZone parent '{zoneGrabable.gameObject.name}'.");
                    return zoneGrabable;
                }
            }

            return null;
        }

        private static void EnsureGrabableInteractionEnabled(Grabable grabable)
        {
            if (grabable == null)
                return;

            GameObject target = grabable.gameObject;
            if (!target.activeInHierarchy)
                target.SetActive(true);

            MissionObjectRegistrar registrar = target.GetComponent<MissionObjectRegistrar>();
            if (registrar != null && registrar.hideAfterRegistration)
                registrar.RestoreRuntimeVisibleState();

            grabable.enabled = true;

            Snapable snapable = target.GetComponent<Snapable>();
            if (snapable == null)
                snapable = target.GetComponentInChildren<Snapable>(true);
            if (snapable != null)
                snapable.enabled = true;
        }

        private void RegisterZone(InteractiveZone zone)
        {
            if (zone == null || string.IsNullOrEmpty(zone.zone))
                return;

            string zoneId = zone.zone;
            _zoneMap[zoneId] = zone;
            _zoneStates[zoneId] = (false, false, false);
            Debug.Log($"[MissionSystem] Registered InteractiveZone '{zoneId}' on '{zone.gameObject.name}'");

            Collider[] zoneColliders = zone.GetComponentsInChildren<Collider>();
            _zoneCollidersMap[zoneId] = zoneColliders;

            zone.onDisable.AddListener(_ =>
            {
                var s = _zoneStates[zoneId];
                _zoneStates[zoneId] = (false, false, s.head);
                Debug.Log($"[MissionSystem] Zone disabled: zone='{zoneId}', state reset (head preserved).");
            });

            // Subscribe to zone entry/exit events on the InteractiveZone component
            zone.onFingerEnter.AddListener((_, controllerItem, __, ___) =>
            {
                eControllerType controller = DetermineControllerType(controllerItem);
                OnZoneFingerEnter(zoneId, controller);
            });

            zone.onFingerExit.AddListener((_, controllerItem, __, ___) =>
            {
                eControllerType controller = DetermineControllerType(controllerItem);
                OnZoneFingerExit(zoneId, controller);
            });

            zone.onSpecifiedFingerEnter.AddListener((_, controllerItem, __) =>
            {
                eControllerType controller = DetermineControllerType(controllerItem);
                OnZoneFingerEnter(zoneId, controller);
            });

            zone.onSpecifiedFingerExit.AddListener((_, controllerItem, __) =>
            {
                eControllerType controller = DetermineControllerType(controllerItem);
                OnZoneFingerExit(zoneId, controller);
            });
        }

        private eControllerType DetermineControllerType(object controllerItem)
        {
            if (controllerItem == null)
                return eControllerType.Left;

            if (controllerItem is ControllerItem typedControllerItem)
                return typedControllerItem.mControllerType;

            if (controllerItem is Component component)
            {
                eControllerType? componentController = TryReadControllerType(component.GetType(), component);
                if (componentController.HasValue)
                    return componentController.Value;
            }

            eControllerType? reflectedController = TryReadControllerType(controllerItem.GetType(), controllerItem);
            if (reflectedController.HasValue)
                return reflectedController.Value;

            Debug.LogWarning($"[MissionSystem] Could not determine controller type from '{controllerItem.GetType().FullName}', defaulting to Left.");
            return eControllerType.Left;
        }

        private eControllerType? TryReadControllerType(Type sourceType, object source)
        {
            const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            System.Reflection.FieldInfo directField = sourceType.GetField("mControllerType", Flags)
                ?? sourceType.GetField("controllerType", Flags);
            if (directField != null && directField.FieldType == typeof(eControllerType))
                return (eControllerType)directField.GetValue(source);

            System.Reflection.PropertyInfo directProperty = sourceType.GetProperty("mControllerType", Flags)
                ?? sourceType.GetProperty("controllerType", Flags);
            if (directProperty != null && directProperty.PropertyType == typeof(eControllerType))
                return (eControllerType)directProperty.GetValue(source);

            System.Reflection.FieldInfo[] fields = sourceType.GetFields(Flags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                if (field.FieldType == typeof(eControllerType))
                    return (eControllerType)field.GetValue(source);
            }

            System.Reflection.PropertyInfo[] properties = sourceType.GetProperties(Flags);
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(eControllerType) && property.CanRead)
                    return (eControllerType)property.GetValue(source);
            }

            return null;
        }

        private void OnGrabbed(string objectId)
        {
            _grabStates[objectId] = true;
        }

        private void OnUngrabbed(string objectId)
        {
            _grabStates[objectId] = false;
        }

        private void OnZoneFingerEnter(string zoneId, eControllerType controller)
        {
            if (!_zoneStates.ContainsKey(zoneId))
                _zoneStates[zoneId] = (false, false, false);

            var state = _zoneStates[zoneId];
            switch (controller)
            {
                case eControllerType.Left:
                    state.left = true;
                    break;
                case eControllerType.Right:
                    state.right = true;
                    break;
            }
            _zoneStates[zoneId] = state;
            Debug.Log($"[MissionSystem] Zone enter detected: zone='{zoneId}', controller={controller}, state=({state.left},{state.right},{state.head})");
        }

        private void OnZoneFingerExit(string zoneId, eControllerType controller)
        {
            if (!_zoneStates.ContainsKey(zoneId))
                return;

            var state = _zoneStates[zoneId];
            switch (controller)
            {
                case eControllerType.Left:
                    state.left = false;
                    break;
                case eControllerType.Right:
                    state.right = false;
                    break;
            }
            _zoneStates[zoneId] = state;
            Debug.Log($"[MissionSystem] Zone exit detected: zone='{zoneId}', controller={controller}, state=({state.left},{state.right},{state.head})");
        }

        /// <summary>
        /// Notifie le manager du step courant pour activer/désactiver le polling tête selon les conditions requises.
        /// </summary>
        public void NotifyStepChanged(MissionStepConfigSO stepConfig)
        {
            _headPollingRequired = StepRequiresHeadZonePolling(stepConfig);
            if (_headPollingRequired)
                Debug.Log($"[MissionSystem] Head zone polling ENABLED for step '{stepConfig?.stepName}'.");
        }

        private bool StepRequiresHeadZonePolling(MissionStepConfigSO stepConfig)
        {
            if (stepConfig == null || stepConfig.conditions == null)
                return false;

            foreach (MissionStepConditionEntry entry in stepConfig.conditions)
            {
                if (entry?.managedCondition is InteractiveZoneEntryMissionConditionData zoneCondition
                    && (zoneCondition.allowedControllers & MissionControllerType.Head) != 0)
                    return true;
            }
            return false;
        }

        private void Update()
        {
            if (_headPollingRequired)
                PollHeadZoneStates();
        }

        private void PollHeadZoneStates()
        {
            if (_zoneCollidersMap.Count == 0)
                return;

            GameObject eyesObject = Vorta.GetUserEyesObject();
            if (eyesObject == null)
                return;

            Vector3 headPos = eyesObject.transform.position;

            foreach (var kvp in _zoneCollidersMap)
            {
                string zoneId = kvp.Key;
                Collider[] colliders = kvp.Value;

                bool headInZone = false;
                foreach (Collider col in colliders)
                {
                    if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                        continue;
                    if (col.bounds.Contains(headPos))
                    {
                        headInZone = true;
                        break;
                    }
                }

                if (!_zoneStates.TryGetValue(zoneId, out var state))
                    continue;

                if (state.head != headInZone)
                {
                    _zoneStates[zoneId] = (state.left, state.right, headInZone);
                    Debug.Log($"[MissionSystem] Head zone {(headInZone ? "enter" : "exit")}: zone='{zoneId}', state=({state.left},{state.right},{headInZone})");
                }
            }
        }

        /// <summary>
        /// Evaluates if a snapable object is currently snapped.
        /// </summary>
        public bool IsObjectSnapped(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
                return false;

            if (_snapableMap.TryGetValue(objectId, out Snapable snapable))
            {
                if (snapable == null)
                    return false;

                // Check both hands against the snap system
                ISnapSystem snapSystem = SnapSystem.GetSnapSystem("");
                if (snapSystem != null)
                {
                    bool leftSnapped = snapSystem.IsSnaped(eControllerType.Left, out _, out _, out UnityEngine.Object leftUserObject);
                    bool rightSnapped = snapSystem.IsSnaped(eControllerType.Right, out _, out _, out UnityEngine.Object rightUserObject);

                    // Check if the snapped object is our target snapable
                    if (leftSnapped && (leftUserObject as GameObject) == snapable.gameObject)
                        return true;
                    if (rightSnapped && (rightUserObject as GameObject) == snapable.gameObject)
                        return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Evaluates if a grabbable object is currently grabbed.
        /// </summary>
        public bool IsObjectGrabbed(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
                return false;

            string explicitGrabableId = $"Grabable_{objectId}";
            if (_grabStates.TryGetValue(explicitGrabableId, out bool explicitState))
                return explicitState;

            return _grabStates.TryGetValue(objectId, out bool state) && state;
        }

        /// <summary>
        /// Evaluates if a zone has been entered by allowed controllers.
        /// </summary>
        public bool IsZoneEntered(string objectId, MissionControllerType allowedControllers)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                Debug.LogWarning("[MissionSystem] IsZoneEntered called with empty zoneId.");
                return false;
            }

            if (!_zoneStates.TryGetValue(objectId, out var state))
            {
                Debug.LogWarning($"[MissionSystem] IsZoneEntered: zone '{objectId}' is not registered.");
                return false;
            }

            // Check if any of the allowed controllers are in the zone
            bool leftAllowed = (allowedControllers & MissionControllerType.Left) != 0;
            bool rightAllowed = (allowedControllers & MissionControllerType.Right) != 0;
            bool headAllowed = (allowedControllers & MissionControllerType.Head) != 0;

            bool result = (leftAllowed && state.left) || (rightAllowed && state.right) || (headAllowed && state.head);
            if (result)
            {
                Debug.Log($"[MissionSystem] Zone condition satisfied: zone='{objectId}', allowed={allowedControllers}, state=({state.left},{state.right},{state.head})");
            }

            return result;
        }

        private string GetObjectId(GameObject go)
        {
            if (go == null)
                return null;

            // Try to get the MissionObjectRegistrar component first
            MissionObjectRegistrar registrar = go.GetComponent<MissionObjectRegistrar>();
            if (registrar != null && !string.IsNullOrEmpty(registrar.objectId))
                return registrar.objectId;

            // Fallback to instance ID for objects without registrar
            return go.GetInstanceID().ToString();
        }
    }
}
