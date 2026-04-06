using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

namespace GAME.MissionSystem
{
    public class MissionDebugStepJumper : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private Button buttonPrefab;

        private bool _menuButtonPressedLastFrame = false;
        private bool _menuVisible = false;
        private List<Button> _spawnedButtons = new List<Button>();

        private void Update()
        {
            InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            bool menuPressed = false;
            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out menuPressed);

            if (menuPressed && !_menuButtonPressedLastFrame)
                ToggleMenu();

            _menuButtonPressedLastFrame = menuPressed;
        }

        private void ToggleMenu()
        {
            _menuVisible = !_menuVisible;

            if (_menuVisible)
                RefreshButtons();

            if (panel != null)
                panel.SetActive(_menuVisible);
        }

        private void RefreshButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            _spawnedButtons.Clear();

            MissionManager manager = MissionManager.Instance;
            if (manager == null || manager.CurrentMission == null)
                return;

            List<MissionStepConfigSO> steps = manager.CurrentMission.steps;
            if (steps == null)
                return;

            for (int i = 0; i < steps.Count; i++)
            {
                MissionStepConfigSO step = steps[i];
                if (step == null)
                    continue;

                Button btn = Instantiate(buttonPrefab, buttonContainer);
                Text label = btn.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = $"{i}. {step.stepName}";

                MissionStepConfigSO captured = step;
                btn.onClick.AddListener(() =>
                {
                    manager.JumpToStep(captured);
                    ToggleMenu();
                });

                _spawnedButtons.Add(btn);
            }
        }
    }
}
