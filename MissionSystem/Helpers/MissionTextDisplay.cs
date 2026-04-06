using UnityEngine;
using TMPro;

namespace GAME.MissionSystem
{
    /// <summary>
    /// Composant pour afficher du texte en world space avec plusieurs types de canvas
    /// </summary>
    public class MissionTextDisplay : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private GameObject dialogueCanvas;
        [SerializeField] private GameObject explicationCanvas;
        [SerializeField] private GameObject scoreCanvas;
        [SerializeField] private GameObject uiButtonCanvas;

        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI explicationText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI uiButtonText;

        [Header("Settings")]
        [SerializeField] private bool lookAtCamera = true;
        [SerializeField] private float lookAtSpeed = 5f;

        private Camera mainCamera;
        private TextDisplayType currentDisplayType = TextDisplayType.Dialogue;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (lookAtCamera && mainCamera != null)
            {
                Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookAtSpeed);
            }
        }

        public void ShowText(TextDisplayType displayType, string text)
        {
            HideAll();
            currentDisplayType = displayType;

            switch (displayType)
            {
                case TextDisplayType.Dialogue:
                    if (dialogueCanvas != null) dialogueCanvas.SetActive(true);
                    if (dialogueText != null) dialogueText.text = text;
                    break;

                case TextDisplayType.Explication:
                    if (explicationCanvas != null) explicationCanvas.SetActive(true);
                    if (explicationText != null) explicationText.text = text;
                    break;

                case TextDisplayType.Score:
                    if (scoreCanvas != null) scoreCanvas.SetActive(true);
                    if (scoreText != null) scoreText.text = text;
                    break;

                case TextDisplayType.UIButton:
                    if (uiButtonCanvas != null) uiButtonCanvas.SetActive(true);
                    if (uiButtonText != null) uiButtonText.text = text;
                    break;
            }

            Debug.Log($"[MissionTextDisplay] Showing {displayType} text: {text}");
        }

        public void HideAll()
        {
            if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
            if (explicationCanvas != null) explicationCanvas.SetActive(false);
            if (scoreCanvas != null) scoreCanvas.SetActive(false);
            if (uiButtonCanvas != null) uiButtonCanvas.SetActive(false);
        }

        public void Hide()
        {
            HideAll();
            Debug.Log($"[MissionTextDisplay] Hidden all text displays");
        }
    }
}
