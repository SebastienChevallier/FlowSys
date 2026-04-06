using UnityEngine;

namespace GAME.FlowSys
{
    public class MissionTrainerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private MissionAnimatorController missionAnimatorController;

        [Header("Animation")]
        [SerializeField] private string idleTrigger = "Idle";

        [Header("Visibility")]
        [SerializeField] private bool disableCollidersWhenHidden = true;

        private Renderer[] cachedRenderers;
        private Collider[] cachedColliders;
        private LODGroup lodGroup;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);

            if (missionAnimatorController == null)
                missionAnimatorController = GetComponent<MissionAnimatorController>() ?? GetComponentInChildren<MissionAnimatorController>(true);

            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            cachedColliders = GetComponentsInChildren<Collider>(true);
            lodGroup = GetComponent<LODGroup>();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public void SetVisibility(TrainerVisibilityMode mode)
        {
            if (mode == TrainerVisibilityMode.NoChange)
                return;

            bool visible = mode == TrainerVisibilityMode.Show;

            if (lodGroup != null)
                lodGroup.enabled = visible;

            if (cachedRenderers != null)
            {
                for (int i = 0; i < cachedRenderers.Length; i++)
                {
                    if (cachedRenderers[i] != null)
                        cachedRenderers[i].enabled = visible;
                }
            }

            if (disableCollidersWhenHidden && cachedColliders != null)
            {
                for (int i = 0; i < cachedColliders.Length; i++)
                {
                    if (cachedColliders[i] != null)
                        cachedColliders[i].enabled = visible;
                }
            }
        }

        public void PlayAnimation(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                return;

            if (missionAnimatorController != null)
            {
                missionAnimatorController.PlayAnimation(triggerName);
                return;
            }

            if (animator != null)
                animator.SetTrigger(triggerName);
        }

        public void PlayIdle()
        {
            PlayAnimation(idleTrigger);
        }

        public void StartTalking()
        {
            if (missionAnimatorController != null)
            {
                missionAnimatorController.StartTalking();
                return;
            }

            PlayAnimation(idleTrigger);
        }

        public void StopTalking()
        {
            if (missionAnimatorController != null)
                missionAnimatorController.StopTalking();
        }
    }
}
