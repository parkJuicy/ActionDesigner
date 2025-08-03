using ActionDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    /// <summary>
    /// Unity Inspector와 유사한 방식으로 작동하는 UI Toolkit 기반 Inspector
    /// </summary>
    public class UIToolkitNodeInspector : VisualElement
    {
        [Obsolete("Obsolete")] public new class UxmlFactory : UxmlFactory<UIToolkitNodeInspector, UxmlTraits> { }

        private Node _currentNode;
        private ActionRunner _actionRunner;
        private SerializedObject _serializedObject;
        private SerializedProperty _nodeProperty;
        
        private ScrollView _scrollView;
        private VisualElement _contentContainer;
        private Label _titleLabel;
        
        // Task 변경 이벤트
        public System.Action OnTaskChanged { get; set; }

        public UIToolkitNodeInspector()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // 기본 스타일 설정
            style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            
            // 제목 라벨
            _titleLabel = new Label("Node Inspector");
            _titleLabel.style.fontSize = 16;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.paddingTop = 8;
            _titleLabel.style.paddingBottom = 8;
            _titleLabel.style.paddingLeft = 10;
            _titleLabel.style.paddingRight = 10;
            _titleLabel.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f, 1f);
            _titleLabel.style.borderBottomWidth = 1;
            _titleLabel.style.borderBottomColor = new Color(0.14f, 0.14f, 0.14f, 1f);
            Add(_titleLabel);

            // 스크롤 뷰
            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            Add(_scrollView);

            // 컨텐츠 컨테이너
            _contentContainer = new VisualElement();
            _contentContainer.style.paddingTop = 10;
            _contentContainer.style.paddingBottom = 10;
            _contentContainer.style.paddingLeft = 10;
            _contentContainer.style.paddingRight = 10;
            _scrollView.Add(_contentContainer);

            ShowEmptyState();
        }

        internal void ShowInspector(NodeView nodeView, ActionRunner actionRunner)
        {
            if (_currentNode == nodeView.Node && _actionRunner == actionRunner)
                return;

            _currentNode = nodeView.Node;
            _actionRunner = actionRunner;

            if (_actionRunner == null || _currentNode == null)
            {
                ShowEmptyState();
                return;
            }

            CreateSerializedObjects();
            BuildInspectorGUI();
        }

        private void CreateSerializedObjects()
        {
            _serializedObject = new SerializedObject(_actionRunner);
            
            // ActionRunner의 action.nodes에서 현재 노드의 인덱스 찾기
            var nodes = _actionRunner.action.nodes;
            int nodeIndex = -1;
            
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == _currentNode)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex >= 0)
            {
                _nodeProperty = _serializedObject.FindProperty($"_action.nodes.Array.data[{nodeIndex}]");
            }
        }

        private void BuildInspectorGUI()
        {
            _contentContainer.Clear();

            if (_nodeProperty == null)
            {
                ShowError("Node property not found");
                return;
            }

            // 타이틀 업데이트
            var nodeTypeName = Runtime.Action.GetOperationType(_currentNode.nameSpace, _currentNode.type)?.Name ?? "Unknown";
            _titleLabel.text = $"{nodeTypeName} Node";

            // 노드의 모든 프로퍼티를 표시
            CreateNodePropertyFields();
        }

        private void CreateNodePropertyFields()
        {
            var iterator = _nodeProperty.Copy();
            var endProperty = iterator.GetEndProperty();
            
            // 첫 번째 자식으로 이동
            if (iterator.NextVisible(true))
            {
                do
                {
                    // 내부 Unity 프로퍼티는 건너뛰기
                    if (iterator.name.StartsWith("m_"))
                        continue;

                    CreatePropertyField(iterator);
                }
                while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, endProperty));
            }
        }

        private void CreatePropertyField(SerializedProperty property)
        {
            // 모든 프로퍼티에 대해 기본 PropertyField 사용
            // SerializeReference 타입의 경우 SubclassSelectorDrawer가 자동으로 처리
            var propertyField = new PropertyField(property.Copy());
            propertyField.Bind(_serializedObject);
            
            // 프로퍼티 변경 시 더티 마킹 및 이벤트 발생
            propertyField.TrackPropertyValue(property.Copy(), (prop) =>
            {
                EditorUtility.SetDirty(_actionRunner);
                // Task 변경 이벤트 발생
                OnTaskChanged?.Invoke();
            });
            
            // 커스텀 스타일 적용
            propertyField.style.marginBottom = 4;
            
            _contentContainer.Add(propertyField);
        }

        private void ShowEmptyState()
        {
            _contentContainer.Clear();
            _titleLabel.text = "Node Inspector";

            var emptyLabel = new Label("No node selected");
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            emptyLabel.style.fontSize = 14;
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            emptyLabel.style.marginTop = 50;
            
            _contentContainer.Add(emptyLabel);
        }

        private void ShowError(string errorMessage)
        {
            _contentContainer.Clear();
            _titleLabel.text = "Inspector Error";

            var errorLabel = new Label($"Error: {errorMessage}");
            errorLabel.style.color = Color.red;
            errorLabel.style.fontSize = 14;
            errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            errorLabel.style.marginTop = 50;
            
            _contentContainer.Add(errorLabel);
        }

        public new void Clear()
        {
            _currentNode = null;
            _actionRunner = null;
            _serializedObject = null;
            _nodeProperty = null;
            ShowEmptyState();
        }
    }
}
