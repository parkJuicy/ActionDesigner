using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    [Serializable]
    public class Action
    {
        public int rootID;
        public List<Node> nodes = new List<Node>();

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
            node.task = instance as Task;
            node.baseType = baseType;
            
            nodes.Add(node);
            return node;
        }
        
        public void DeleteNode(Node node)
        {
            if (node.id == rootID)
            {
                rootID = 0;
            }

            nodes.Remove(node);
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