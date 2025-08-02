using ActionDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class InspectorView : VisualElement
    {
        [Obsolete("Obsolete")] public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        Node _node;
        Type _type;
        List<FieldInfo> _fields = new List<FieldInfo>();
        FieldDrawer _drawer = new FieldDrawer();
        ActionRunner _actionRunner;
        private IMGUIContainer _container;
        private bool _needsRedraw = true;

        internal void ShowInspector(NodeView nodeView, ActionRunner actionRunner)
        {
            // 같은 노드라면 다시 그리지 않음
            if (_node == nodeView.Node && _actionRunner == actionRunner && !_needsRedraw)
                return;
                
            Clear();
            _node = nodeView.Node;
            _actionRunner = actionRunner;
            _needsRedraw = false;

            _type = Runtime.Action.GetOperationType(_node.nameSpace, _node.type);
            if (_type == null)
                return;

            var fields = _type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            _fields.Clear();

            foreach (var field in fields)
            {
                bool isPublic = field.IsPublic;
                bool isSerializeField = Attribute.IsDefined(field, typeof(SerializeField));

                if (isPublic || isSerializeField)
                    _fields.Add(field);
            }

            _container = new IMGUIContainer(DrawInspectorView);
            Add(_container);
        }

        private void DrawInspectorView()
        {
            EditorGUILayout.LabelField(_type.Name, EditorStyles.boldLabel);
            _drawer.Draw(_node.task, _fields, SaveField);
        }

        private void SaveField()
        {
            if (_actionRunner != null)
            {
                EditorUtility.SetDirty(_actionRunner);
                // 필드 값 변경 시에는 다시 그리지 않음 - 이미 그려진 상태에서 값만 변경
                // _needsRedraw = true; // 이 줄을 주석 처리
            }
        }
    }
}