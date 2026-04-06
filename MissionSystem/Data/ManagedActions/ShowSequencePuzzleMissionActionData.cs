using System;
using UnityEngine;

namespace GAME.MissionSystem
{
    [MissionActionCategory("UI")]
    [Serializable]
    public sealed class ShowSequencePuzzleMissionActionData : MissionActionData
    {
        public string[] vignetteIds = Array.Empty<string>();
        public Sprite[] vignetteSprites = Array.Empty<Sprite>();
        public int[] correctSequence = Array.Empty<int>();
        public int maxAttempts = 2;

        public override string GetDisplayName()
        {
            return $"Sequence Puzzle ({vignetteIds?.Length ?? 0} vignettes, max {maxAttempts} attempts)";
        }

        public override string GetTypeName() => nameof(ShowSequencePuzzleMissionActionData);
        public override bool IsAsync => true;

        public override void Execute(IMissionContext context, Action onComplete)
        {
            MissionTextUIManager uiManager = MissionTextUIManager.Instance;
            if (uiManager == null)
            {
                Debug.LogError("[MissionSystem] Cannot show sequence puzzle: UIManager not found");
                onComplete?.Invoke();
                return;
            }

            SequencePuzzle puzzle = uiManager.GetSequencePuzzle();
            if (puzzle == null)
            {
                Debug.LogError("[MissionSystem] Cannot show sequence puzzle: SequencePuzzle component not found in UIManager");
                onComplete?.Invoke();
                return;
            }

            puzzle.Setup(vignetteIds, vignetteSprites, correctSequence, maxAttempts, _ => onComplete?.Invoke());
        }
    }
}
