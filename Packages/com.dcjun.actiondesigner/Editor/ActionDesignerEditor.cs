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
        // 마지막 사용된 Action을 저장 (문제 2 해결)
        private static Runtime.Action _lastUsedAction;
        private static GameObject _lastActionGameObject;
        
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
            
            // 초기 선택 처리 (문제 1 해결)
            EditorApplication.delayCall += () => {
                OnSelectionChange();
            };
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
                    _actionRunner = UnityEngine.Object.FindObjectOfType<ActionRunner>();
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
                // 문제 2, 3 해결: 마지막 Action GameObject 사용
                var targetGameObject = Selection.activeGameObject;
                if (targetGameObject == null || !HasActionComponent(targetGameObject))
                {
                    targetGameObject = _lastActionGameObject;
                }
                
                _nodeInspector.UpdateSelection(nodeView, targetGameObject);
            }
        }

        void OnSelectionChange()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject && HasActionComponent(gameObject))
            {
                var action = GetActionFromGameObject(gameObject);
                if (action != null)
                {
                    _lastUsedAction = action;
                    _lastActionGameObject = gameObject;
                    
                    _actionRunner = gameObject.GetComponent<ActionRunner>();
                    if (_actionView != null)
                    {
                        _actionView.PopulateView(action);
                        if (_actionNameLabel != null)
                        {
                            _actionNameLabel.text = $"{gameObject.name} - Action Designer";
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// GameObject가 Action을 포함한 컴포넌트를 가지고 있는지 확인
        /// </summary>
        private bool HasActionComponent(GameObject gameObject)
        {
            if (gameObject == null) return false;
            
            // ActionRunner 확인
            if (gameObject.GetComponent<ActionRunner>() != null)
                return true;
                
            // 다른 Action을 가진 컴포넌트들 확인 (확장 가능)
            var allComponents = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                if (component == null) continue;
                
                var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Runtime.Action))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// GameObject에서 Action을 가져오기
        /// </summary>
        private Runtime.Action GetActionFromGameObject(GameObject gameObject)
        {
            if (gameObject == null) return null;
            
            // ActionRunner에서 Action 가져오기
            var actionRunner = gameObject.GetComponent<ActionRunner>();
            if (actionRunner != null)
            {
                return actionRunner.Action;
            }
            
            // 다른 컴포넌트에서 Action 찾기
            var allComponents = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                if (component == null) continue;
                
                var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Runtime.Action))
                    {
                        var action = field.GetValue(component) as Runtime.Action;
                        if (action != null)
                        {
                            return action;
                        }
                    }
                }
            }
            
            return null;
        }
    }
}