using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            string fullTypeName = key;
            if (!string.IsNullOrEmpty(namespaceType))
            {
                fullTypeName = $"{namespaceType}.{key}";
            }

            if (!_operationTypes.TryGetValue(fullTypeName, out type))
            {
                // 1. 먼저 정확한 타입명으로 시도
                type = Type.GetType(fullTypeName);
                
                // 2. 실패하면 모든 로드된 어셈블리에서 검색
                if (type == null)
                {
                    type = FindTypeInAllAssemblies(fullTypeName);
                }
                
                // 3. 여전히 실패하면 짧은 이름으로만 검색
                if (type == null && !string.IsNullOrEmpty(key))
                {
                    type = FindTypeByShortName(key);
                }
                
                _operationTypes[fullTypeName] = type;
            }

            if (type == null)
            {
                Debug.LogError($"Invalid Type : <color=red>{fullTypeName}</color>");
                Debug.LogWarning($"사용 가능한 타입들을 확인하려면 다음을 시도해보세요:\n" +
                    $"1. 네임스페이스가 정확한지 확인\n" +
                    $"2. Assembly Definition 설정 확인\n" +
                    $"3. 스크립트가 올바르게 컴파일되었는지 확인");
                _operationTypes.Remove(fullTypeName);
            }

            return type;
        }
        
        /// <summary>
        /// 모든 로드된 어셈블리에서 타입 검색
        /// </summary>
        private static Type FindTypeInAllAssemblies(string fullTypeName)
        {
            try
            {
                // 현재 도메인의 모든 어셈블리 검색
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var type = assembly.GetType(fullTypeName);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                    catch (Exception)
                    {
                        // 어셈블리 접근 실패 시 무시하고 계속
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Assembly 검색 중 오류 발생: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// 짧은 이름으로 타입 검색 (네임스페이스 무시)
        /// </summary>
        private static Type FindTypeByShortName(string shortName)
        {
            try
            {
                var matchingTypes = new List<Type>();
                
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => t.Name == shortName && !t.IsAbstract)
                            .ToArray();
                        matchingTypes.AddRange(types);
                    }
                    catch (Exception)
                    {
                        // 어셈블리 접근 실패 시 무시하고 계속
                        continue;
                    }
                }
                
                if (matchingTypes.Count == 1)
                {
                    Debug.Log($"타입 '{shortName}'을 다음에서 찾았습니다: {matchingTypes[0].FullName}");
                    return matchingTypes[0];
                }
                else if (matchingTypes.Count > 1)
                {
                    Debug.LogWarning($"'{shortName}' 이름의 타입이 여러 개 발견되었습니다:\n" +
                        string.Join("\n", matchingTypes.Select(t => t.FullName)));
                    return matchingTypes[0]; // 첫 번째 것 사용
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"타입 검색 중 오류 발생: {ex.Message}");
            }
            
            return null;
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