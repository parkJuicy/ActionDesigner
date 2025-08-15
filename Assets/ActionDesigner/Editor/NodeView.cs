using System;
using ActionDesigner.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        private BaseNode _node;
        private Port input;
        private Port output;
        private bool _isRoot;

        public BaseNode Node { get => _node; }
        public Port Input { get => input; }
        public Port Output { get => output; }
        public Action<NodeView> OnNodeSelected { get; internal set; }
        public Action<int> OnNodeRootSet { get; internal set; }
        public Action<NodeView> OnNodeTypeChanged { get; internal set; }

        public NodeView(BaseNode node, bool isRoot) : base(UIToolkitPath.nodeViewUxml)
        {
            _node = node;
            _isRoot = isRoot;
            
            UpdateTitle();
            viewDataKey = _node.id.ToString();

            InitNodeStyle();
            CreatePorts();
            RefreshNodeClasses();
        }
        
        public void UpdateTitle()
        {
            string oldNodeType = DetermineNodeType();
            string oldTitle = title;
            
            // 타이틀 설정
            string newTitle = _node.GetDisplayName();
            title = newTitle;
            
            string newNodeType = DetermineNodeType();
            if (newNodeType != oldNodeType)
            {
                UpdateNodeStyle();
                OnNodeTypeChanged?.Invoke(this);
            }
            else if (oldTitle != newTitle)
            {
                UpdateNodeStyle();
            }
        }

        private string DetermineNodeType()
        {
            return _node.GetNodeType();
        }
        
        private void UpdateNodeStyle()
        {
            RefreshNodeClasses();
        }
        
        private void InitNodeStyle()
        {
            style.left = _node.position.x;
            style.top = _node.position.y;
        }

        private void CreatePorts()
        {
            CreateInputPort();
            CreateOutputPort();
        }

        private void CreateInputPort()
        {
            // Motion은 여러 부모 가능, Condition은 하나의 부모만 가능
            if (_node is MotionNode)
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            else
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            
            input.portName = "";
            input.style.flexDirection = FlexDirection.Row;
            input.style.paddingLeft = 12;
            inputContainer.Add(input);
        }

        private void CreateOutputPort()
        {
            // Motion은 여러 자식 가능, Condition은 하나의 자식만 가능
            if (_node is MotionNode)
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            else
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                
            output.portName = "";
            output.style.flexDirection = FlexDirection.RowReverse;
            output.style.paddingRight = 12;
            outputContainer.Add(output);
        }

        private void RefreshNodeClasses()
        {
            // 기존 클래스들 제거
            RemoveFromClassList("root");
            RemoveFromClassList("motion");
            RemoveFromClassList("condition");
            
            // 새로운 클래스 추가
            if (_isRoot)
            {
                AddToClassList("root");
            }
            else if (_node is MotionNode)
            {
                AddToClassList("motion");
            }
            else if (_node is ConditionNode)
            {
                AddToClassList("condition");
            }
            
            MarkDirtyRepaint();
        }
        
        public void SetRuntimeHighlight(bool highlight)
        {
            if (highlight)
            {
                AddToClassList("runtime-active");
            }
            else
            {
                RemoveFromClassList("runtime-active");
            }
            
            MarkDirtyRepaint();
        }

        public override void SetPosition(Rect newPos)
        {
            if (Application.isPlaying) return;

            base.SetPosition(newPos);
            _node.position = newPos.position;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Motion만 루트 노드로 설정 가능
            if (_node is MotionNode motionNode && motionNode.IsValid)
            {
                evt.menu.AppendAction("Set Root Node", (actionEvent) =>
                {
                    OnNodeRootSet.Invoke(_node.id);
                });
            }
            else
            {
                evt.menu.AppendAction("Set Root Node", null, DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("(루트 노드는 Motion만 가능)", null, DropdownMenuAction.Status.Disabled);
            }
            
            // 디버그 정보
            evt.menu.AppendSeparator();
            evt.menu.AppendAction($"Node ID: {_node.id}", null, DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction($"Type: {_node.GetNodeType()}", null, DropdownMenuAction.Status.Disabled);
            
            if (_node is MotionNode motion)
                evt.menu.AppendAction($"Motion: {motion.motion?.GetType().Name}", null, DropdownMenuAction.Status.Disabled);
            else if (_node is ConditionNode condition)
                evt.menu.AppendAction($"Condition: {condition.condition?.GetType().Name}", null, DropdownMenuAction.Status.Disabled);
        }
    }
}
