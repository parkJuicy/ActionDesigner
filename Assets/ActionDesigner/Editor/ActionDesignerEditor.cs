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
        InspectorView _inspectorView;

        Label _actionNameLabel;
        string _selectedAction;

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
            _inspectorView = root.Q<InspectorView>();
            _actionNameLabel = _actionView.Q<Label>("actionName");

            _actionView.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.ShowInspector(nodeView, _actionRunner);
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
            _inspectorView?.Clear();
        }

        void OnInspectorUpdate()
        {
            if (_actionRunner == null)
                return;
        }
    }
}