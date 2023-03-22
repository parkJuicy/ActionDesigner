using JuicyFlowChart;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        private Runtime.Node _node;
        private Port input;
        private Port output;
        private bool _isRoot;

        public Runtime.Node Node { get => _node; }
        public Port Input { get => input; }
        public Port Output { get => output; }
        public System.Action<NodeView> OnNodeSelected { get; internal set; }

        public NodeView(Runtime.Node node, bool isRoot) : base(FlowChartEditorPath.nodeViewUxml)
        {
            _node = node;
            _isRoot = isRoot;
            title = _node.type;
            viewDataKey = _node.id.ToString();

            InitNodeStyle();

            if (!_isRoot)
                CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
        }

        private void InitNodeStyle()
        {
            style.left = _node.position.x;
            style.top = _node.position.y;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
        }

        private void CreateInputPorts()
        {
            input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            input.portName = "";
            input.style.flexDirection = FlexDirection.Row;
            input.style.paddingLeft = 12;
            inputContainer.Add(input);
        }

        private void CreateOutputPorts()
        {
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            output.portName = "";
            output.style.flexDirection = FlexDirection.RowReverse;
            output.style.paddingRight = 12;
            outputContainer.Add(output);
        }

        private void SetupClasses()
        {
            if (_isRoot)
            {
                AddToClassList("root");
            }
            //else if (_node.baseType == "Action")
            //{
            //    AddToClassList("action");
            //}
            //else if (_node.baseType == "Condition")
            //{
            //    AddToClassList("condition");
            //}
        }

        public override void SetPosition(Rect newPos)
        {
            if (Application.isPlaying)
                return;

            base.SetPosition(newPos);
            _node.position = newPos.position;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }

        internal void UpdateState()
        {
            //RemoveFromClassList("enable");
            //RemoveFromClassList("disable");

            //if (Application.isPlaying)
            //{
            //    switch (_node.Task.CurrentState)
            //    {
            //        case Task.State.Enable:
            //            AddToClassList("enable");
            //            break;
            //        case Task.State.Disable:
            //            AddToClassList("disable");
            //            break;
            //    }
            //}
        }
    }
}