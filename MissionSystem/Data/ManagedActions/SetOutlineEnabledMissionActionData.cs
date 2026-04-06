using System;
using EPOOutline;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("Visual")]
    [Serializable]
    public sealed class SetOutlineEnabledMissionActionData : MissionActionData
    {
        public string objectId;
        public bool outlineEnabled = true;
        public bool disableOutlineOnStepExit;
        public Color outlineColor = Color.yellow;
        public float outlineWidth = 1f;

        public override string GetDisplayName()
        {
            return outlineEnabled && disableOutlineOnStepExit
                ? $"Set Outline On on '{objectId}' (Auto Off On Exit)"
                : $"Set Outline {(outlineEnabled ? "On" : "Off")} on '{objectId}'";
        }

        public override string GetTypeName() => nameof(SetOutlineEnabledMissionActionData);
        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            ApplyOutlineState(context, outlineEnabled);
            onComplete?.Invoke();
        }

        public override void HandleStepExit(IMissionContext context)
        {
            if (outlineEnabled && disableOutlineOnStepExit)
                ApplyOutlineState(context, false);
        }

        private void ApplyOutlineState(IMissionContext context, bool enabled)
        {
            GameObject obj = context?.GetObjectById(objectId);
            if (obj == null)
            {
                Debug.LogWarning($"[SetOutlineEnabled] Object '{objectId}' not found");
                return;
            }

            Outlinable outlinable = obj.GetComponent<Outlinable>();
            if (outlinable == null)
            {
                Debug.LogWarning($"[SetOutlineEnabled] No Outlinable on '{objectId}' — add isOutlinable on MissionObjectRegistrar");
                return;
            }

            if (enabled)
            {
                if (outlinable.OutlineTargets.Count == 0)
                    outlinable.AddAllChildRenderersToRenderingList(
                        RenderersAddingMode.MeshRenderer |
                        RenderersAddingMode.SkinnedMeshRenderer |
                        RenderersAddingMode.SpriteRenderer);

                outlinable.OutlineParameters.Color = outlineColor;
                outlinable.OutlineParameters.DilateShift = Mathf.Clamp(outlineWidth, 0.05f, 1f);
                outlinable.OutlineParameters.BlurShift = Mathf.Clamp(outlineWidth, 0.05f, 1f);
                outlinable.enabled = true;
                Debug.Log($"[SetOutlineEnabled] Enabled outline on '{objectId}' with {outlinable.OutlineTargets.Count} targets");
            }
            else
            {
                outlinable.enabled = false;
                Debug.Log($"[SetOutlineEnabled] Disabled outline on '{objectId}'");
            }
        }
    }
}
