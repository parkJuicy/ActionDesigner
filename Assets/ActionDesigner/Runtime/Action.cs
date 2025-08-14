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
            
            // 노드 타입에 따라 적절한 노드 생성
            if (baseType == "Motion")
            {
                node = new MotionNode();
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
            
            // 기본 정보 설정
            node.type = type;
            node.nameSpace = namespaceType;
            node.id = GUID.Generate().GetHashCode();
            node.position = position;
            
            // type/namespace 기반으로 실제 객체 생성
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

            // 이 노드를 참조하는 모든 연결 제거
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
