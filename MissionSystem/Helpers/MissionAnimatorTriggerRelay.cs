using UnityEngine;

namespace GAME.MissionSystem
{
    public class MissionAnimatorTriggerRelay : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void Trigger(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                Debug.LogWarning($"[MissionAnimatorTriggerRelay] Cannot trigger on '{gameObject.name}': parameter name is null or empty");
                return;
            }

            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorTriggerRelay] Cannot trigger '{parameterName}' on '{gameObject.name}': Animator is null");
                return;
            }

            animator.SetTrigger(parameterName);
            Debug.Log($"[MissionAnimatorTriggerRelay] Triggered '{parameterName}' on '{gameObject.name}'");
        }

        private void ResolveReferences()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }
    }
}
