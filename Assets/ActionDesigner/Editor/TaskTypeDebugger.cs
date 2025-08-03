using ActionDesigner.Runtime;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ActionDesigner.Editor
{
    public static class TaskTypeDebugger
    {
        [MenuItem("Action Designer/Debug Task Types")]
        public static void DebugTaskTypes()
        {
            var baseType = typeof(Task);
            var foundTypes = new System.Collections.Generic.List<Type>();

            Debug.Log("=== Task Type Debugging ===");
            Debug.Log($"Base type: {baseType.FullName}");

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        // Task를 상속받는 모든 타입 찾기
                        if (baseType.IsAssignableFrom(type) && type != baseType)
                        {
                            Debug.Log($"Found type inheriting from Task: {type.FullName}");
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

            Debug.Log($"Total valid Task types found: {foundTypes.Count}");
            foreach (var type in foundTypes.OrderBy(t => t.Name))
            {
                Debug.Log($"  ✓ {type.FullName}");
            }
        }

        [MenuItem("Action Designer/Check TNewTask")]
        public static void CheckTNewTask()
        {
            var tNewTaskType = Type.GetType("deep.TNewTask");
            if (tNewTaskType == null)
            {
                Debug.LogError("TNewTask type not found! Make sure it's compiled properly.");
                return;
            }

            Debug.Log($"TNewTask found: {tNewTaskType.FullName}");
            Debug.Log($"  - Base type: {tNewTaskType.BaseType?.FullName}");
            Debug.Log($"  - IsAbstract: {tNewTaskType.IsAbstract}");
            Debug.Log($"  - IsSerializable: {tNewTaskType.IsSerializable}");
            Debug.Log($"  - IsPublic: {tNewTaskType.IsPublic}");

            var taskType = typeof(Task);
            Debug.Log($"  - Is assignable from Task: {taskType.IsAssignableFrom(tNewTaskType)}");

            var transitionType = typeof(Transition);
            Debug.Log($"  - Is assignable from Transition: {transitionType.IsAssignableFrom(tNewTaskType)}");
        }
    }
}
