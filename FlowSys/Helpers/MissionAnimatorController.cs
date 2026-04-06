using UnityEngine;

namespace GAME.FlowSys
{
    public class MissionAnimatorController : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;

        [Header("Animation Parameters")]
        [SerializeField] private string talkBool = "Talk";
        [SerializeField] private string idleBool = "Idle";
        [SerializeField] private string rightArmUiBool = "Right Arm UI";
        [SerializeField] private string leftArmUiBool = "Left Arm UI";

        public Animator Animator => animator;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] No Animator found on '{gameObject.name}' or its children");
            }
        }

        public void PlayAnimation(string animationName)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] Cannot play animation '{animationName}': Animator is null");
                return;
            }

            SetAnimationState(animationName);
            Debug.Log($"[MissionAnimatorController] Playing animation bool state: {animationName}");
        }

        public void SetBool(string parameterName, bool value)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] Cannot set bool '{parameterName}': Animator is null");
                return;
            }

            animator.SetBool(parameterName, value);
            Debug.Log($"[MissionAnimatorController] Set bool '{parameterName}' to {value}");
        }

        public void SetFloat(string parameterName, float value)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] Cannot set float '{parameterName}': Animator is null");
                return;
            }

            animator.SetFloat(parameterName, value);
            Debug.Log($"[MissionAnimatorController] Set float '{parameterName}' to {value}");
        }

        public void SetTrigger(string parameterName)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] Cannot trigger '{parameterName}': Animator is null");
                return;
            }

            animator.SetTrigger(parameterName);
            Debug.Log($"[MissionAnimatorController] Triggered '{parameterName}'");
        }

        public void SetInteger(string parameterName, int value)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[MissionAnimatorController] Cannot set integer '{parameterName}': Animator is null");
                return;
            }

            animator.SetInteger(parameterName, value);
            Debug.Log($"[MissionAnimatorController] Set integer '{parameterName}' to {value}");
        }

        public void StartTalking()
        {
            SetAnimationState(talkBool);
        }

        public void StopTalking()
        {
            SetAnimationState(idleBool);
        }

        public void WaveArms()
        {
            SetAnimationState(rightArmUiBool);
        }

        private void SetAnimationState(string activeStateName)
        {
            if (animator == null)
                return;

            SetKnownStateBool(talkBool, activeStateName);
            SetKnownStateBool(idleBool, activeStateName);
            SetKnownStateBool(rightArmUiBool, activeStateName);
            SetKnownStateBool(leftArmUiBool, activeStateName);
        }

        private void SetKnownStateBool(string parameterName, string activeStateName)
        {
            if (string.IsNullOrWhiteSpace(parameterName) || !HasBoolParameter(parameterName))
                return;

            animator.SetBool(parameterName, string.Equals(parameterName, activeStateName, System.StringComparison.Ordinal));
        }

        private bool HasBoolParameter(string parameterName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(parameterName))
                return false;

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].type != AnimatorControllerParameterType.Bool)
                    continue;

                if (string.Equals(parameters[i].name, parameterName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
