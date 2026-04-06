using System;
using UnityEngine;
using Evaveo.nsVorta.HighLevel;
using Evaveo.nsVorta.RUNTIME;
using VortaPickableObject = Evaveo.nsVorta.RUNTIME.PickableObject;
using VortaWrapperPickableObject = Evaveo.nsVorta.RUNTIME.Wrappers.PickableObject;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Implémentation de IPickable pour objets Grabable de Vorta
    /// </summary>
    public class PickableObject : MonoBehaviour, IPickable
    {
        public event Action OnPicked;
        
        private Grabable grabable;
        private VortaWrapperPickableObject vortaWrapperPickableObject;
        private VortaPickableObject vortaPickableObject;
        private bool pickingEnabled = false;
        private bool isLaserHovered = false;
        private bool hasBeenPicked = false;
        
        private void Awake()
        {
            grabable = GetComponent<Grabable>();
            vortaWrapperPickableObject = GetComponent<VortaWrapperPickableObject>();
            if (vortaWrapperPickableObject == null)
                vortaWrapperPickableObject = GetComponentInChildren<VortaWrapperPickableObject>(true);
            if (vortaWrapperPickableObject == null)
                vortaWrapperPickableObject = GetComponentInParent<VortaWrapperPickableObject>();

            vortaPickableObject = vortaWrapperPickableObject != null
                ? vortaWrapperPickableObject
                : GetComponent<VortaPickableObject>();

            if (vortaPickableObject == null)
                vortaPickableObject = GetComponentInChildren<VortaPickableObject>(true);
            if (vortaPickableObject == null)
                vortaPickableObject = GetComponentInParent<VortaPickableObject>();

            Debug.Log($"[PickableObject] Awake on '{gameObject.name}': grabable={grabable != null}, wrapper={vortaWrapperPickableObject != null}, vortaPickable={vortaPickableObject != null}");
        }
        
        private void OnEnable()
        {
            hasBeenPicked = false;

            if (grabable != null)
            {
                grabable.onGrab.AddListener(OnObjectGrabbed);
            }

            if (vortaPickableObject != null)
            {
                vortaPickableObject.onRolloverIn.AddListener(OnRolloverInHandler);
                vortaPickableObject.onRolloverOut.AddListener(OnRolloverOutHandler);
                Debug.Log($"[PickableObject] OnEnable '{gameObject.name}': Subscribed to onRolloverIn/Out events");
            }
            else
            {
                Debug.LogWarning($"[PickableObject] OnEnable '{gameObject.name}': vortaPickableObject is NULL - cannot subscribe to rollover events!");
            }
        }
        
        private void OnDisable()
        {
            if (grabable != null)
            {
                grabable.onGrab.RemoveListener(OnObjectGrabbed);
            }

            if (vortaPickableObject != null)
            {
                vortaPickableObject.onRolloverIn.RemoveListener(OnRolloverInHandler);
                vortaPickableObject.onRolloverOut.RemoveListener(OnRolloverOutHandler);
            }

            if (pickingEnabled)
            {
                ControllerManager.onTriggerClicked.RemoveListener(OnControllerTriggerClicked);
            }
            isLaserHovered = false;
        }
        
        public void ResetPickState()
        {
            hasBeenPicked = false;
            isLaserHovered = false;
        }

        public void EnablePicking(bool enable)
        {
            if (pickingEnabled == enable) return;

            bool wasEnabled = pickingEnabled;
            pickingEnabled = enable;
            hasBeenPicked = false;
            isLaserHovered = false;

            Debug.Log($"[PickableObject] EnablePicking({enable}) on '{gameObject.name}' (wasEnabled={wasEnabled})");

            if (enable)
            {
                ControllerManager.onTriggerClicked.AddListener(OnControllerTriggerClicked);
            }
            else
            {
                ControllerManager.onTriggerClicked.RemoveListener(OnControllerTriggerClicked);
            }

            if (enable && vortaPickableObject == null)
            {
                Debug.LogWarning($"[MissionSystem] EnablePicking on '{gameObject.name}': vortaPickableObject is null. Laser hover detection won't work — check that VortaWrapperPickableObject or VortaPickableObject is present on this GameObject or its children/parent.");
            }
            
            Debug.Log($"[MissionSystem] Picking {(enable ? "enabled" : "disabled")} for {gameObject.name} (wrapper: {(vortaWrapperPickableObject != null)}, runtime: {(vortaPickableObject != null)})");
        }

        private void OnRolloverInHandler(VortaPickableObject pickable)
        {
            Debug.Log($"[PickableObject] OnRolloverInHandler called on '{gameObject.name}' (pickingEnabled={pickingEnabled}, pickable={pickable?.name})");
            
            if (!IsMatchingPickable(pickable))
            {
                Debug.Log($"[PickableObject] OnRolloverInHandler '{gameObject.name}': pickable doesn't match, ignoring");
                return;
            }

            isLaserHovered = true;
            Debug.Log($"[PickableObject] OnRolloverInHandler '{gameObject.name}': isLaserHovered set to TRUE");
        }

        private void OnRolloverOutHandler(VortaPickableObject pickable)
        {
            Debug.Log($"[PickableObject] OnRolloverOutHandler called on '{gameObject.name}' (pickable={pickable?.name})");
            
            if (!IsMatchingPickable(pickable))
            {
                Debug.Log($"[PickableObject] OnRolloverOutHandler '{gameObject.name}': pickable doesn't match, ignoring");
                return;
            }

            isLaserHovered = false;
            Debug.Log($"[PickableObject] OnRolloverOutHandler '{gameObject.name}': isLaserHovered set to FALSE");
        }

        private void OnControllerTriggerClicked(ControllerItem controller, eControllerType controllerType, eControllerEvent controllerEvent, VortaClickedEventArgs eventArgs)
        {
            Debug.Log($"[PickableObject] OnControllerTriggerClicked on '{gameObject.name}': pickingEnabled={pickingEnabled}, hasBeenPicked={hasBeenPicked}, isLaserHovered={isLaserHovered}");
            
            if (!pickingEnabled)
            {
                Debug.Log($"[PickableObject] OnControllerTriggerClicked '{gameObject.name}': picking NOT enabled, ignoring");
                return;
            }

            if (hasBeenPicked)
            {
                Debug.Log($"[PickableObject] OnControllerTriggerClicked '{gameObject.name}': already picked, ignoring");
                return;
            }

            if (!isLaserHovered)
            {
                Debug.Log($"[PickableObject] OnControllerTriggerClicked '{gameObject.name}': NOT laser hovered, ignoring");
                return;
            }

            Debug.Log($"[MissionSystem] ✓ Laser-picked object: {gameObject.name}");
            NotifyPicked();
        }
        
        private void OnObjectGrabbed(Grabable grabbedObject, eControllerType controllerType)
        {
            // DÉSACTIVÉ: Le grabbing ne doit PAS déclencher le picking pour les missions
            // Seul le laser trigger doit fonctionner
            Debug.Log($"[PickableObject] OnObjectGrabbed '{gameObject.name}': Grab detected but IGNORED (only laser picking allowed)");
            return;
            
            /*
            if (!pickingEnabled)
                return;

            if (hasBeenPicked)
                return;

            if (grabbedObject != null && grabable != null && grabbedObject != grabable)
                return;

            Debug.Log($"[MissionSystem] Object picked: {gameObject.name}");
            NotifyPicked();
            */
        }

        private bool IsMatchingPickable(VortaPickableObject pickable)
        {
            if (pickable == null)
                return false;

            if (pickable == vortaPickableObject)
                return true;

            Transform incomingTransform = pickable.transform;
            Transform currentTransform = transform;

            return incomingTransform == currentTransform
                || incomingTransform.IsChildOf(currentTransform)
                || currentTransform.IsChildOf(incomingTransform);
        }

        private void NotifyPicked()
        {
            if (hasBeenPicked)
            {
                return;
            }

            hasBeenPicked = true;
            OnPicked?.Invoke();
        }
    }
}
