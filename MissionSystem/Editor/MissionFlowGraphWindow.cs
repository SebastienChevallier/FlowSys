using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GAME.MissionSystem;

namespace GAME.MissionSystem.Editor
{
    public class MissionFlowGraphWindow : EditorWindow
    {
        private MissionFlowConfigSO _currentAsset;
        private MissionFlowGraphView _graphView;
        private IMGUIContainer _propertiesPanel;
        private ObjectField _assetField;
        private Button _saveButton;
        private Button _refreshButton;
        private Button _createStepButton;
        private Label _statusLabel;

        [MenuItem("GAME/MS/Ouvrir le Graph Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionFlowGraphWindow>("Mission Flow Graph");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            ConstructUI();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SaveNodePositions();
        }

        private void ConstructUI()
        {
            rootVisualElement.Clear();

            // Toolbar
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.paddingLeft = 3;
            toolbar.style.paddingRight = 3;
            toolbar.style.paddingTop = 3;
            toolbar.style.paddingBottom = 3;
            toolbar.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1);

            _assetField = new ObjectField { label = "Config", objectType = typeof(MissionFlowConfigSO) };
            _assetField.style.flexGrow = 1;
            _assetField.RegisterValueChangedCallback(evt => OnAssetSelected(evt.newValue as MissionFlowConfigSO));
            toolbar.Add(_assetField);

            _saveButton = new Button(SaveAsset) { text = "Save" };
            toolbar.Add(_saveButton);

            _refreshButton = new Button(RefreshGraph) { text = "Refresh" };
            toolbar.Add(_refreshButton);

            _createStepButton = new Button(CreateNewStep) { text = "Create Step" };
            toolbar.Add(_createStepButton);

            rootVisualElement.Add(toolbar);

            // Status
            _statusLabel = new Label("Ready");
            _statusLabel.style.paddingLeft = 5;
            _statusLabel.style.paddingTop = 3;
            _statusLabel.style.fontSize = 10;
            rootVisualElement.Add(_statusLabel);

            // Main container
            var mainContainer = new VisualElement();
            mainContainer.style.flexGrow = 1;
            mainContainer.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(mainContainer);

            // GraphView
            _graphView = new MissionFlowGraphView(this);
            _graphView.style.flexGrow = 1;
            mainContainer.Add(_graphView);

            _propertiesPanel = null;

            UpdateUI();
        }

        private void DrawPropertiesPanel()
        {
            GUILayout.Label("Properties", EditorStyles.boldLabel);

            if (_currentAsset == null)
            {
                GUILayout.Label("No asset loaded");
                return;
            }

            EditorGUILayout.ObjectField(_currentAsset, typeof(MissionFlowConfigSO), false);

            if (_graphView?.GetSelectedNode() is MissionStepNode selectedNode && selectedNode.StepData != null)
            {
                DrawStepProperties(selectedNode.StepData);
            }
        }

        private void DrawStepProperties(MissionStepConfigSO step)
        {
            GUILayout.Label($"Step: {step.stepName}", EditorStyles.boldLabel);

            step.stepName = EditorGUILayout.TextField("Name", step.stepName);

            GUILayout.Label($"Actions: {step.actions.Count}", EditorStyles.label);
            foreach (var action in step.actions)
            {
                GUILayout.Label($"  • {action.managedAction?.GetDisplayName() ?? "(empty)"}", EditorStyles.miniLabel);
            }

            GUILayout.Label($"Conditions: {step.conditions.Count}", EditorStyles.label);
            foreach (var cond in step.conditions)
            {
                GUILayout.Label($"  • {cond.managedCondition?.GetDisplayName() ?? "(empty)"}", EditorStyles.miniLabel);
            }
        }

        private void OnAssetSelected(MissionFlowConfigSO asset)
        {
            if (asset == null)
            {
                _currentAsset = null;
                _graphView.BuildGraphFromAsset(null);
                _statusLabel.text = "No asset selected";
                UpdateUI();
                return;
            }

            _currentAsset = asset;
            _graphView.BuildGraphFromAsset(asset);
            _statusLabel.text = $"Loaded: {asset.name}";
            UpdateUI();
        }

        private void SaveAsset()
        {
            if (_currentAsset == null)
            {
                _statusLabel.text = "No asset loaded";
                return;
            }

            SaveNodePositions();
            EditorUtility.SetDirty(_currentAsset);
            AssetDatabase.SaveAssets();
            _statusLabel.text = "Saved";
        }

        private void SaveNodePositions()
        {
            if (_currentAsset == null || _graphView == null)
                return;

            _graphView.SaveNodePositions();
        }

        private void CreateNewStep()
        {
            if (_currentAsset == null)
            {
                _statusLabel.text = "Select asset first";
                return;
            }

            _graphView.CreateNewStep();
        }

        private void RefreshGraph()
        {
            if (_graphView == null)
                return;

            if (_currentAsset == null)
            {
                _graphView.BuildGraphFromAsset(null);
                _graphView.ApplySettings();
                _statusLabel.text = "Graph refreshed (no asset)";
                Repaint();
                return;
            }

            AssetDatabase.Refresh();
            _graphView.BuildGraphFromAsset(_currentAsset);
            UpdateUI();
            _statusLabel.text = $"Graph refreshed: {_currentAsset.name}";
            Repaint();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _statusLabel.text = "Play mode - read only";
                _assetField.SetEnabled(false);
                _saveButton.SetEnabled(false);
                _refreshButton.SetEnabled(false);
                _createStepButton.SetEnabled(false);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _assetField.SetEnabled(true);
                _saveButton.SetEnabled(true);
                _refreshButton.SetEnabled(true);
                _createStepButton.SetEnabled(true);
                _statusLabel.text = "Ready";
            }
        }

        private void UpdateUI()
        {
            bool hasAsset = _currentAsset != null;
            _saveButton.SetEnabled(hasAsset);
            _refreshButton.SetEnabled(true);
            _createStepButton.SetEnabled(hasAsset);
        }

        public void UpdatePropertyPanel()
        {
            _propertiesPanel?.MarkDirtyRepaint();
        }

        public MissionFlowConfigSO GetCurrentAsset()
        {
            return _currentAsset;
        }

        public void SetStatusMessage(string message)
        {
            _statusLabel.text = message;
        }

        public void ApplySettings()
        {
            _graphView?.ApplySettings();
            _propertiesPanel?.MarkDirtyRepaint();
        }
    }
}
