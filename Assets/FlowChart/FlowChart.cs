using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JuicyFlowChart
{
    [CreateAssetMenu()]
    public class FlowChart : ScriptableObject
    {
        [HideInInspector, SerializeField]
        private int _rootID;

        [HideInInspector, SerializeField]
        private List<Node> _nodes = new List<Node>();

        public int RootID { get => _rootID; internal set => _rootID = value; }
        public List<Node> Nodes { get => _nodes; internal set => _nodes = value; }

        private static Dictionary<string, Type> _nodeTypes = new Dictionary<string, Type>();

        public static Type GetNodeType(string namespaceType, string key)
        {
            Type type;
            if (!string.IsNullOrEmpty(namespaceType))
            {
                key = $"{namespaceType}.{key}";
            }

            if (!_nodeTypes.TryGetValue(key, out type))
            {
                type = Type.GetType(key);
                _nodeTypes[key] = type;
            }

            if(type == null)
            {
                Debug.LogError($"Invalid Type : <color=red>{key}</color>");
                _nodeTypes.Remove(key);
            }

            return type;
        }

        public Node CreateNode(string type, string namespaceType, string baseType, Vector2 position)
        {
            Node node = new Node();
            node.Name = type;
            node.Namespace = namespaceType;
            node.BaseType = baseType;
            node.ID = GUID.Generate().GetHashCode();
            node.Position = position;

            var instance = Activator.CreateInstance(FlowChart.GetNodeType(namespaceType, type));
            node.Data = JsonUtility.ToJson(instance);
            _nodes.Add(node);
            EditorUtility.SetDirty(this);
            return node;
        }

        public Node CreateRootNode(Node childNode)
        {
            Node root = new Node();
            root.Name = "Root";
            root.Namespace = "JuicyFlowChart";
            root.BaseType = root.Name;
            root.ID = GUID.Generate().GetHashCode();
            root.Position = childNode.Position + new Vector2(0, -150f);
            AddChild(root, childNode);

            var instance = Activator.CreateInstance(FlowChart.GetNodeType("JuicyFlowChart", "Root"));
            root.Data = JsonUtility.ToJson(instance);
            _rootID = root.ID;
            _nodes.Add(root);
            EditorUtility.SetDirty(this);
            return root;
        }

        public void DeleteNode(Node node)
        {
            if (node.ID == _rootID)
            {
                _rootID = 0;
            }

            _nodes.Remove(node);
            EditorUtility.SetDirty(this);
        }

        public void AddChild(Node parent, Node child)
        {
            parent.ChildrenID.Add(child.ID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(Node parent, Node child)
        {
            parent.ChildrenID.Remove(child.ID);
            EditorUtility.SetDirty(this);
        }

        #region Runtime
        public Task Clone(GameObject gameObject)
        {
            Node rootNode = _nodes.Find(x => x.ID == _rootID);
            Task rootTask = (Task)JsonUtility.FromJson(rootNode.Data, GetNodeType(rootNode.Namespace, rootNode.Name));
            rootTask.SetGameObject(gameObject);
            rootTask.NodeID = rootNode.ID;
            Traverse(rootNode, rootTask, gameObject);
            return rootTask;
        }

        public void Traverse(Node node, Task task, GameObject gameObject)
        {
            if (node != null)
            {
                List<int> childrenID = node.ChildrenID;
                childrenID.ForEach((nodeID) =>
                {
                    Node targetNode = _nodes.Find(x => x.ID == nodeID);
                    Type targetType = GetNodeType(targetNode.Namespace, targetNode.Name);
                    if(targetType == null)
                        return;

                    Task targetTask = (Task)JsonUtility.FromJson(targetNode.Data, targetType);
                    targetTask.NodeID = targetNode.ID;
                    targetTask.SetGameObject(gameObject);

                    task.Children.Add(targetTask);
                    Traverse(targetNode, targetTask, gameObject);
                });
            }
        }
        #endregion
    }
}