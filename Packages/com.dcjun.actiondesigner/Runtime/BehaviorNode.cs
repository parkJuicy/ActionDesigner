using System;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Behavior을 담는 노드 - type/namespace 기반으로 Behavior 객체 생성
    /// </summary>
    [Serializable]
    public class BehaviorNode : BaseNode
    {
        [SerializeReference, SubclassSelector]
        public IBehavior behavior;

        public override string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(title))
            {
                return title;
            }
            else if (behavior != null)
            {
#if UNITY_EDITOR
                return UnityEditor.ObjectNames.NicifyVariableName(behavior.GetType().Name);
#else
                return behavior.GetType().Name;
#endif
            }
            return "Empty Behavior";
        }

        public override string GetNodeType()
        {
            return "Behavior";
        }

        public override bool CanAddChild()
        {
            return true;
        }

        public override object GetNodeObject()
        {
            return behavior;
        }

        public override void CreateNodeObject(string type, string namespaceType)
        {
            if (string.IsNullOrEmpty(type)) return;

            var operationType = Action.GetOperationType(namespaceType, type);
            if (operationType != null && typeof(IBehavior).IsAssignableFrom(operationType))
            {
                behavior = Activator.CreateInstance(operationType) as IBehavior;
            }
        }
        
        public bool IsValid => behavior != null;
    }
}