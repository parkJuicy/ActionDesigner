using System;
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
        private string _lastTaskTypeName; // Task 타입 변경 감지용

        public Runtime.Node Node { get => _node; }
        public Port Input { get => input; }
        public Port Output { get => output; }
        public Action<NodeView> OnNodeSelected { get; internal set; }
        public Action<int> OnNodeRootSet { get; internal set; }

        public NodeView(Runtime.Node node, bool isRoot) : base(UIToolkitPath.nodeViewUxml)
        {
            _node = node;
            _isRoot = isRoot;
            
            UpdateTitle(); // 초기 타이틀 설정
            viewDataKey = _node.id.ToString();

            InitNodeStyle();

            if (!_isRoot)
                CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            
            // 주기적으로 Task 타입 변경 확인
            schedule.Execute(() => CheckForTaskTypeChanges()).Every(100); // 100ms마다 확인
        }
        
        /// <summary>
        /// 외부에서 호출할 수 있는 타이틀 업데이트 메서드
        /// </summary>
        public void RefreshTitle()
        {
            UpdateTitle();
        }
        
        /// <summary>
        /// Task 타입에 따라 노드 타이틀 업데이트
        /// </summary>
        private void UpdateTitle()
        {
            string displayTitle = "Unknown";
            
            if (_node.task != null)
            {
                // Task가 있으면 Task 타입명 사용
                var taskType = _node.task.GetType();
                displayTitle = UnityEditor.ObjectNames.NicifyVariableName(taskType.Name);
                _lastTaskTypeName = taskType.Name;
            }
            else if (!string.IsNullOrEmpty(_node.type))
            {
                // Task가 없으면 노드의 기본 타입 사용
                displayTitle = UnityEditor.ObjectNames.NicifyVariableName(_node.type);
                _lastTaskTypeName = _node.type;
            }
            
            title = displayTitle;
        }
        
        /// <summary>
        /// Task 타입 변경 감지 및 타이틀 업데이트
        /// </summary>
        private void CheckForTaskTypeChanges()
        {
            if (_node?.task == null)
            {
                // Task가 null이 된 경우
                if (_lastTaskTypeName != "None")
                {
                    _lastTaskTypeName = "None";
                    UpdateTitle();
                }
                return;
            }
            
            string currentTaskTypeName = _node.task.GetType().Name;
            if (currentTaskTypeName != _lastTaskTypeName)
            {
                // Task 타입이 변경된 경우
                UpdateTitle();
            }
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
            if (_node.baseType == "Motion")
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            else
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            
            input.portName = "";
            input.style.flexDirection = FlexDirection.Row;
            input.style.paddingLeft = 12;
            inputContainer.Add(input);
        }

        private void CreateOutputPorts()
        {
            // Motion은 여러 개의 자식을 가질 수 있고, Transition은 하나만 가질 수 있음
            if (_node.baseType == "Motion")
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            else // Transition
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                
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
            else if (_node.baseType == "Motion")
            {
                AddToClassList("motion");
            }
            else if (_node.baseType == "Transition")
            {
                AddToClassList("transition");
            }
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

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Motion만 루트 노드로 설정 가능
            if (_node.baseType == "Motion")
            {
                evt.menu.AppendAction("Set Root Node", (actionEvent) =>
                {
                    OnNodeRootSet.Invoke(_node.id);
                });
            }
            else
            {
                evt.menu.AppendAction("Set Root Node", (actionEvent) =>
                {
                    // 아무것도 하지 않음
                }, DropdownMenuAction.Status.Disabled);
                
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("(루트 노드는 Motion만 가능)", (actionEvent) =>
                {
                    // 정보 메시지
                }, DropdownMenuAction.Status.Disabled);
            }
        }
    }
}