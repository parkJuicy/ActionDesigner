using ActionDesigner.Runtime;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Motion = ActionDesigner.Runtime.Motion;
using Condition = ActionDesigner.Runtime.Condition;

namespace ActionDesigner.Editor
{
    public static class TaskTypeDebugger
    {
        [MenuItem("Action Designer/Debug Motion Types")]
        public static void DebugMotionTypes()
        {
            var baseType = typeof(Motion);
            DebugTypes("Motion", baseType);
        }

        [MenuItem("Action Designer/Debug Condition Types")]
        public static void DebugConditionTypes()
        {
            var baseType = typeof(Condition);
            DebugTypes("Condition", baseType);
        }

        private static void DebugTypes(string typeName, Type baseType)
        {
            var foundTypes = new System.Collections.Generic.List<Type>();

            Debug.Log($"=== {typeName} Type Debugging ===");
            Debug.Log($"Base type: {baseType.FullName}");

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        // baseType을 상속받는 모든 타입 찾기
                        if (baseType.IsAssignableFrom(type) && type != baseType)
                        {
                            Debug.Log($"Found type inheriting from {typeName}: {type.FullName}");
                            Debug.Log($"  - IsAbstract: {type.IsAbstract}");
                            Debug.Log($"  - IsSerializable: {type.IsSerializable}");
                            Debug.Log($"  - IsPublic: {type.IsPublic}");
                            Debug.Log($"  - IsNestedPublic: {type.IsNestedPublic}");

                            // 조건 확인
                            bool meetsConditions = !type.IsAbstract && 
                                                 type.IsSerializable &&
                                                 (type.IsPublic || type.IsNestedPublic);

                            Debug.Log($"  - Meets conditions: {meetsConditions}");

                            if (meetsConditions)
                            {
                                foundTypes.Add(type);
                            }
                            Debug.Log("---");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to scan assembly {assembly.GetName().Name}: {e.Message}");
                }
            }

            Debug.Log($"Total valid {typeName} types found: {foundTypes.Count}");
            foreach (var type in foundTypes.OrderBy(t => t.Name))
            {
                Debug.Log($"  ✓ {type.FullName}");
            }
        }

        [MenuItem("Action Designer/Check Node Types")]
        public static void CheckNodeTypes()
        {
            Debug.Log("=== Node Type Check ===");
            
            var motionType = typeof(Motion);
            var conditionType = typeof(Condition);
            
            Debug.Log($"Motion type: {motionType.FullName}");
            Debug.Log($"Condition type: {conditionType.FullName}");
            
            // Motion 하위 타입들 체크
            var motionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .Where(type => motionType.IsAssignableFrom(type) && !type.IsAbstract)
                .ToArray();
                
            Debug.Log($"Found {motionTypes.Length} Motion types:");
            foreach (var type in motionTypes)
            {
                Debug.Log($"  - {type.FullName}");
            }
            
            // Condition 하위 타입들 체크
            var conditionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .Where(type => conditionType.IsAssignableFrom(type) && !type.IsAbstract)
                .ToArray();
                
            Debug.Log($"Found {conditionTypes.Length} Condition types:");
            foreach (var type in conditionTypes)
            {
                Debug.Log($"  - {type.FullName}");
            }
        }
    }
}
