using ActionDesigner.Runtime;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class UIToolkitNodeInspector : VisualElement
    {
        [Obsolete("Obsolete")] public new class UxmlFactory : UxmlFactory<UIToolkitNodeInspector, UxmlTraits>
        {
        }

        private BaseNode _currentNode;
        private GameObject _targetGameObject;
        private SerializedObject _serializedObject;
        private SerializedProperty _nodeProperty;

        private ScrollView _scrollView;
        private VisualElement _contentContainer;

        public System.Action OnTaskChanged { get; set; }

        public UIToolkitNodeInspector()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // 기본 스타일 설정
            style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);

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

        public void UpdateSelection(NodeView nodeView, GameObject targetGameObject = null)
        {
            if (nodeView == null)
            {
                ShowEmptyState();
                return;
            }

            // targetGameObject가 제공되지 않으면 Selection에서 찾기
            if (targetGameObject == null)
            {
                targetGameObject = Selection.activeGameObject;
            }

            // 여전히 없으면 ActionComponent를 가진 GameObject 찾기
            if (targetGameObject == null || !HasActionComponent(targetGameObject))
            {
                targetGameObject = FindGameObjectWithAction();
            }

            if (targetGameObject == null)
            {
                ShowError("Action component not found. Please select a GameObject with Action component.");
                return;
            }

            ShowInspector(nodeView, targetGameObject);
        }

        internal void ShowInspector(NodeView nodeView, GameObject targetGameObject)
        {
            if (_currentNode == nodeView.Node && _targetGameObject == targetGameObject)
                return;

            _currentNode = nodeView.Node;
            _targetGameObject = targetGameObject;

            if (_targetGameObject == null || _currentNode == null)
            {
                ShowEmptyState();
                return;
            }

            CreateSerializedObjects();
            BuildInspectorGUI();
        }

        private void CreateSerializedObjects()
        {
            var actionComponent = FindActionComponent(_targetGameObject);
            if (actionComponent == null)
            {
                _serializedObject = null;
                _nodeProperty = null;
                return;
            }

            _serializedObject = new SerializedObject(actionComponent);
            var action = GetActionFromComponent(actionComponent);

            if (action == null)
            {
                _nodeProperty = null;
                return;
            }

            var nodes = action.nodes;
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
                // 다른 컴포넌트의 경우 Action 필드 찾기
                var actionFieldName = FindActionFieldName(actionComponent);
                if (!string.IsNullOrEmpty(actionFieldName))
                {
                    _nodeProperty = _serializedObject.FindProperty($"{actionFieldName}.nodes.Array.data[{nodeIndex}]");
                }
            }
        }

        private void BuildInspectorGUI()
        {
            _contentContainer.Clear();

            if (_nodeProperty == null)
            {
                ShowError("Node property not found in the selected component.");
                return;
            }

            // 노드의 속성들 표시
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
                if (_targetGameObject != null)
                {
                    var actionComponent = FindActionComponent(_targetGameObject);
                    if (actionComponent != null)
                    {
                        EditorUtility.SetDirty(actionComponent);
                    }
                }
                // Behavior/Condition 변경 이벤트 발생
                OnTaskChanged?.Invoke();
            });

            // 커스텀 스타일 적용
            propertyField.style.marginBottom = 4;

            _contentContainer.Add(propertyField);
        }

        private void ShowEmptyState()
        {
            _contentContainer.Clear();

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

            var errorLabel = new Label($"Error: {errorMessage}");
            errorLabel.style.color = Color.red;
            errorLabel.style.fontSize = 14;
            errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            errorLabel.style.marginTop = 50;
            errorLabel.style.whiteSpace = WhiteSpace.Normal;

            _contentContainer.Add(errorLabel);
        }

        public new void Clear()
        {
            _currentNode = null;
            _targetGameObject = null;
            _serializedObject = null;
            _nodeProperty = null;
            ShowEmptyState();
        }

        /// <summary>
        /// GameObject가 Action 컴포넌트를 가지고 있는지 확인
        /// </summary>
        private bool HasActionComponent(GameObject gameObject)
        {
            if (gameObject == null) return false;
            return FindActionComponent(gameObject) != null;
        }

        /// <summary>
        /// GameObject에서 Action을 가진 컴포넌트 찾기
        /// </summary>
        private MonoBehaviour FindActionComponent(GameObject gameObject)
        {
            if (gameObject == null) return null;
            // 다른 MonoBehaviour에서 Action 필드를 가진 것 찾기
            var allComponents = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                if (component == null) continue;

                var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(Runtime.Action))
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 컴포넌트에서 Action 가져오기
        /// </summary>
        private Runtime.Action GetActionFromComponent(MonoBehaviour component)
        {
            if (component == null) return null;
            // 다른 컴포넌트의 경우
            var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Runtime.Action))
                {
                    return field.GetValue(component) as Runtime.Action;
                }
            }

            return null;
        }

        /// <summary>
        /// 컴포넌트에서 Action 필드 이름 찾기
        /// </summary>
        private string FindActionFieldName(MonoBehaviour component)
        {
            if (component == null) return null;
            // 다른 컴포넌트의 경우
            var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Runtime.Action))
                {
                    return field.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// Scene에서 Action을 가진 GameObject 찾기
        /// </summary>
        private GameObject FindGameObjectWithAction()
        {
            // 다른 Action 컴포넌트들도 찾기
            var allMonoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var component in allMonoBehaviours)
            {
                if (HasActionComponent(component.gameObject))
                {
                    return component.gameObject;
                }
            }

            return null;
        }
    }
}
