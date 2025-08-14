using ActionDesigner.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public static class UIToolkitPath
    {
        public const string uxmlPath = "Assets/ActionDesigner/Editor/UIToolkit/ActionDesignerEditor.uxml";
        public const string ussPath = "Assets/ActionDesigner/Editor/UIToolkit/ActionDesignerEditor.uss";
        public const string nodeViewUxml = "Assets/ActionDesigner/Editor/UIToolkit/NodeView.uxml";
    }

    public class ActionDesignerEditor : EditorWindow
    {
        ActionRunner _actionRunner;
        ActionView _actionView;
        UIToolkitNodeInspector _nodeInspector;
        Label _actionNameLabel;

        [MenuItem("Action Designer/Editor...")]
        public static void OpenWindow()
        {
            ActionDesignerEditor window = GetWindow<ActionDesignerEditor>();
            window.titleContent = new GUIContent("Action Designer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIToolkitPath.uxmlPath);
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIToolkitPath.ussPath);
            root.styleSheets.Add(styleSheet);

            _actionView = root.Q<ActionView>();
            
            var leftPanel = root.Q<VisualElement>("left-panel");
            _nodeInspector = new UIToolkitNodeInspector();
            _nodeInspector.style.flexGrow = 1;
            _nodeInspector.OnTaskChanged = () => {
                // Task가 변경되면 즉시 NodeView 타이틀 새로고침
                _actionView?.RefreshAllNodeTitles();
            };
            leftPanel.Add(_nodeInspector);
        
            _actionNameLabel = _actionView.Q<Label>("actionName");
            _actionView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        void OnNodeSelectionChanged(NodeView nodeView)
        {
            _nodeInspector?.ShowInspector(nodeView, _actionRunner);
        }

        void OnSelectionChange()
        {
            if (Selection.activeGameObject)
            {
                _actionRunner = Selection.activeGameObject.GetComponent<ActionRunner>();
                if (_actionRunner)
                {
                    _actionView?.ShowView(_actionRunner);
                    _actionNameLabel.text = _actionRunner.gameObject.name;
                    return;
                }
            }

            _actionRunner = null;
            _actionNameLabel.text = null;
            _actionView?.ClearView();
            _nodeInspector?.Clear();
        }
    }
}
