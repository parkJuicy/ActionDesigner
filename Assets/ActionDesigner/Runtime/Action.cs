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
        [SerializeReference]
        public List<BaseNode> nodes = new List<BaseNode>();

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

        public BaseNode CreateNode(string type, string namespaceType, string baseType, Vector2 position)
        {
            BaseNode node;
            
            if (baseType == "Behavior")
            {
                node = new BehaviorNode();
            }
            else if (baseType == "Condition")
            {
                node = new ConditionNode();
            }
            else
            {
                Debug.LogError($"Unknown base type: {baseType}");
                return null;
            }
            
            node.type = type;
            node.nameSpace = namespaceType;
            node.id = GUID.Generate().GetHashCode();
            node.position = position;
            node.CreateNodeObject();
            
            nodes.Add(node);
            return node;
        }
        
        public void DeleteNode(BaseNode node)
        {
            if (node.id == rootID)
            {
                rootID = 0;
            }

            foreach (var n in nodes)
            {
                n.childrenID.Remove(node.id);
            }

            nodes.Remove(node);
        }

        public void AddChild(BaseNode parent, BaseNode child)
        {
            if (!parent.childrenID.Contains(child.id))
            {
                parent.childrenID.Add(child.id);
            }
        }

        public void RemoveChild(BaseNode parent, BaseNode child)
        {
            parent.childrenID.Remove(child.id);
        }

        public BaseNode FindNode(int id)
        {
            return nodes.Find(n => n.id == id);
        }

        public BaseNode GetRootNode()
        {
            return FindNode(rootID);
        }
    }
}
