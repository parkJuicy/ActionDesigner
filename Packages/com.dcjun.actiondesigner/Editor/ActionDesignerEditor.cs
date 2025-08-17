using ActionDesigner.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace ActionDesigner.Editor
{
    public static class UIToolkitPath
    {
        private static string _packagePath;
        
        public static string PackagePath
        {
            get
            {
                if (string.IsNullOrEmpty(_packagePath))
                {
                    // 패키지 경로 자동 탐지
                    var guids = AssetDatabase.FindAssets("ActionDesignerEditor t:Script");
                    if (guids.Length > 0)
                    {
                        var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var packageRoot = Path.GetDirectoryName(Path.GetDirectoryName(scriptPath));
                        _packagePath = packageRoot.Replace('\\', '/');
                    }
                }
                return _packagePath;
            }
        }
        
        public static string uxmlPath => Path.Combine(PackagePath, "Editor/UIToolkit/ActionDesignerEditor.uxml").Replace('\\', '/');
        public static string ussPath => Path.Combine(PackagePath, "Editor/UIToolkit/ActionDesignerEditor.uss").Replace('\\', '/');
        public static string nodeViewUxml => Path.Combine(PackagePath, "Editor/UIToolkit/NodeView.uxml").Replace('\\', '/');
    }

    public class ActionDesignerEditor : EditorWindow
    {
        ActionRunner _actionRunner;
        ActionView _actionView;
        UIToolkitNodeInspector _nodeInspector;
        Label _actionNameLabel;
        
        // 플레이 모드 대신 런타임 선해
        private bool _wasInPlayMode = false;

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

            _actionNameLabel = root.Q<Label>("actionName");

            if (_actionView != null)
            {
                _actionView.OnNodeSelectionChanged = OnNodeSelectionChanged;
            }

            // 런타임 상태 업데이트 등록
            EditorApplication.update += UpdateRuntimeState;
        }

        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= UpdateRuntimeState;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnterPlayMode();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    OnExitPlayMode();
                    break;
            }
        }

        void OnEnterPlayMode()
        {
            _wasInPlayMode = true;
        }

        void OnExitPlayMode()
        {
            _wasInPlayMode = false;
            _actionView?.UpdateRuntimeStates(null, 0);
        }

        void UpdateRuntimeState()
        {
            if (!_wasInPlayMode || _actionRunner == null)
            {
                // ActionRunner를 다시 찾아보기
                if (Selection.activeGameObject != null)
                {
                    _actionRunner = Selection.activeGameObject.GetComponent<ActionRunner>();
                }
                
                if (_actionRunner == null)
                {
                    _actionRunner = FindObjectOfType<ActionRunner>();
                }
                
                if (_actionRunner == null)
                {
                    _actionView?.UpdateRuntimeStates(null, 0);
                    return;
                }
            }

            if (_actionRunner != null && _actionView != null)
            {
                _actionView.UpdateRuntimeStates(_actionRunner, _actionRunner.currentNodeID);
            }
        }

        void OnNodeSelectionChanged(NodeView nodeView)
        {
            if (_nodeInspector != null)
            {
                _nodeInspector.UpdateSelection(nodeView);
            }
        }

        void OnSelectionChange()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject)
            {
                ActionRunner actionRunner = gameObject.GetComponent<ActionRunner>();
                if (actionRunner)
                {
                    _actionRunner = actionRunner;
                    if (_actionView != null)
                    {
                        _actionView.PopulateView(actionRunner.Action);
                        if (_actionNameLabel != null)
                        {
                            _actionNameLabel.text = $"{gameObject.name} - Action Designer";
                        }
                    }
                }
            }
        }
    }
}