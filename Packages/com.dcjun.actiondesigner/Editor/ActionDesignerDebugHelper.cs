using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ActionDesigner.Editor
{
    public static class ActionDesignerDebugHelper
    {
        [MenuItem("Action Designer/Debug/List All IBehavior Types")]
        public static void ListAllBehaviorTypes()
        {
            var behaviorTypes = TypeCache.GetTypesDerivedFrom<ActionDesigner.Runtime.IBehavior>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.FullName)
                .ToArray();

            Debug.Log($"=== 발견된 IBehavior 타입들 ({behaviorTypes.Length}개) ===");
            foreach (var type in behaviorTypes)
            {
                Debug.Log($"✓ {type.FullName} (Assembly: {type.Assembly.GetName().Name})");
            }

            if (behaviorTypes.Length == 0)
            {
                Debug.LogWarning("IBehavior를 구현한 타입을 찾을 수 없습니다. 다음을 확인해주세요:\n" +
                    "1. IBehavior 인터페이스를 구현했는지\n" +
                    "2. [System.Serializable] 속성이 있는지\n" +
                    "3. 클래스가 abstract가 아닌지");
            }
        }

        [MenuItem("Action Designer/Debug/List All ICondition Types")]
        public static void ListAllConditionTypes()
        {
            var conditionTypes = TypeCache.GetTypesDerivedFrom<ActionDesigner.Runtime.ICondition>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.FullName)
                .ToArray();

            Debug.Log($"=== 발견된 ICondition 타입들 ({conditionTypes.Length}개) ===");
            foreach (var type in conditionTypes)
            {
                Debug.Log($"✓ {type.FullName} (Assembly: {type.Assembly.GetName().Name})");
            }

            if (conditionTypes.Length == 0)
            {
                Debug.LogWarning("ICondition을 구현한 타입을 찾을 수 없습니다. 다음을 확인해주세요:\n" +
                    "1. ICondition 인터페이스를 구현했는지\n" +
                    "2. [System.Serializable] 속성이 있는지\n" +
                    "3. 클래스가 abstract가 아닌지");
            }
        }

        [MenuItem("Action Designer/Debug/List All Loaded Assemblies")]
        public static void ListAllLoadedAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(a => a.GetName().Name)
                .ToArray();

            Debug.Log($"=== 로드된 어셈블리들 ({assemblies.Length}개) ===");
            foreach (var assembly in assemblies)
            {
                try
                {
                    var name = assembly.GetName().Name;
                    var location = assembly.IsDynamic ? "Dynamic" : assembly.Location;
                    Debug.Log($"✓ {name} - {location}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"어셈블리 정보 읽기 실패: {ex.Message}");
                }
            }
        }

        [MenuItem("Action Designer/Debug/Test Type Resolution")]
        public static void TestTypeResolution()
        {
            Debug.Log("=== 타입 해결 테스트 ===");
            
            // 테스트할 타입명들
            string[] testTypes = {
                "DebugLogTask",
                "ActionDesigner.Runtime.WaitTask",
                "WaitTask",
                "SimpleDebugLogTask"
            };

            foreach (var typeName in testTypes)
            {
                var type = ActionDesigner.Runtime.Action.GetOperationType("", typeName);
                if (type != null)
                {
                    Debug.Log($"✓ '{typeName}' → {type.FullName}");
                }
                else
                {
                    Debug.LogError($"✗ '{typeName}' → 찾을 수 없음");
                }
            }
        }

        [MenuItem("Action Designer/Debug/Clear Type Cache")]
        public static void ClearTypeCache()
        {
            // Reflection을 통해 private static field에 접근하여 캐시 초기화
            var actionType = typeof(ActionDesigner.Runtime.Action);
            var cacheField = actionType.GetField("_operationTypes", BindingFlags.NonPublic | BindingFlags.Static);
            
            if (cacheField != null)
            {
                var cache = cacheField.GetValue(null) as System.Collections.IDictionary;
                cache?.Clear();
                Debug.Log("타입 캐시를 초기화했습니다.");
            }
            else
            {
                Debug.LogWarning("타입 캐시 필드를 찾을 수 없습니다.");
            }
        }
    }
}