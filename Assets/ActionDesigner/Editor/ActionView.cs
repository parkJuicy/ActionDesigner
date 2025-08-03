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
                    nodeView.RefreshTitle();
                }
            }
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

                    Edge edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                });
            });
        }

        NodeView FindNodeView(int nodeID)
        {
            return GetNodeByGuid(nodeID.ToString()) as NodeView;
        }

        void CreateNodeView(Runtime.Node node)
        {
            NodeView nodeView = new NodeView(node, node.id == _action.rootID)
            {
                OnNodeSelected = OnNodeSelected,
                OnNodeRootSet = OnNodeRootSet
            };
            AddElement(nodeView);
        }
        
        private bool ValidateConnection(Runtime.Node parent, Runtime.Node child)
        {
            // Motion은 Transition과만 연결 가능
            if (parent.task is Motion && !(child.task is Transition))
            {
                return false;
            }
            
            // Transition은 Motion과만 연결 가능
            if (parent.task is Transition && !(child.task is Motion))
            {
                return false;
            }
            
            // Transition은 하나의 자식만 가질 수 있음
            if (parent.task is Transition && parent.childrenID.Count >= 1)
            {
                return false;
            }
            
            return true;
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
                    if (ValidateConnection(parentView.Node, childView.Node))
                    {
                        _action.AddChild(parentView.Node, childView.Node);
                        validEdges.Add(edge);
                    }
                    else
                    {
                        // 잘못된 연결인 경우 경고 메시지
                        string parentType = parentView.Node.task.GetType().Name;
                        string childType = childView.Node.task.GetType().Name;
                        
                        if (parentView.Node.task is Motion && !(childView.Node.task is Transition))
                        {
                            Debug.LogWarning($"Motion은 Transition과만 연결할 수 있습니다: {parentType} -> {childType}");
                        }
                        else if (parentView.Node.task is Transition && !(childView.Node.task is Motion))
                        {
                            Debug.LogWarning($"Transition은 Motion과만 연결할 수 있습니다: {parentType} -> {childType}");
                        }
                        else if (parentView.Node.task is Transition && parentView.Node.childrenID.Count >= 1)
                        {
                            Debug.LogWarning($"Transition은 하나의 자식만 가질 수 있습니다: {parentType}");
                        }
                        
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
                    if (nodeView.Input != null && nodeView.Input.connections != null)
                    {
                        foreach (var parentEdge in nodeView.Input.connections)
                        {
                            if (parentEdge == null)
                                break;

                            var parent = parentEdge.output.node as NodeView;
                            parent.Node.childrenID.Sort(SortByHoriziontalPosition);
                        }
                    }
                });
            }

            EditorUtility.SetDirty(_actionRunner);
            return graphViewChange;
        }

        int SortByHoriziontalPosition(int left, int right)
        {
            return FindNodeView(left).Node.position.x < FindNodeView(right).Node.position.x ? -1 : 1;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_action == null || Application.isPlaying)
                return;

            evt.menu.AppendSeparator();
            ShowNodeTypes<Motion>(evt);
            ShowNodeTypes<Transition>(evt);
        }

        void ShowNodeTypes<T>(ContextualMenuPopulateEvent evt) where T : Task
        {
            VisualElement contentViewContainer = ElementAt(1);
            Vector3 screenMousePosition = evt.localMousePosition;
            Vector2 worldMousePosition = screenMousePosition - contentViewContainer.transform.position;
            worldMousePosition *= 1 / contentViewContainer.transform.scale.x;

            var types = TypeCache.GetTypesDerivedFrom<T>();
            foreach (var type in types)
            {
                string menu;
                if (type.Namespace == null)
                    menu = $"{type.BaseType.Name}/{type.Name}";
                else
                    menu = $"{type.BaseType.Name}/{type.Namespace}/{type.Name}";

                evt.menu.AppendAction(menu, (actionEvent) =>
                {
                    Runtime.Node node = _action.CreateNode(type.Name, type.Namespace, type.BaseType.Name, worldMousePosition);
                    
                    // 루트 노드는 Motion만 가능
                    if (_action.rootID == 0 && typeof(T) == typeof(Motion))
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
                    if (ValidateConnection(startNodeView.Node, endNodeView.Node))
                    {
                        compatiblePorts.Add(port);
                    }
                }
                // Input 포트에서 시작하는 경우 (하위 -> 상위 연결)
                else
                {
                    if (ValidateConnection(endNodeView.Node, startNodeView.Node))
                    {
                        compatiblePorts.Add(port);
                    }
                }
            }
            
            return compatiblePorts;
        }
        
        void OnNodeRootSet(int newRootNodeID)
        {
            _action.rootID = newRootNodeID;
            _action.nodes.ForEach(node => DisconnectRootParentEdge(node, newRootNodeID));
            
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            DrawNode();
            DrawEdge();
        }

        void DisconnectRootParentEdge(Runtime.Node node, int newRootNodeID)
        {
            if (node.childrenID.Contains(newRootNodeID))
                node.childrenID.Remove(newRootNodeID);
        }
    }
}