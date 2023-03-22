using ActionDesigner.Editor;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuicyFlowChart
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        private Node _node;
        private FlowChart _flowChart;
        private object _selectedInstance;
        private Type _type;
        private FieldInfo[] _fields;
        private FieldDrawer _drawer = new FieldDrawer();

        internal void ShowInspector(NodeView nodeView, FlowChart flowChart)
        {
            Clear();
            _node = nodeView.Node;

            _flowChart = flowChart;
            _type = FlowChart.GetNodeType(_node.Namespace, _node.Name);
            if (_type == null)
                return;

            _selectedInstance = JsonUtility.FromJson(_node.Data, _type);
            _fields = _type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            IMGUIContainer container = new IMGUIContainer(DrawInspectorView);
            Add(container);
        }

        private void DrawInspectorView()
        {
            EditorGUILayout.LabelField(_type.Name,EditorStyles.boldLabel);
            //_drawer.Draw(_selectedInstance, _fields, SaveField);
        }

        private void SaveField()
        {
            _node.Data = JsonUtility.ToJson(_selectedInstance);
            EditorUtility.SetDirty(_flowChart);
        }
    }
}