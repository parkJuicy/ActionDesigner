using JuicyFlowChart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    [Serializable]
    public class Action
    {
        [HideInInspector, SerializeField]
        int _rootID;
        [HideInInspector, SerializeField]
        List<Node> _nodes = new List<Node>();

        public int rootID { get => _rootID; internal set => _rootID = value; }
        public List<Node> nodes { get => _nodes; internal set => _nodes = value; }

        static Dictionary<string, Type> _operationTypes = new Dictionary<string, Type>();

        public static Type GetOperationType(string namespaceType, string key)
        {
            Type type;
            if (!string.IsNullOrEmpty(namespaceType))
            {
                key = $"{namespaceType}.{key}";
            }

            if (!_operationTypes.TryGetValue(key, out type))
            {
                type = Type.GetType(key);
                _operationTypes[key] = type;
            }

            if (type == null)
            {
                Debug.LogError($"Invalid Type : <color=red>{key}</color>");
                _operationTypes.Remove(key);
            }

            return type;
        }

        public Node CreateNode(string type, string namespaceType, string baseType, Vector2 position)
        {
            Node node = new Node();
            node.type = type;
            node.nameSpace = namespaceType;
            node.id = GUID.Generate().GetHashCode();
            node.position = position;

            var instance = Activator.CreateInstance(GetOperationType(namespaceType, type));
            node.operation = instance as Operation;

            _nodes.Add(node);
            return node;
        }

        public Node CreateRootNode(Node childNode)
        {
            Node root = new Node();
            root.type = "Root";
            root.nameSpace = "ActionDesigner.Runtime";
            root.id = GUID.Generate().GetHashCode();
            root.position = childNode.position + new Vector2(0, -150f);
            AddChild(root, childNode);

            var instance = Activator.CreateInstance(GetOperationType("ActionDesigner.Runtime", "Root"));

            _rootID = root.id;
            _nodes.Add(root);
            return root;
        }

        public void DeleteNode(Node node)
        {
            if (node.id == _rootID)
            {
                _rootID = 0;
            }

            _nodes.Remove(node);
        }

        public void AddChild(Node parent, Node child)
        {
            parent.childrenID.Add(child.id);
        }

        public void RemoveChild(Node parent, Node child)
        {
            parent.childrenID.Remove(child.id);
        }
    }
}