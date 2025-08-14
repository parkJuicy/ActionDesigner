using ActionDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Motion = ActionDesigner.Runtime.Motion;
using Condition = ActionDesigner.Runtime.Condition;

namespace ActionDesigner.Editor
{
    /// <summary>
    /// 스페이스바로 열리는 노드 검색 창
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ActionView _actionView;
        private Vector2 _mousePosition;

        public static void Open(ActionView actionView, Vector2 mousePosition)
        {
            var searchWindow = CreateInstance<NodeSearchWindow>();
            searchWindow._actionView = actionView;
            searchWindow._mousePosition = mousePosition;
            
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(mousePosition)), searchWindow);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // Motion 노드들 추가
            AddMotionNodes(tree);
            
            // Condition 노드들 추가  
            AddConditionNodes(tree);

            return tree;
        }

        private void AddMotionNodes(List<SearchTreeEntry> tree)
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Motion"), 1));

            var motionTypes = TypeCache.GetTypesDerivedFrom<Motion>();
            var namespaceGroups = new Dictionary<string, List<Type>>();

            // Namespace별로 그룹화
            foreach (var type in motionTypes)
            {
                if (type.IsAbstract) continue;

                string namespaceKey = GetNamespaceDisplayName(type.Namespace);
                if (!namespaceGroups.ContainsKey(namespaceKey))
                {
                    namespaceGroups[namespaceKey] = new List<Type>();
                }
                namespaceGroups[namespaceKey].Add(type);
            }

            // 각 namespace 그룹별로 항목 추가
            foreach (var kvp in namespaceGroups)
            {
                if (kvp.Value.Count > 1)
                {
                    // 여러 타입이 있으면 그룹으로 만들기
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(kvp.Key), 2));
                    foreach (var type in kvp.Value)
                    {
                        tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
                        {
                            level = 3,
                            userData = new NodeCreationData
                            {
                                Type = type,
                                BaseType = "Motion"
                            }
                        });
                    }
                }
                else
                {
                    // 단일 타입이면 바로 추가
                    var type = kvp.Value[0];
                    string displayName = string.IsNullOrEmpty(kvp.Key) ? type.Name : $"{kvp.Key}/{type.Name}";
                    tree.Add(new SearchTreeEntry(new GUIContent(displayName))
                    {
                        level = 2,
                        userData = new NodeCreationData
                        {
                            Type = type,
                            BaseType = "Motion"
                        }
                    });
                }
            }
        }

        private void AddConditionNodes(List<SearchTreeEntry> tree)
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Condition"), 1));

            var conditionTypes = TypeCache.GetTypesDerivedFrom<Condition>();
            var namespaceGroups = new Dictionary<string, List<Type>>();

            // Namespace별로 그룹화
            foreach (var type in conditionTypes)
            {
                if (type.IsAbstract) continue;

                string namespaceKey = GetNamespaceDisplayName(type.Namespace);
                if (!namespaceGroups.ContainsKey(namespaceKey))
                {
                    namespaceGroups[namespaceKey] = new List<Type>();
                }
                namespaceGroups[namespaceKey].Add(type);
            }

            // 각 namespace 그룹별로 항목 추가
            foreach (var kvp in namespaceGroups)
            {
                if (kvp.Value.Count > 1)
                {
                    // 여러 타입이 있으면 그룹으로 만들기
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(kvp.Key), 2));
                    foreach (var type in kvp.Value)
                    {
                        tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
                        {
                            level = 3,
                            userData = new NodeCreationData
                            {
                                Type = type,
                                BaseType = "Condition"
                            }
                        });
                    }
                }
                else
                {
                    // 단일 타입이면 바로 추가
                    var type = kvp.Value[0];
                    string displayName = string.IsNullOrEmpty(kvp.Key) ? type.Name : $"{kvp.Key}/{type.Name}";
                    tree.Add(new SearchTreeEntry(new GUIContent(displayName))
                    {
                        level = 2,
                        userData = new NodeCreationData
                        {
                            Type = type,
                            BaseType = "Condition"
                        }
                    });
                }
            }
        }

        private string GetNamespaceDisplayName(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return "Default";
                
            // "ActionDesigner.Runtime.Tasks" -> "Tasks"
            if (namespaceName.StartsWith("ActionDesigner.Runtime."))
            {
                return namespaceName.Substring("ActionDesigner.Runtime.".Length);
            }
            
            return namespaceName;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData is NodeCreationData nodeData)
            {
                _actionView.CreateNodeAtPosition(nodeData.Type, nodeData.BaseType, _mousePosition);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 노드 생성 데이터
        /// </summary>
        private class NodeCreationData
        {
            public Type Type;
            public string BaseType;
        }
    }
}
