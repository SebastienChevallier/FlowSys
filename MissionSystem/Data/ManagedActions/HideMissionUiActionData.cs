using System;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class HideMissionUiActionData : MissionActionData
    {
        public bool hideCanvas = true;
        public bool hideTextPanel = true;
        public bool hideButtonsPanel = true;

        public override string GetDisplayName()
        {
            return "Hide Mission UI";
        }

        public override string GetTypeName()
        {
            return nameof(HideMissionUiActionData);
        }

        public override bool IsAsync => false;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            context?.HideMissionUI(hideCanvas, hideTextPanel, hideButtonsPanel);
            onComplete?.Invoke();
        }
    }
}
