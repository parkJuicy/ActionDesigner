using ActionDesigner.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Motion = ActionDesigner.Runtime.Motion;
using Condition = ActionDesigner.Runtime.Condition;

namespace ActionDesigner.Editor
{
    public class ActionView : GraphView
    {
        [Obsolete("Obsolete")] public new class UxmlFactory : UxmlFactory<ActionView, UxmlTraits> { }
        ActionRunner _actionRunner;
        Runtime.Action _action;

        public Action<NodeView> OnNodeSelected { get; internal set; }

        public ActionView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIToolkitPath.ussPath);
            styleSheets.Add(styleSheet);
        }
        
        /// <summary>
        /// 모든 NodeView의 타이틀을 새로고침
        /// </summary>
        public void RefreshAllNodeTitles()
        {
            foreach (var element in graphElements)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.UpdateTitle();
                }
            }
            
            // 노드 타입 변경 후 잘못된 연결 제거
            ValidateAndCleanConnections();
        }
        
        /// <summary>
        /// 잘못된 연결들을 검사하고 제거
        /// </summary>
        private void ValidateAndCleanConnections()
        {
            if (_action == null) return;
            
            var invalidConnections = new List<(BaseNode parent, BaseNode child)>();
            
            // 모든 연결 검사
            foreach (var node in _action.nodes)
            {
                for (int i = node.childrenID.Count - 1; i >= 0; i--)
                {
                    var childID = node.childrenID[i];
                    var childNode = _action.nodes.Find(n => n.id == childID);
                    
                    if (childNode == null)
                    {
                        // 자식 노드가 없음
                        node.childrenID.RemoveAt(i);
                        continue;
                    }
                    
                    // 연결 규칙 검증
                    if (!IsValidConnection(node, childNode))
                    {
                        invalidConnections.Add((node, childNode));
                    }
                }
            }
            
            // 잘못된 연결 제거
            foreach (var (parent, child) in invalidConnections)
            {
                RemoveConnection(parent, child);
                Debug.LogWarning($"잘못된 연결 제거: {parent.GetDisplayName()} -> {child.GetDisplayName()}");
            }
            
            if (invalidConnections.Count > 0)
            {
                // UI 새로고침
                RefreshGraphView();
                EditorUtility.SetDirty(_actionRunner);
            }
        }

        /// <summary>
        /// 연결이 유효한지 검증 (개선된 단일 메서드)
        /// </summary>
        private bool IsValidConnection(BaseNode parent, BaseNode child)
        {
            // 노드가 유효하지 않으면 연결 불가
            if (parent == null || child == null) return false;
            
            bool parentIsMotion = parent is MotionNode motionParent && motionParent.IsValid;
            bool parentIsCondition = parent is ConditionNode conditionParent && conditionParent.IsValid;
            bool childIsMotion = child is MotionNode motionChild && motionChild.IsValid;
            bool childIsCondition = child is ConditionNode conditionChild && conditionChild.IsValid;
            
            if (!parentIsMotion && !parentIsCondition) return false;
            if (!childIsMotion && !childIsCondition) return false;
            
            // Motion → Condition만 허용
            if (parentIsMotion && !childIsCondition) return false;
            
            // Condition → Motion만 허용  
            if (parentIsCondition && !childIsMotion) return false;
            
            // Condition은 하나의 자식만 가능 (체인 구조)
            if (parentIsCondition && parent.childrenID.Count >= 1) return false;
            
            return true;
        }

        /// <summary>
        /// 연결 제거
        /// </summary>
        private void RemoveConnection(BaseNode parent, BaseNode child)
        {
            _action.RemoveChild(parent, child);
        }
        
        /// <summary>
        /// 그래프 뷰 전체 새로고침
        /// </summary>
        private void RefreshGraphView()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            DrawNode();
            DrawEdge();
        }
        
        internal void ShowView(ActionRunner actionRunner)
        {
            _actionRunner = actionRunner;
            _action = actionRunner.action;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            DrawNode();
            DrawEdge();
        }

        internal void ClearView()
        {
            _actionRunner = null;
            _action = null;
            DeleteElements(graphElements.ToList());
        }

        private void DrawNode()
        {
            _action.nodes.ForEach(node => CreateNodeView(node));
        }

        private void DrawEdge()
        {
            _action.nodes.ForEach(node =>
            {
                node.childrenID.ForEach(childID =>
                {
                    NodeView parentView = FindNodeView(node.id);
                    NodeView childView = FindNodeView(childID);

                    if (parentView != null && childView != null)
                    {
                        Edge edge = parentView.Output.ConnectTo(childView.Input);
                        AddElement(edge);
                    }
                });
            });
        }

        NodeView FindNodeView(int nodeID)
        {
            return GetNodeByGuid(nodeID.ToString()) as NodeView;
        }

        void CreateNodeView(BaseNode node)
        {
            NodeView nodeView = new NodeView(node, node.id == _action.rootID)
            {
                OnNodeSelected = OnNodeSelected,
                OnNodeRootSet = OnNodeRootSet,
                OnNodeTypeChanged = OnNodeTypeChanged
            };
            AddElement(nodeView);
        }
        
        /// <summary>
        /// 노드 타입 변경 시 호출되는 이벤트 핸들러
        /// </summary>
        void OnNodeTypeChanged(NodeView nodeView)
        {
            // 즉시 연결 검증 및 정리
            ValidateAndCleanConnections();
        }
        
        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (_action == null || Application.isPlaying)
                return graphViewChange;

            // Delete Node
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    NodeView nodeView = element as NodeView;
                    if (nodeView != null)
                    {
                        _action.DeleteNode(nodeView.Node);
                    }

                    Edge edge = element as Edge;
                    if (edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        _action.RemoveChild(parentView.Node, childView.Node);
                    }
                });
            }

            // Create Edge with validation
            if (graphViewChange.edgesToCreate != null)
            {
                var validEdges = new List<Edge>();
                
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    
                    // 연결 규칙 검증
                    if (IsValidConnection(parentView.Node, childView.Node))
                    {
                        _action.AddChild(parentView.Node, childView.Node);
                        validEdges.Add(edge);
                    }
                    else
                    {
                        // 개선된 에러 메시지
                        ShowConnectionError(parentView.Node, childView.Node);
                        
                        // 잘못된 연결 제거
                        edge.output.Disconnect(edge);
                        edge.input.Disconnect(edge);
                        RemoveElement(edge);
                    }
                });
                
                graphViewChange.edgesToCreate = validEdges;
            }

            if (graphViewChange.movedElements != null)
            {
                graphViewChange.movedElements.ForEach((node) =>
                {
                    NodeView nodeView = node as NodeView;
                    if (nodeView?.Input?.connections != null)
                    {
                        foreach (var parentEdge in nodeView.Input.connections)
                        {
                            if (parentEdge == null) break;

                            var parent = parentEdge.output.node as NodeView;
                            parent?.Node?.childrenID.Sort(SortByHoriziontalPosition);
                        }
                    }
                });
            }

            EditorUtility.SetDirty(_actionRunner);
            return graphViewChange;
        }

        /// <summary>
        /// 개선된 연결 에러 메시지
        /// </summary>
        private void ShowConnectionError(BaseNode parent, BaseNode child)
        {
            string parentName = parent.GetDisplayName();
            string childName = child.GetDisplayName();
            
            bool parentIsMotion = parent is MotionNode;
            bool parentIsCondition = parent is ConditionNode;
            bool childIsMotion = child is MotionNode;
            bool childIsCondition = child is ConditionNode;
            
            if (parentIsMotion && childIsMotion)
            {
                Debug.LogWarning($"Motion끼리는 연결할 수 없습니다. Motion 뒤에는 Condition이 와야 합니다: {parentName} -> {childName}");
            }
            else if (parentIsCondition && childIsCondition)
            {
                Debug.LogWarning($"Condition끼리는 연결할 수 없습니다. Condition 뒤에는 Motion이 와야 합니다: {parentName} -> {childName}");
            }
            else if (parentIsCondition && parent.childrenID.Count >= 1)
            {
                Debug.LogWarning($"Condition은 하나의 자식만 가질 수 있습니다 (체인 구조): {parentName}");
            }
            else
            {
                Debug.LogWarning($"유효하지 않은 연결입니다: {parentName} -> {childName}");
            }
        }

        int SortByHoriziontalPosition(int left, int right)
        {
            var leftNode = FindNodeView(left)?.Node;
            var rightNode = FindNodeView(right)?.Node;
            
            if (leftNode == null || rightNode == null) return 0;
            
            return leftNode.position.x < rightNode.position.x ? -1 : 1;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_action == null || Application.isPlaying)
                return;

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Motion/", null, DropdownMenuAction.Status.Disabled);
            ShowNodeTypes<Motion>(evt, "Motion");
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Condition/", null, DropdownMenuAction.Status.Disabled);
            ShowNodeTypes<Condition>(evt, "Condition");
        }

        void ShowNodeTypes<T>(ContextualMenuPopulateEvent evt, string baseType) where T : class
        {
            VisualElement contentViewContainer = ElementAt(1);
            Vector3 screenMousePosition = evt.localMousePosition;
            Vector2 worldMousePosition = screenMousePosition - contentViewContainer.transform.position;
            worldMousePosition *= 1 / contentViewContainer.transform.scale.x;

            var types = TypeCache.GetTypesDerivedFrom<T>();
            foreach (var type in types)
            {
                if (type.IsAbstract) continue;

                string menu;
                if (string.IsNullOrEmpty(type.Namespace))
                    menu = $"{baseType}/{type.Name}";
                else
                    menu = $"{baseType}/{type.Namespace.Replace("ActionDesigner.Runtime.", "")}/{type.Name}";

                evt.menu.AppendAction(menu, (actionEvent) =>
                {
                    BaseNode node = _action.CreateNode(type.Name, type.Namespace, baseType, worldMousePosition);
                    
                    // 루트 노드는 Motion만 가능
                    if (_action.rootID == 0 && baseType == "Motion")
                        _action.rootID = node.id;

                    CreateNodeView(node);
                    EditorUtility.SetDirty(_actionRunner);
                });
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (Application.isPlaying)
            {
                return new List<Port>();
            }

            var compatiblePorts = new List<Port>();
            var startNodeView = startPort.node as NodeView;
            
            if (startNodeView == null)
                return compatiblePorts;

            foreach (var port in ports.ToList())
            {
                var endNodeView = port.node as NodeView;
                
                // 기본 조건: 반대 방향이고 다른 노드여야 함
                if (port.direction == startPort.direction || port.node == startPort.node)
                    continue;
                
                if (endNodeView == null)
                    continue;
                
                // Output 포트에서 시작하는 경우 (상위 -> 하위 연결)
                if (startPort.direction == Direction.Output)
                {
                    if (IsValidConnection(startNodeView.Node, endNodeView.Node))
                    {
                        compatiblePorts.Add(port);
                    }
                }
                // Input 포트에서 시작하는 경우 (하위 -> 상위 연결)
                else
                {
                    if (IsValidConnection(endNodeView.Node, startNodeView.Node))
                    {
                        compatiblePorts.Add(port);
                    }
                }
            }
            
            return compatiblePorts;
        }
        
        void OnNodeRootSet(int newRootNodeID)
        {
            var newRootNode = _action.FindNode(newRootNodeID);
            
            // 루트 노드는 Motion만 가능
            if (newRootNode == null || !(newRootNode is MotionNode motionNode) || !motionNode.IsValid)
            {
                Debug.LogWarning("루트 노드는 Motion만 설정할 수 있습니다.");
                return;
            }
            
            _action.rootID = newRootNodeID;
            
            // 새 루트 노드를 부모로 가진 연결들 제거
            _action.nodes.ForEach(node => DisconnectRootParentEdge(node, newRootNodeID));
            
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            DrawNode();
            DrawEdge();
        }

        void DisconnectRootParentEdge(BaseNode node, int newRootNodeID)
        {
            if (node.childrenID.Contains(newRootNodeID))
                node.childrenID.Remove(newRootNodeID);
        }
    }
}
