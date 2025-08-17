using System;
using System.Linq;
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
            // 좌표를 정수로 따림 (초기 설정 시에도)
            style.left = Mathf.Round(_node.position.x);
            style.top = Mathf.Round(_node.position.y);
            
            // 위치 정확성을 위한 추가 설정
            style.position = Position.Absolute;
            style.marginLeft = 0;
            style.marginTop = 0;
            style.marginRight = 0;
            style.marginBottom = 0;
        }

        private void CreatePorts()
        {
            CreateInputPort();
            CreateOutputPort();
        }

        private void CreateInputPort()
        {
            // Behavior은 여러 부모 가능, Condition은 하나의 부모만 가능
            if (_node is BehaviorNode)
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
            // Behavior은 여러 자식 가능, Condition은 하나의 자식만 가능
            if (_node is BehaviorNode)
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
            RemoveFromClassList("behavior");
            RemoveFromClassList("condition");
            
            // 새로운 클래스 추가
            if (_isRoot)
            {
                AddToClassList("root");
            }
            else if (_node is BehaviorNode)
            {
                AddToClassList("behavior");
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

            var actionView = GetFirstAncestorOfType<ActionView>();
            var finalPos = newPos;

            if (actionView != null)
            {
                float snapDistance = 15f; // 스냅 감도
                var otherNodeViews = actionView.nodes.Cast<NodeView>().Where(n => n != this && n.style.position == Position.Absolute);

                foreach (var otherNodeView in otherNodeViews)
                {
                    var otherPos = otherNodeView.GetPosition();

                    // X축 스냅
                    if (Mathf.Abs(otherPos.x - finalPos.x) < snapDistance)
                    {
                        finalPos.x = otherPos.x;
                    }

                    // Y축 스냅
                    if (Mathf.Abs(otherPos.y - finalPos.y) < snapDistance)
                    {
                        finalPos.y = otherPos.y;
                    }
                }
            }

            // 최종 위치를 정수로 반올림하여 픽셀 정렬
            finalPos.x = Mathf.Round(finalPos.x);
            finalPos.y = Mathf.Round(finalPos.y);

            base.SetPosition(finalPos);
            _node.position = new Vector2(finalPos.x, finalPos.y);
        }


        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Behavior만 루트 노드로 설정 가능
            if (_node is BehaviorNode behaviorNode && behaviorNode.IsValid)
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
                evt.menu.AppendAction("(루트 노드는 Behavior만 가능)", null, DropdownMenuAction.Status.Disabled);
            }
            
            // 디버그 정보
            evt.menu.AppendSeparator();
            evt.menu.AppendAction($"Node ID: {_node.id}", null, DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction($"Type: {_node.GetNodeType()}", null, DropdownMenuAction.Status.Disabled);
            
            if (_node is BehaviorNode behavior)
                evt.menu.AppendAction($"Behavior: {behavior.behavior?.GetType().Name}", null, DropdownMenuAction.Status.Disabled);
            else if (_node is ConditionNode condition)
                evt.menu.AppendAction($"Condition: {condition.condition?.GetType().Name}", null, DropdownMenuAction.Status.Disabled);
        }
    }
}