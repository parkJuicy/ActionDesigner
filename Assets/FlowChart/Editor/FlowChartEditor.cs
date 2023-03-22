using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuicyFlowChart
{
    public static class FlowChartEditorPath
    {
        public const string uxmlPath = "Assets/FlowChart/Editor/UIBuilder/FlowChartEditor.uxml";
        public const string ussPath = "Assets/FlowChart/Editor/UIBuilder/FlowChartEditor.uss";
        public const string nodeViewUxml = "Assets/FlowChart/Editor/UIBuilder/NodeView.uxml";
    }

    public class FlowChartEditor : EditorWindow
    {
        private FlowChartView _flowChartView;
        private InspectorView _inspectorView;
        private FlowChart _flowChart;

        private Label _flowChartName;
        private string _selectedName;

        private Button _saveButton;
        private Color _saveButtonColor = new Color(0.96f, 0.2f, 0.26f);

        [MenuItem("FlowChart/Editor...")]
        public static void OpenWindow()
        {
            FlowChartEditor wnd = GetWindow<FlowChartEditor>();
            wnd.titleContent = new GUIContent("FlowChartEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is FlowChart)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FlowChartEditorPath.uxmlPath);
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FlowChartEditorPath.ussPath);
            root.styleSheets.Add(styleSheet);

            _flowChartView = root.Q<FlowChartView>();
            _inspectorView = root.Q<InspectorView>();
            _flowChartName = _flowChartView.Q<Label>("flowChartName");
            _saveButton = root.Q<Button>("save");
            _saveButton.clicked += SaveFlowChart;
            _saveButton.style.backgroundColor = Color.gray;

            _flowChartView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.ShowInspector(nodeView, _flowChart);
        }

        private void SaveFlowChart()
        {
            AssetDatabase.SaveAssets();
            Debug.Log("<color=cyan>SAVE COMPLETE</color>");
            _saveButton.style.backgroundColor = Color.gray;
        }

        private void OnSelectionChange()
        {
            // Select FlowChart
            FlowChart selectedFlowChart = Selection.activeObject as FlowChart;
            if (!selectedFlowChart)
            {
                if (Selection.activeGameObject)
                {
                    FlowChartRunner runner = Selection.activeGameObject.GetComponent<FlowChartRunner>();
                    if (runner && runner.FlowChart != null)
                    {
                        _flowChart = runner.FlowChart;
                        ConnectTaskToNode(runner.Root);
                        _selectedName = string.Format($"{runner.name} - {_flowChart.name}");
                    }
                }
            }
            else
            {
                _flowChart = selectedFlowChart;
                _selectedName = _flowChart.name;
            }

            // Show FlowChart
            if (_flowChart)
            {
                _flowChartView?.ShowView(_flowChart);
            }
            else
            {
                _flowChartView?.ClearView();
            }

            // Show FlowChart Name
            if (_flowChartName != null)
                _flowChartName.text = _selectedName;
        }

        private void OnProjectChange()
        {
            if (_flowChart)
            {
                if (_flowChartName != null)
                    _flowChartName.text = _flowChart.name;
            }
        }

        private void OnInspectorUpdate()
        {
            if (_flowChart == null)
                return;

            _flowChartView?.UpdateNodeState();
            if(EditorUtility.IsDirty(_flowChart))
            {
                _saveButton.style.backgroundColor = _saveButtonColor;
            }
        }

        private void ConnectTaskToNode(Task rootTask)
        {
            if (_flowChart.RootID == 0 || rootTask == null)
                return;

            Node rootNode = _flowChart.Nodes.Find(x => x.ID == _flowChart.RootID);
            rootNode.Task = rootTask;
            Traverse(rootNode, rootTask);
        }

        public void Traverse(Node node, Task task)
        {
            if (node != null)
            {
                List<int> childrenID = node.ChildrenID;
                childrenID.ForEach((nodeID) =>
                {
                    Node targetNode = _flowChart.Nodes.Find(x => x.ID == nodeID);
                    Task targetTask;
                    targetTask = task.Children.Find(x => x.NodeID == targetNode.ID);
                    targetNode.Task = targetTask;
                    Traverse(targetNode, targetTask);
                });
            }
        }
    }
}