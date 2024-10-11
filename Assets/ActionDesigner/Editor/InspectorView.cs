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
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        Node _node;
        Type _type;
        List<FieldInfo> _fields = new List<FieldInfo>();
        FieldDrawer _drawer = new FieldDrawer();
        ActionRunner _actionRunner;

        internal void ShowInspector(NodeView nodeView, ActionRunner actionRunner)
        {
            Clear();
            _node = nodeView.Node;
            _actionRunner = actionRunner;

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

            IMGUIContainer container = new IMGUIContainer(DrawInspectorView);
            Add(container);
        }

        private void DrawInspectorView()
        {
            EditorGUILayout.LabelField(_type.Name, EditorStyles.boldLabel);
            _drawer.Draw(_node.task, _fields, SaveField);
        }

        private void SaveField()
        {
            if (_actionRunner != null)
                EditorUtility.SetDirty(_actionRunner);
        }
    }
}