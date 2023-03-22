using ActionDesigner.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    public class ActionView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<ActionView, GraphView.UxmlTraits> { }
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
            NodeView nodeView = new NodeView(node, node.id == _action.rootID);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
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

            // Create Edge
            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    _action.AddChild(parentView.Node, childView.Node);
                });
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
            ShowNodeTypes<Operation>(evt);
        }

        void ShowNodeTypes<T>(ContextualMenuPopulateEvent evt) where T : Operation
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
                    CreateNodeView(node);

                    if (_action.rootID == 0)
                    {
                        Runtime.Node root = _action.CreateRootNode(node);
                        CreateNodeView(root);
                        DrawEdge();
                    }

                    EditorUtility.SetDirty(_actionRunner);
                });
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (Application.isPlaying)
            {
                List<Port> emptyPort = new List<Port>();
                return emptyPort;
            }

            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }
    }
}