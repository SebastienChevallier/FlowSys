using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using GAME.MissionSystem;

namespace GAME.MissionSystem.Editor
{
    internal sealed class MissionConditionPortData
    {
        public MissionStepNode Node;
        public MissionStepConditionEntry Condition;
        public bool IsSecondary;
    }

    public class MissionFlowGraphView : GraphView
    {
        private MissionFlowGraphWindow _window;
        private MissionFlowConfigSO _currentAsset;
        private Dictionary<string, MissionStepNode> _nodeMap = new Dictionary<string, MissionStepNode>();
        private MissionStepNode _selectedNode;
        private GridBackground _gridBackground;

        public MissionFlowGraphView(MissionFlowGraphWindow window) : base()
        {
            _window = window;

            // Setup grid
            _gridBackground = new GridBackground();
            Insert(0, _gridBackground);

            // Setup zoom/pan
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Try to load stylesheet
            var stylesheet = Resources.Load<StyleSheet>("MissionFlowGraphView");
            if (stylesheet != null)
                styleSheets.Add(stylesheet);

            // Event callbacks
            nodeCreationRequest += OnNodeCreationRequest;
            graphViewChanged += OnGraphViewChanged;

            style.flexGrow = 1;
            ApplySettings();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (port == null || port == startPort)
                    return;

                if (port.node == startPort.node)
                    return;

                if (port.direction == startPort.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void BuildGraphFromAsset(MissionFlowConfigSO asset)
        {
            _currentAsset = asset;
            ClearGraph();

            if (asset == null || asset.steps == null)
            {
                ApplySettings();
                return;
            }

            // Create nodes for all steps
            foreach (var step in asset.steps)
            {
                CreateNodeForStep(step);
            }

            RebuildGraphPresentation();

            _window.SetStatusMessage($"Loaded {asset.steps.Count} steps");
        }

        private void CreateNodeForStep(MissionStepConfigSO step)
        {
            var node = new MissionStepNode(step, this);
            AddElement(node);
            _nodeMap[step.stepId] = node;
        }

        private void ConnectNodes(MissionStepConfigSO sourceStep, MissionStepConditionEntry condition, MissionStepTransition transition, bool isSecondary = false)
        {
            if (!_nodeMap.TryGetValue(sourceStep.stepId, out var sourceNode))
                return;

            Port outputPort = sourceNode.GetOutputPort(condition, isSecondary);
            if (outputPort == null)
                return;

            if (!_nodeMap.TryGetValue(transition.targetStep.stepId, out var targetNode))
                return;

            var edge = new Edge
            {
                output = outputPort,
                input = targetNode.GetInputPort()
            };

            AddElement(edge);
            outputPort.Connect(edge);
            targetNode.GetInputPort().Connect(edge);
            ApplyEdgeStyle(edge);
        }

        public MissionStepNode CreateNewStep()
        {
            if (_currentAsset == null)
                return null;

            var step = ScriptableObject.CreateInstance<MissionStepConfigSO>();
            step.stepId = "Step_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            step.stepName = "New Step";
            step.editorPosition = Vector2.zero;

            _currentAsset.steps.Add(step);
            CreateNodeForStep(step);

            EditorUtility.SetDirty(_currentAsset);
            _window.SetStatusMessage($"Created step: {step.stepId}");

            return _nodeMap[step.stepId];
        }

        public void SaveNodePositions()
        {
            if (_currentAsset == null)
                return;

            foreach (var node in _nodeMap.Values)
            {
                if (node.StepData != null)
                {
                    node.StepData.editorPosition = node.GetPosition().position;
                }
            }

            EditorUtility.SetDirty(_currentAsset);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            bool shouldRefreshGraph = false;

            if (change.edgesToCreate != null)
            {
                for (int i = 0; i < change.edgesToCreate.Count; i++)
                {
                    Edge edge = change.edgesToCreate[i];
                    SynchronizeEdge(edge);
                    shouldRefreshGraph = true;
                }
            }

            if (change.elementsToRemove != null)
            {
                for (int i = 0; i < change.elementsToRemove.Count; i++)
                {
                    if (change.elementsToRemove[i] is Edge edge)
                    {
                        RemoveEdgeBinding(edge);
                        shouldRefreshGraph = true;
                    }
                }
            }

            if (shouldRefreshGraph)
            {
                SaveNodePositions();
                AssetDatabase.SaveAssets();
                RefreshEdges();
            }

            return change;
        }

        private void SynchronizeEdge(Edge edge)
        {
            if (edge?.output?.userData is not MissionConditionPortData portData)
                return;

            if (edge.input?.node is not MissionStepNode targetNode)
                return;

            MissionStepTransition transition = EnsureTransitionForCondition(portData.Node.StepData, portData.Condition, portData.IsSecondary);
            transition.targetStep = targetNode.StepData;
            if (string.IsNullOrEmpty(transition.displayName))
                transition.displayName = GetConditionTransitionDisplayName(portData.Condition, portData.IsSecondary);

            if (portData.IsSecondary)
                portData.Condition.secondaryTargetTransitionId = transition.transitionId;
            else
                portData.Condition.targetTransitionId = transition.transitionId;

            if (portData.IsSecondary && portData.Condition.managedCondition is TimerCountdownMissionConditionData timerCondition)
                timerCondition.targetStepOnTimeout = targetNode.StepData;
            ApplyEdgeStyle(edge);
            MarkAssetDirty();
            portData.Node.RefreshPortsForConditions();
        }

        private void RemoveEdgeBinding(Edge edge)
        {
            if (edge?.output?.userData is not MissionConditionPortData portData)
                return;

            MissionStepTransition transition = GetTransitionForCondition(portData.Node.StepData, portData.Condition, portData.IsSecondary);
            if (transition != null)
                transition.targetStep = null;

            if (portData.IsSecondary)
            {
                portData.Condition.secondaryTargetTransitionId = string.Empty;
                if (portData.Condition.managedCondition is TimerCountdownMissionConditionData timerCondition)
                    timerCondition.targetStepOnTimeout = null;
            }
            else
            {
                portData.Condition.targetTransitionId = string.Empty;
            }

            MarkAssetDirty();
            portData.Node.RefreshPortsForConditions();
        }

        internal MissionStepTransition EnsureTransitionForCondition(MissionStepConfigSO step, MissionStepConditionEntry condition, bool isSecondary = false)
        {
            if (step == null || condition == null)
                return null;

            step.transitions ??= new List<MissionStepTransition>();

            MissionStepTransition transition = GetTransitionForCondition(step, condition, isSecondary);
            if (transition != null)
                return transition;

            transition = new MissionStepTransition
            {
                transitionId = Guid.NewGuid().ToString("N"),
                displayName = GetConditionTransitionDisplayName(condition, isSecondary)
            };
            step.transitions.Add(transition);
            if (isSecondary)
                condition.secondaryTargetTransitionId = transition.transitionId;
            else
                condition.targetTransitionId = transition.transitionId;
            return transition;
        }

        internal MissionStepTransition GetTransitionForCondition(MissionStepConfigSO step, MissionStepConditionEntry condition, bool isSecondary = false)
        {
            if (step?.transitions == null || condition == null)
                return null;

            string transitionId = isSecondary ? condition.secondaryTargetTransitionId : condition.targetTransitionId;
            if (!string.IsNullOrEmpty(transitionId))
            {
                for (int i = 0; i < step.transitions.Count; i++)
                {
                    MissionStepTransition candidate = step.transitions[i];
                    if (candidate != null && string.Equals(candidate.transitionId, transitionId, StringComparison.Ordinal))
                        return candidate;
                }
            }

            return null;
        }

        private static string GetConditionTransitionDisplayName(MissionStepConditionEntry condition, bool isSecondary)
        {
            string baseName = condition?.managedCondition?.GetDisplayName() ?? "Condition";
            return isSecondary ? $"{baseName} [Timeout]" : baseName;
        }

        internal void RefreshEdges()
        {
            if (_currentAsset == null)
            {
                ApplySettings();
                return;
            }

            RebuildGraphPresentation();
        }

        private void RebuildGraphPresentation()
        {
            if (_currentAsset == null)
            {
                ApplySettings();
                return;
            }

            var edgeList = new List<Edge>(edges);
            for (int i = 0; i < edgeList.Count; i++)
                RemoveElement(edgeList[i]);

            foreach (MissionStepNode node in _nodeMap.Values)
                node.RefreshPortsForConditions();

            foreach (MissionStepConfigSO step in _currentAsset.steps)
            {
                step?.EnsureDataIntegrity();

                if (step?.conditions == null)
                    continue;

                for (int i = 0; i < step.conditions.Count; i++)
                {
                    MissionStepConditionEntry condition = step.conditions[i];
                    MissionStepTransition transition = GetTransitionForCondition(step, condition);
                    if (condition != null && transition != null && transition.targetStep != null)
                        ConnectNodes(step, condition, transition);

                    MissionStepTransition secondaryTransition = GetTransitionForCondition(step, condition, true);
                    if (condition != null && secondaryTransition != null && secondaryTransition.targetStep != null)
                        ConnectNodes(step, condition, secondaryTransition, true);
                }
            }

            ApplySettings();
        }

        private void ApplyEdgeStyle(Edge edge)
        {
            if (edge == null)
                return;

            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            if (settings == null)
                return;

            edge.edgeControl.inputColor = settings.connectionColor;
            edge.edgeControl.outputColor = settings.connectionColor;
            edge.edgeControl.edgeWidth = Mathf.RoundToInt(settings.connectionWidth);
            edge.capabilities &= ~Capabilities.Selectable;
            edge.style.opacity = 0.95f;
        }

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            var newNode = CreateNewStep();
            if (newNode != null)
            {
                newNode.SetPosition(new Rect(context.screenMousePosition - _window.position.position, Vector2.zero));
            }
        }

        private void ClearGraph()
        {
            foreach (var node in _nodeMap.Values)
            {
                RemoveElement(node);
            }
            _nodeMap.Clear();
            _selectedNode = null;

            var edgeList = new List<Edge>(edges);
            foreach (var edge in edgeList)
            {
                RemoveElement(edge);
            }
        }

        public void OnNodeSelected(MissionStepNode node)
        {
            _selectedNode = node;
            _window.UpdatePropertyPanel();
        }

        public MissionStepNode GetSelectedNode()
        {
            return _selectedNode;
        }

        public void MarkAssetDirty()
        {
            if (_currentAsset != null)
                EditorUtility.SetDirty(_currentAsset);

            if (_selectedNode?.StepData != null)
                EditorUtility.SetDirty(_selectedNode.StepData);

            _window.Repaint();
        }

        internal void MarkStepDirty(MissionStepConfigSO step)
        {
            if (_currentAsset != null)
                EditorUtility.SetDirty(_currentAsset);

            if (step != null)
                EditorUtility.SetDirty(step);
        }

        internal void SaveNodePosition(MissionStepNode node)
        {
            if (node?.StepData == null)
                return;

            node.StepData.editorPosition = node.GetPosition().position;
            MarkStepDirty(node.StepData);
        }

        public void ApplySettings()
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            if (settings == null)
                return;

            style.backgroundColor = settings.graphBackgroundColor;
            style.unityBackgroundImageTintColor = settings.gridColor;

            if (_gridBackground != null)
            {
                _gridBackground.style.backgroundColor = settings.graphBackgroundColor;
                _gridBackground.style.unityBackgroundImageTintColor = settings.gridColor;
            }

            foreach (MissionStepNode node in _nodeMap.Values)
            {
                node?.ApplySettings(settings);
            }

            foreach (Edge edge in edges)
            {
                ApplyEdgeStyle(edge);
            }
        }
    }

    public class MissionStepNode : Node
    {
        private readonly MissionStepConfigSO _stepData;
        private readonly MissionFlowGraphView _graphView;
        private readonly Port _inputPort;
        private readonly Label _summaryLabel;
        private readonly IMGUIContainer _editorContainer;
        private readonly VisualElement _transitionsContainer;
        private readonly VisualElement _actionsContainer;
        private readonly VisualElement _conditionsContainer;
        private readonly MissionInlineEditorContext _editorContext;
        private readonly Dictionary<MissionStepConditionEntry, Port> _conditionPorts = new Dictionary<MissionStepConditionEntry, Port>();
        private readonly Dictionary<MissionStepConditionEntry, Port> _secondaryConditionPorts = new Dictionary<MissionStepConditionEntry, Port>();
        private readonly Dictionary<MissionStepActionEntry, bool> _actionFoldoutStates = new Dictionary<MissionStepActionEntry, bool>();
        private readonly Dictionary<MissionStepConditionEntry, bool> _conditionFoldoutStates = new Dictionary<MissionStepConditionEntry, bool>();
        private bool _showOnEnterSection = true;
        private bool _showOnExitSection = true;
        private bool _showConditionsSection = true;
        private bool _showTransitionsSection;
        private float _buttonSize = 24f;
        private int _headerFontSize = 11;
        private int _titleFontSize = 12;
        private Color _titleColor = Color.white;
        private int _summaryFontSize = 9;
        private Color _summaryTextColor = Color.white;
        private int _actionFontSize = 11;
        private Color _itemTextColor = Color.white;
        private int _parameterFontSize = 10;
        private Color _parameterTextColor = Color.white;
        private Color _emptyStateTextColor = Color.gray;
        private Color _nodeBackgroundColor = new Color(0.2f, 0.65f, 0.9f, 1f);
        private Color _actionOnEnterColor = new Color(0.08f, 0.32f, 0.7f, 1f);
        private Color _actionOnExitColor = new Color(0.08f, 0.32f, 0.7f, 1f);
        private Color _conditionColor = new Color(1f, 0.2f, 0.2f, 1f);
        private Color _reactionColor = new Color(0.35f, 0.45f, 0.95f, 1f);
        private Color _headerOnEnterColor = new Color(1f, 0.55f, 0.3f, 1f);
        private Color _headerOnExitColor = new Color(1f, 0.55f, 0.3f, 1f);
        private Color _headerConditionsColor = new Color(1f, 0.55f, 0.3f, 1f);
        private Color _headerTransitionsColor = new Color(0.55f, 0.25f, 0.75f, 1f);
        private Color _headerTextColor = Color.white;
        private Color _addButtonTextColor = Color.white;
        private Color _deleteButtonTextColor = Color.white;

        public MissionStepConfigSO StepData => _stepData;

        public MissionStepNode(MissionStepConfigSO stepData, MissionFlowGraphView graphView)
        {
            _stepData = stepData;
            _graphView = graphView;
            _editorContext = new MissionInlineEditorContext
            {
                SaveGraphPositions = MarkStepDirtyOnly,
                Repaint = RefreshVisualState
            };

            title = $"[{stepData.stepId}]";
            SetPosition(new Rect(stepData.editorPosition, Vector2.zero));
            AddToClassList("mission-step-node");

            var summaryContainer = new VisualElement();
            summaryContainer.style.paddingLeft = 6;
            summaryContainer.style.paddingRight = 6;
            summaryContainer.style.paddingTop = 4;
            summaryContainer.style.paddingBottom = 2;

            _summaryLabel = new Label();
            _summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryContainer.Add(_summaryLabel);
            mainContainer.Add(summaryContainer);

            _editorContainer = new IMGUIContainer(DrawNodeEditor);
            _editorContainer.style.paddingLeft = 6;
            _editorContainer.style.paddingRight = 6;
            _editorContainer.style.paddingBottom = 2;
            extensionContainer.Add(_editorContainer);

            _transitionsContainer = new VisualElement();
            _transitionsContainer.style.paddingLeft = 6;
            _transitionsContainer.style.paddingRight = 6;
            _transitionsContainer.style.paddingBottom = 6;
            extensionContainer.Add(_transitionsContainer);

            _actionsContainer = new VisualElement();
            _actionsContainer.style.paddingLeft = 6;
            _actionsContainer.style.paddingRight = 6;
            _actionsContainer.style.paddingBottom = 6;
            extensionContainer.Add(_actionsContainer);

            _conditionsContainer = new VisualElement();
            _conditionsContainer.style.paddingLeft = 6;
            _conditionsContainer.style.paddingRight = 6;
            _conditionsContainer.style.paddingBottom = 6;
            extensionContainer.Add(_conditionsContainer);

            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            _inputPort.portName = string.Empty;
            _inputPort.AddToClassList("mission-step-input-port");
            inputContainer.Add(_inputPort);

            expanded = true;
            RefreshPortsForConditions();
            RefreshExpandedState();
            RefreshPorts();
            RefreshVisualState();
        }

        private void DrawNodeEditor()
        {
            _stepData.EnsureDataIntegrity();
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(2f);
            string previousName = _stepData.stepName;
            _stepData.stepName = EditorGUILayout.TextField("Step Name", _stepData.stepName);
            if (!string.Equals(previousName, _stepData.stepName, StringComparison.Ordinal))
                title = $"[{_stepData.stepId}]";

            if (EditorGUI.EndChangeCheck())
                SaveStepData();
        }


        private void DrawManagedActionEditor(MissionStepActionEntry entry)
        {
            if (entry.managedAction == null)
            {
                EditorGUILayout.HelpBox("Missing managed action instance.", MessageType.Warning);
                return;
            }

            Type managedType = entry.managedAction.GetType();
            if (!MissionManagedTypeEditorRegistry.TryGetActionRenderer(managedType, out IMissionActionEditorRenderer renderer) || renderer == null)
            {
                EditorGUILayout.HelpBox($"No editor renderer registered for {managedType.Name}.", MessageType.Info);
                return;
            }

            renderer.Draw(entry, _editorContext);
        }

        private void ShowAddActionMenu(MissionStepActionPhase phase)
        {
            GenericMenu menu = new GenericMenu();
            bool hasItem = false;

            foreach (Type type in TypeCache.GetTypesDerivedFrom<MissionActionData>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                hasItem = true;
                string label = GetTypeMenuLabel(type);
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    MissionActionData instance = Activator.CreateInstance(type) as MissionActionData;
                    if (instance == null)
                        return;

                    _stepData.AddStructuredAction(new MissionStepActionEntry
                    {
                        phase = phase,
                        managedAction = instance
                    });

                    SaveStepData();
                });
            }

            if (!hasItem)
                menu.AddDisabledItem(new GUIContent("No managed action type found"));

            menu.ShowAsContext();
        }

        private void ShowAddConditionMenu()
        {
            GenericMenu menu = new GenericMenu();
            bool hasItem = false;

            foreach (Type type in TypeCache.GetTypesDerivedFrom<MissionConditionData>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                hasItem = true;
                string label = GetTypeMenuLabel(type);
                menu.AddItem(new GUIContent(label), false, () =>
                {
                    MissionConditionData instance = Activator.CreateInstance(type) as MissionConditionData;
                    if (instance == null)
                        return;

                    _stepData.AddStructuredCondition(new MissionStepConditionEntry
                    {
                        managedCondition = instance
                    });

                    _graphView.EnsureTransitionForCondition(_stepData, _stepData.conditions[_stepData.conditions.Count - 1]);
                    SaveStepStructure(true);
                    _graphView.RefreshEdges();
                });
            }

            if (!hasItem)
                menu.AddDisabledItem(new GUIContent("No managed condition type found"));

            menu.ShowAsContext();
        }

        private void DrawManagedConditionEditor(MissionStepConditionEntry entry)
        {
            if (entry.managedCondition == null)
            {
                EditorGUILayout.HelpBox("Missing managed condition instance.", MessageType.Warning);
                return;
            }

            Type managedType = entry.managedCondition.GetType();
            if (!MissionManagedTypeEditorRegistry.TryGetConditionRenderer(managedType, out IMissionConditionEditorRenderer renderer) || renderer == null)
            {
                EditorGUILayout.HelpBox($"No editor renderer registered for {managedType.Name}.", MessageType.Info);
                return;
            }

            renderer.Draw(entry, _editorContext);
        }

        private void RefreshVisualState()
        {
            title = $"[{_stepData.stepId}]";
            _summaryLabel.text = $"{_stepData.stepName}\nEnter: {_stepData.GetStructuredActionCount(MissionStepActionPhase.OnEnter)} | Exit: {_stepData.GetStructuredActionCount(MissionStepActionPhase.OnExit)} | Cond: {_stepData.GetStructuredConditionCount()} | Transitions: {_stepData.transitions.Count}";
            _editorContainer?.MarkDirtyRepaint();
            RebuildTransitionsUI();
            RebuildActionsUI();
            RebuildConditionsUI();
        }

        private void SaveStepData()
        {
            _stepData.editorPosition = GetPosition().position;
            _graphView?.MarkStepDirty(_stepData);
            RefreshVisualState();
        }

        private void SaveStepStructure(bool refreshPorts)
        {
            _stepData.editorPosition = GetPosition().position;
            _graphView?.MarkStepDirty(_stepData);
            if (refreshPorts)
                RefreshPortsForConditions();
            RefreshVisualState();
        }

        private void MarkStepDirty()
        {
            SaveStepData();
        }

        private void MarkStepDirtyOnly()
        {
            _stepData.editorPosition = GetPosition().position;
            _graphView?.MarkStepDirty(_stepData);
        }

        private static string GetManagedDisplayName(MissionActionData action)
        {
            return action?.GetDisplayName() ?? "(empty action)";
        }

        private static string GetManagedDisplayName(MissionConditionData condition)
        {
            return condition?.GetDisplayName() ?? "(empty condition)";
        }

        private static string GetTypeMenuLabel(Type type)
        {
            string name = type.Name;
            name = name.Replace("MissionActionData", string.Empty);
            name = name.Replace("MissionConditionData", string.Empty);
            name = ObjectNames.NicifyVariableName(name);

            MissionActionCategoryAttribute categoryAttr = (MissionActionCategoryAttribute)Attribute.GetCustomAttribute(type, typeof(MissionActionCategoryAttribute));
            if (categoryAttr != null && !string.IsNullOrEmpty(categoryAttr.Category))
                return $"{categoryAttr.Category}/{name}";

            return name;
        }

        public Port GetInputPort()
        {
            return _inputPort;
        }

        public Port GetOutputPort(MissionStepConditionEntry condition, bool isSecondary = false)
        {
            if (condition == null)
                return null;

            if (isSecondary)
            {
                _secondaryConditionPorts.TryGetValue(condition, out Port secondaryPort);
                return secondaryPort;
            }

            _conditionPorts.TryGetValue(condition, out Port port);
            return port;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            AddToClassList("selected");
            _graphView?.OnNodeSelected(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            RemoveFromClassList("selected");
        }

        public void ApplySettings(MissionFlowGraphSettings settings)
        {
            if (settings == null)
                return;

            _buttonSize = settings.buttonSize;
            _headerFontSize = Mathf.RoundToInt(settings.headerFontSize);
            _titleFontSize = Mathf.RoundToInt(settings.nodeTitleFontSize);
            _titleColor = settings.nodeTitleColor;
            _summaryFontSize = Mathf.RoundToInt(settings.nodeSummaryFontSize);
            _summaryTextColor = settings.nodeSummaryTextColor;
            _actionFontSize = Mathf.RoundToInt(settings.actionFontSize);
            _itemTextColor = settings.itemTextColor;
            _parameterFontSize = Mathf.RoundToInt(settings.parameterFontSize);
            _parameterTextColor = settings.parameterTextColor;
            _emptyStateTextColor = settings.emptyStateTextColor;
            _nodeBackgroundColor = settings.nodeBackgroundColor;
            _actionOnEnterColor = settings.actionOnEnterColor;
            _actionOnExitColor = settings.actionOnExitColor;
            _conditionColor = settings.conditionColor;
            _reactionColor = settings.reactionColor;
            _headerOnEnterColor = settings.headerOnEnterColor;
            _headerOnExitColor = settings.headerOnExitColor;
            _headerConditionsColor = settings.headerConditionsColor;
            _headerTransitionsColor = settings.headerTransitionsColor;
            _headerTextColor = settings.headerTextColor;
            _addButtonTextColor = settings.addButtonTextColor;
            _deleteButtonTextColor = settings.deleteButtonTextColor;
            style.minWidth = settings.nodeMinWidth;
            style.borderTopWidth = settings.nodeBorderWidth;
            style.borderRightWidth = settings.nodeBorderWidth;
            style.borderBottomWidth = settings.nodeBorderWidth;
            style.borderLeftWidth = settings.nodeBorderWidth;
            style.borderTopColor = settings.nodeBorderColor;
            style.borderRightColor = settings.nodeBorderColor;
            style.borderBottomColor = settings.nodeBorderColor;
            style.borderLeftColor = settings.nodeBorderColor;
            style.backgroundColor = settings.nodeBackgroundColor;
            style.borderTopLeftRadius = 10;
            style.borderTopRightRadius = 10;
            style.borderBottomLeftRadius = 10;
            style.borderBottomRightRadius = 10;
            style.overflow = Overflow.Hidden;

            mainContainer.style.borderTopLeftRadius = 10;
            mainContainer.style.borderTopRightRadius = 10;
            mainContainer.style.borderBottomLeftRadius = 10;
            mainContainer.style.borderBottomRightRadius = 10;
            mainContainer.style.overflow = Overflow.Hidden;
            mainContainer.style.backgroundColor = settings.nodeBackgroundColor;

            topContainer.style.borderTopLeftRadius = 10;
            topContainer.style.borderTopRightRadius = 10;
            topContainer.style.borderBottomLeftRadius = 10;
            topContainer.style.borderBottomRightRadius = 10;
            topContainer.style.overflow = Overflow.Hidden;

            extensionContainer.style.borderBottomLeftRadius = 10;
            extensionContainer.style.borderBottomRightRadius = 10;
            extensionContainer.style.overflow = Overflow.Hidden;
            extensionContainer.style.backgroundColor = settings.nodeBackgroundColor;

            titleContainer.style.backgroundColor = settings.nodeBackgroundColor;
            titleContainer.style.color = settings.nodeTitleColor;
            titleContainer.style.fontSize = settings.nodeTitleFontSize;
            titleContainer.style.borderBottomColor = settings.nodeBorderColor;
            titleContainer.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleContainer.style.borderTopLeftRadius = 10;
            titleContainer.style.borderTopRightRadius = 10;
            titleContainer.style.overflow = Overflow.Hidden;

            _summaryLabel.style.color = settings.nodeSummaryTextColor;
            _summaryLabel.style.fontSize = settings.nodeSummaryFontSize;
            _summaryLabel.style.opacity = 0.9f;

            foreach (Port port in _conditionPorts.Values)
            {
                if (port == null)
                    continue;

                port.portColor = settings.connectionColor;
                port.style.marginTop = 2;
                port.style.marginBottom = 2;
            }

            _inputPort.portColor = settings.connectionColor;
            _inputPort.style.marginTop = 8;
            _inputPort.style.marginBottom = 8;

            RefreshPorts();
            _editorContainer?.MarkDirtyRepaint();
            RebuildTransitionsUI();
            RebuildActionsUI();
            RebuildConditionsUI();
        }

        private void RebuildTransitionsUI()
        {
            if (_transitionsContainer == null)
                return;

            _transitionsContainer.Clear();
            _transitionsContainer.Add(CreateTransitionsSectionElement());

            VisualElement spacer = new VisualElement();
            spacer.style.height = 3;
            _transitionsContainer.Add(spacer);
        }

        private VisualElement CreateTransitionsSectionElement()
        {
            int transitionCount = _stepData.transitions?.Count ?? 0;
            VisualElement section = new VisualElement();
            section.style.marginTop = 3;
            section.style.borderTopWidth = 1;
            section.style.borderRightWidth = 1;
            section.style.borderBottomWidth = 1;
            section.style.borderLeftWidth = 1;
            Color sectionBorderColor = Color.Lerp(_nodeBackgroundColor, Color.black, 0.55f);
            section.style.borderTopColor = sectionBorderColor;
            section.style.borderRightColor = sectionBorderColor;
            section.style.borderBottomColor = sectionBorderColor;
            section.style.borderLeftColor = sectionBorderColor;
            section.style.backgroundColor = _reactionColor;
            section.style.paddingBottom = 4;
            section.style.borderTopLeftRadius = 4;
            section.style.borderTopRightRadius = 4;
            section.style.borderBottomLeftRadius = 4;
            section.style.borderBottomRightRadius = 4;

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.backgroundColor = _headerTransitionsColor;
            headerRow.style.paddingLeft = 6;
            headerRow.style.paddingRight = 4;
            headerRow.style.paddingTop = 3;
            headerRow.style.paddingBottom = 3;
            headerRow.style.borderTopLeftRadius = 4;
            headerRow.style.borderTopRightRadius = 4;

            Foldout sectionFoldout = new Foldout();
            sectionFoldout.text = $"Transitions ({transitionCount})";
            sectionFoldout.value = _showTransitionsSection;
            sectionFoldout.style.flexGrow = 1;
            sectionFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionFoldout.style.fontSize = _headerFontSize;
            sectionFoldout.style.color = _headerTextColor;
            sectionFoldout.RegisterValueChangedCallback(evt =>
            {
                _showTransitionsSection = evt.newValue;
                RefreshVisualState();
            });

            headerRow.Add(sectionFoldout);
            section.Add(headerRow);

            if (_showTransitionsSection)
            {
                if (_stepData.transitions == null || _stepData.transitions.Count == 0)
                {
                    Label emptyLabel = new Label("No transitions");
                    emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                    emptyLabel.style.color = _emptyStateTextColor;
                    emptyLabel.style.fontSize = _parameterFontSize;
                    emptyLabel.style.paddingLeft = 8;
                    emptyLabel.style.paddingRight = 8;
                    emptyLabel.style.paddingTop = 6;
                    emptyLabel.style.paddingBottom = 4;
                    section.Add(emptyLabel);
                }
                else
                {
                    for (int i = 0; i < _stepData.transitions.Count; i++)
                        section.Add(CreateTransitionCard(_stepData.transitions[i]));
                }
            }

            return section;
        }

        private VisualElement CreateTransitionCard(MissionStepTransition transition)
        {
            VisualElement card = new VisualElement();
            card.style.marginLeft = 4;
            card.style.marginRight = 4;
            card.style.marginTop = 4;
            card.style.borderTopWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            Color cardBorderColor = Color.Lerp(_nodeBackgroundColor, Color.black, 0.62f);
            card.style.borderTopColor = cardBorderColor;
            card.style.borderRightColor = cardBorderColor;
            card.style.borderBottomColor = cardBorderColor;
            card.style.borderLeftColor = cardBorderColor;
            card.style.backgroundColor = _reactionColor;
            card.style.borderTopLeftRadius = 3;
            card.style.borderTopRightRadius = 3;
            card.style.borderBottomLeftRadius = 3;
            card.style.borderBottomRightRadius = 3;

            Label transitionLabel = new Label(GetTransitionSummaryLabel(transition));
            transitionLabel.style.color = _parameterTextColor;
            transitionLabel.style.fontSize = _parameterFontSize;
            transitionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            transitionLabel.style.paddingLeft = 8;
            transitionLabel.style.paddingRight = 8;
            transitionLabel.style.paddingTop = 6;
            transitionLabel.style.paddingBottom = 6;
            transitionLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(transitionLabel);

            return card;
        }

        private string GetTransitionSummaryLabel(MissionStepTransition transition)
        {
            string targetName = transition?.targetStep != null ? transition.targetStep.stepName : "(unassigned)";
            return transition == null
                ? "(null transition)"
                : $"{transition.displayName} [{transition.transitionId}] → {targetName}";
        }

        private void RebuildActionsUI()
        {
            if (_actionsContainer == null)
                return;

            _actionsContainer.Clear();

            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            Color onEnterHeaderColor = settings != null ? settings.headerOnEnterColor : new Color(0.2f, 0.3f, 0.6f, 1f);
            Color onExitHeaderColor = settings != null ? settings.headerOnExitColor : new Color(0.6f, 0.3f, 0.2f, 1f);

            _actionsContainer.Add(CreateActionSectionElement("On Enter", MissionStepActionPhase.OnEnter, _actionOnEnterColor, onEnterHeaderColor, _showOnEnterSection, value => _showOnEnterSection = value));

            VisualElement spacer = new VisualElement();
            spacer.style.height = 3;
            _actionsContainer.Add(spacer);

            _actionsContainer.Add(CreateActionSectionElement("On Exit", MissionStepActionPhase.OnExit, _actionOnExitColor, onExitHeaderColor, _showOnExitSection, value => _showOnExitSection = value));
        }

        private VisualElement CreateActionSectionElement(string titleText, MissionStepActionPhase phase, Color bodyColor, Color headerColor, bool isExpanded, Action<bool> setExpanded)
        {
            List<MissionStepActionEntry> entries = _stepData.GetStructuredActionEntries(phase);

            VisualElement section = MissionGraphEditorVisualElementUtility.CreateSectionContainer(bodyColor, _nodeBackgroundColor);
            Button addButton = MissionGraphEditorVisualElementUtility.CreateIconButton("+", () => ShowAddActionMenu(phase), _buttonSize, false, true);
            VisualElement headerRow = MissionGraphEditorVisualElementUtility.CreateSectionHeader($"{titleText} ({entries.Count})", isExpanded, value =>
            {
                setExpanded?.Invoke(value);
                RefreshVisualState();
            }, headerColor, _headerTextColor, _headerFontSize, addButton);

            section.Add(headerRow);

            if (isExpanded)
            {
                if (entries.Count == 0)
                {
                    section.Add(MissionGraphEditorVisualElementUtility.CreateEmptyStateLabel("No actions", _emptyStateTextColor, _parameterFontSize));
                }
                else
                {
                    for (int i = 0; i < entries.Count; i++)
                        section.Add(CreateActionCard(entries[i], i, entries.Count, bodyColor));
                }
            }

            return section;
        }

        private VisualElement CreateActionCard(MissionStepActionEntry entry, int index, int total, Color cardColor)
        {
            VisualElement card = MissionGraphEditorVisualElementUtility.CreateCard(cardColor, _nodeBackgroundColor);
            VisualElement headerRow = MissionGraphEditorVisualElementUtility.CreateCardHeader(cardColor);

            Foldout actionFoldout = new Foldout();
            actionFoldout.text = GetManagedDisplayName(entry.managedAction);
            actionFoldout.value = GetActionFoldoutState(entry);
            actionFoldout.style.flexGrow = 1;
            actionFoldout.style.fontSize = _actionFontSize;
            actionFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionFoldout.style.color = _itemTextColor;
            actionFoldout.RegisterValueChangedCallback(evt =>
            {
                _actionFoldoutStates[entry] = evt.newValue;
                RefreshVisualState();
            });

            Button upButton = MissionGraphEditorVisualElementUtility.CreateIconButton("↑", () =>
            {
                _stepData.MoveStructuredAction(entry, -1);
                SaveStepData();
            }, _buttonSize, false, index > 0);

            Button downButton = MissionGraphEditorVisualElementUtility.CreateIconButton("↓", () =>
            {
                _stepData.MoveStructuredAction(entry, 1);
                SaveStepData();
            }, _buttonSize, false, index < total - 1);

            Button removeButton = MissionGraphEditorVisualElementUtility.CreateIconButton("X", () =>
            {
                _stepData.RemoveStructuredAction(entry);
                SaveStepData();
            }, _buttonSize, true, true);

            headerRow.Add(actionFoldout);
            headerRow.Add(upButton);
            headerRow.Add(downButton);
            headerRow.Add(removeButton);
            card.Add(headerRow);

            if (GetActionFoldoutState(entry))
            {
                card.Add(CreateActionBody(entry));
            }

            return card;
        }

        private VisualElement CreateActionBody(MissionStepActionEntry entry)
        {
            if (entry?.managedAction == null)
                return MissionGraphEditorVisualElementUtility.CreateInspectorBody(() => DrawManagedActionEditor(entry));

            Type managedType = entry.managedAction.GetType();

            // Try UI Toolkit renderer first
            if (MissionRendererAdapter.TryGetUIElementRenderer(managedType, out IMissionUIElementRenderer uiRenderer))
            {
                VisualElement uiElement = uiRenderer.BuildElement(entry, _editorContext);
                if (uiElement != null)
                    return uiElement;
            }

            // Fall back to IMGUI wrapper
            return MissionGraphEditorVisualElementUtility.CreateInspectorBody(() => DrawManagedActionEditor(entry));
        }

        public void RefreshPortsForConditions()
        {
            _stepData.EnsureDataIntegrity();

            _conditionPorts.Clear();
            _secondaryConditionPorts.Clear();

            for (int i = 0; i < _stepData.conditions.Count; i++)
            {
                MissionStepConditionEntry condition = _stepData.conditions[i];
                if (condition == null)
                    continue;

                Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                port.portName = string.Empty;
                port.AddToClassList("mission-condition-output-port");
                port.userData = new MissionConditionPortData
                {
                    Node = this,
                    Condition = condition,
                    IsSecondary = false
                };
                port.tooltip = GetConditionPortLabel(condition, i);
                _conditionPorts[condition] = port;

                if (condition.managedCondition is TimerCountdownMissionConditionData)
                {
                    Port secondaryPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                    secondaryPort.portName = string.Empty;
                    secondaryPort.AddToClassList("mission-condition-output-port");
                    secondaryPort.userData = new MissionConditionPortData
                    {
                        Node = this,
                        Condition = condition,
                        IsSecondary = true
                    };
                    secondaryPort.tooltip = $"{GetConditionPortLabel(condition, i)} [Timeout]";
                    _secondaryConditionPorts[condition] = secondaryPort;
                }
            }

            RebuildConditionsUI();
            RefreshPorts();
            RefreshExpandedState();
        }

        private void RebuildConditionsUI()
        {
            if (_conditionsContainer == null)
                return;

            _conditionsContainer.Clear();

            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            Color headerColor = settings != null ? settings.headerConditionsColor : new Color(0.6f, 0.2f, 0.2f, 1f);
            List<MissionStepConditionEntry> entries = _stepData.GetStructuredConditionEntries();

            VisualElement section = MissionGraphEditorVisualElementUtility.CreateSectionContainer(_conditionColor, _nodeBackgroundColor);
            Button addButton = MissionGraphEditorVisualElementUtility.CreateIconButton("+", ShowAddConditionMenu, _buttonSize, false, true);
            VisualElement headerRow = MissionGraphEditorVisualElementUtility.CreateSectionHeader($"Conditions ({entries.Count})", _showConditionsSection, value =>
            {
                _showConditionsSection = value;
                RefreshVisualState();
            }, headerColor, _headerTextColor, _headerFontSize, addButton);

            section.Add(headerRow);

            if (_showConditionsSection)
            {
                if (entries.Count == 0)
                {
                    section.Add(MissionGraphEditorVisualElementUtility.CreateEmptyStateLabel("No conditions", _emptyStateTextColor, _parameterFontSize));
                }
                else
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        section.Add(CreateConditionCard(entries[i], i, entries.Count));
                    }
                }
            }

            _conditionsContainer.Add(section);
        }

        private VisualElement CreateConditionCard(MissionStepConditionEntry entry, int index, int total)
        {
            VisualElement card = MissionGraphEditorVisualElementUtility.CreateCard(_conditionColor, _nodeBackgroundColor);
            VisualElement headerRow = MissionGraphEditorVisualElementUtility.CreateCardHeader(_conditionColor);

            Foldout conditionFoldout = new Foldout();
            conditionFoldout.text = GetManagedDisplayName(entry.managedCondition);
            conditionFoldout.value = GetConditionFoldoutState(entry);
            conditionFoldout.style.flexGrow = 1;
            conditionFoldout.style.fontSize = _actionFontSize;
            conditionFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            conditionFoldout.style.color = _itemTextColor;
            conditionFoldout.RegisterValueChangedCallback(evt =>
            {
                _conditionFoldoutStates[entry] = evt.newValue;
                RefreshVisualState();
            });

            Button upButton = MissionGraphEditorVisualElementUtility.CreateIconButton("↑", () =>
            {
                _stepData.MoveStructuredCondition(entry, -1);
                SaveStepStructure(true);
            }, _buttonSize, false, index > 0);

            Button downButton = MissionGraphEditorVisualElementUtility.CreateIconButton("↓", () =>
            {
                _stepData.MoveStructuredCondition(entry, 1);
                SaveStepStructure(true);
            }, _buttonSize, false, index < total - 1);

            Button removeButton = MissionGraphEditorVisualElementUtility.CreateIconButton("X", () =>
            {
                MissionStepTransition transition = _graphView.GetTransitionForCondition(_stepData, entry);
                MissionStepTransition secondaryTransition = _graphView.GetTransitionForCondition(_stepData, entry, true);
                _stepData.RemoveStructuredCondition(entry);
                if (transition != null)
                    _stepData.transitions.Remove(transition);
                if (secondaryTransition != null)
                    _stepData.transitions.Remove(secondaryTransition);
                SaveStepStructure(true);
                _graphView.RefreshEdges();
            }, _buttonSize, true, true);

            Port port = GetOutputPort(entry);
            Port secondaryPort = GetOutputPort(entry, true);
            if (port != null)
            {
                port.RemoveFromHierarchy();
                port.AddToClassList("mission-condition-output-port");
                port.style.marginLeft = 6;
            }

            if (secondaryPort != null)
            {
                secondaryPort.RemoveFromHierarchy();
                secondaryPort.AddToClassList("mission-condition-output-port");
                secondaryPort.style.marginLeft = 4;
            }

            headerRow.Add(conditionFoldout);
            headerRow.Add(upButton);
            headerRow.Add(downButton);
            headerRow.Add(removeButton);
            card.Add(headerRow);

            if (port != null || secondaryPort != null)
                card.Add(CreateConditionTransitionSummaryRows(entry, port, secondaryPort));

            if (GetConditionFoldoutState(entry))
                card.Add(BuildConditionCardBody(entry));

            return card;
        }

        private VisualElement CreateConditionTransitionSummaryRows(MissionStepConditionEntry entry, Port primaryPort, Port secondaryPort)
        {
            VisualElement container = MissionGraphEditorVisualElementUtility.CreateSummaryContainer();

            AddTransitionSummaryRow(container, GetPrimaryConditionSummaryText(entry), primaryPort);

            if (secondaryPort != null)
                AddTransitionSummaryRow(container, GetSecondaryConditionSummaryText(entry), secondaryPort);

            return container;
        }

        private void AddTransitionSummaryRow(VisualElement container, string label, Port port)
        {
            if (port == null)
                return;

            VisualElement row = MissionGraphEditorVisualElementUtility.CreateSummaryRow(label, port, _parameterTextColor, _parameterFontSize);
            if (row != null)
                container.Add(row);
        }

        private string GetPrimaryConditionSummaryText(MissionStepConditionEntry entry)
        {
            if (entry?.managedCondition is TimerCountdownMissionConditionData timerCondition)
            {
                string actionSuffix = timerCondition.requireAllActionsCompleted ? ", all actions OK" : string.Empty;
                return $"Timer > 0{actionSuffix}";
            }

            return GetManagedDisplayName(entry?.managedCondition);
        }

        private string GetSecondaryConditionSummaryText(MissionStepConditionEntry entry)
        {
            if (entry?.managedCondition is TimerCountdownMissionConditionData)
                return "Timer < 0";

            return "Secondary";
        }

        private VisualElement BuildConditionCardBody(MissionStepConditionEntry entry)
        {
            VisualElement root = new VisualElement();

            // Transition selector rows
            List<MissionStepTransition> transitions = _stepData.transitions ?? new List<MissionStepTransition>();
            if (transitions.Count > 0)
            {
                root.Add(BuildTransitionSelectorField(entry, false, "Target Transition"));
                if (entry?.managedCondition is TimerCountdownMissionConditionData)
                    root.Add(BuildTransitionSelectorField(entry, true, "Timeout Transition"));
            }

            // Try UI Toolkit renderer
            if (entry?.managedCondition != null)
            {
                Type managedType = entry.managedCondition.GetType();
                if (MissionRendererAdapter.TryGetUIConditionElementRenderer(managedType, out IMissionUIConditionElementRenderer uiRenderer))
                {
                    VisualElement uiElement = uiRenderer.BuildElement(entry, _editorContext);
                    if (uiElement != null)
                    {
                        root.Add(uiElement);
                        return root;
                    }
                }
            }

            // Fall back to IMGUI
            root.Add(MissionRendererAdapter.WrapIMGUIInContainer(() =>
            {
                DrawManagedConditionEditor(entry);
            }));

            return root;
        }

        private VisualElement BuildTransitionSelectorField(MissionStepConditionEntry entry, bool isSecondary, string label)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            Label labelElement = new Label(label);
            labelElement.style.minWidth = 150;
            labelElement.style.color = _parameterTextColor;
            labelElement.style.fontSize = _parameterFontSize;

            List<MissionStepTransition> transitions = _stepData.transitions ?? new List<MissionStepTransition>();
            string currentTransitionId = isSecondary ? entry.secondaryTargetTransitionId : entry.targetTransitionId;

            List<string> displayOptions = new List<string> { "First Available Transition" };
            int selectedIndex = 0;

            for (int i = 0; i < transitions.Count; i++)
            {
                MissionStepTransition transition = transitions[i];
                string displayText = transition == null
                    ? "(null transition)"
                    : $"{transition.displayName} [{transition.transitionId}]";
                displayOptions.Add(displayText);

                if (transition != null && string.Equals(currentTransitionId, transition.transitionId, StringComparison.Ordinal))
                    selectedIndex = i + 1;
            }

            PopupField<string> popupField = new PopupField<string>(displayOptions, selectedIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                int chosenIndex = popupField.index;
                if (chosenIndex > 0 && chosenIndex <= transitions.Count)
                {
                    MissionStepTransition selectedTransition = transitions[chosenIndex - 1];
                    if (selectedTransition != null)
                    {
                        if (isSecondary)
                            entry.secondaryTargetTransitionId = selectedTransition.transitionId;
                        else
                            entry.targetTransitionId = selectedTransition.transitionId;
                        SaveStepData();
                    }
                }
            });
            popupField.style.flexGrow = 1;

            row.Add(labelElement);
            row.Add(popupField);

            return row;
        }

        private void DrawConditionCardBody(MissionStepConditionEntry entry)
        {
            DrawTargetTransitionSelector(entry, false);
            if (entry?.managedCondition is TimerCountdownMissionConditionData)
                DrawTargetTransitionSelector(entry, true);
            DrawManagedConditionEditor(entry);
        }

        private void DrawTargetTransitionSelector(MissionStepConditionEntry entry, bool isSecondary)
        {
            List<MissionStepTransition> transitions = _stepData.transitions ?? new List<MissionStepTransition>();
            if (transitions.Count == 0)
            {
                EditorGUILayout.HelpBox("No transition available for this condition.", MessageType.Info);
                return;
            }

            string currentTransitionId = isSecondary ? entry.secondaryTargetTransitionId : entry.targetTransitionId;
            string[] displayedOptions = new string[transitions.Count + 1];
            displayedOptions[0] = "First Available Transition";

            int selectedIndex = 0;
            for (int i = 0; i < transitions.Count; i++)
            {
                MissionStepTransition transition = transitions[i];
                displayedOptions[i + 1] = transition == null
                    ? "(null transition)"
                    : $"{transition.displayName} [{transition.transitionId}]";

                if (transition != null && string.Equals(currentTransitionId, transition.transitionId, StringComparison.Ordinal))
                    selectedIndex = i + 1;
            }

            string label = isSecondary ? "Timeout Transition" : "Target Transition";
            int newIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions);
            string newTransitionId = newIndex <= 0 ? string.Empty : transitions[newIndex - 1]?.transitionId ?? string.Empty;
            if (!string.Equals(currentTransitionId, newTransitionId, StringComparison.Ordinal))
            {
                if (isSecondary)
                    entry.secondaryTargetTransitionId = newTransitionId;
                else
                    entry.targetTransitionId = newTransitionId;

                MarkStepDirty();
                _graphView.RefreshEdges();
            }
        }

        private string GetConditionPortLabel(MissionStepConditionEntry condition, int index)
        {
            string displayName = GetManagedDisplayName(condition?.managedCondition);
            return $"C{index + 1}: {displayName}";
        }

        private bool GetActionFoldoutState(MissionStepActionEntry entry)
        {
            if (entry == null)
                return false;

            if (_actionFoldoutStates.TryGetValue(entry, out bool isExpanded))
                return isExpanded;

            _actionFoldoutStates[entry] = false;
            return false;
        }

        private bool GetConditionFoldoutState(MissionStepConditionEntry entry)
        {
            if (entry == null)
                return false;

            if (_conditionFoldoutStates.TryGetValue(entry, out bool isExpanded))
                return isExpanded;

            _conditionFoldoutStates[entry] = true;
            return true;
        }
    }
}
