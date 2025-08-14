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
        
            _actionNameLabel = _actionView.Q<Label>("actionName");
            _actionView.OnNodeSelected = OnNodeSelectionChanged;
            
            // 플레이 모드 변화 감지 등록
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _wasInPlayMode = Application.isPlaying;
            
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
        
        /// <summary>
        /// 플레이 모드 변화 처리
        /// </summary>
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                    
                case PlayModeStateChange.EnteredEditMode:
                    if (_wasInPlayMode)
                        RefreshAfterPlayMode();
                    _wasInPlayMode = false;
                    break;
                    
                case PlayModeStateChange.EnteredPlayMode:
                    _wasInPlayMode = true;
                    break;
            }
        }
        
        /// <summary>
        /// 플레이 모드 종료 후 데이터 리프레시
        /// </summary>
        private void RefreshAfterPlayMode()
        {
            if (_actionRunner != null)
            {
                // ActionRunner가 여전히 유효한지 확인
                if (_actionRunner == null || _actionRunner.Action == null)
                {
                    // ActionRunner가 사라졌거나 데이터가 손상됨
                    Debug.LogWarning("Action Designer: ActionRunner 또는 Action 데이터가 손실됨. 비어있는 상태로 전환.");
                    _actionRunner = null;
                    _actionNameLabel.text = "No ActionRunner Selected";
                    _actionView?.ClearView();
                    _nodeInspector?.Clear();
                    return;
                }
                
                // 데이터 리프레시
                Debug.Log($"Action Designer: ActionRunner '{_actionRunner.gameObject.name}' 데이터 리프레시");
                _actionView?.ShowView(_actionRunner);
                _actionNameLabel.text = _actionRunner.gameObject.name;
                
                // 인스펙터 초기화 (선택 노드 해제)
                _nodeInspector?.Clear();
            }
            else
            {
                // 선택된 ActionRunner가 없으면 자동으로 선택 다시 확인
                OnSelectionChange();
            }
        }
        
        /// <summary>
        /// 에디터 창 닫을 때 정리
        /// </summary>
        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
}
