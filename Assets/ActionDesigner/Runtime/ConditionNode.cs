using System;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Condition을 담는 노드 - type/namespace 기반으로 Condition 객체 생성
    /// </summary>
    [Serializable]
    public class ConditionNode : BaseNode
    {
        [SerializeReference, SubclassSelector]
        public ICondition condition;

        public override string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(title))
            {
                return title;
            }
            else if (condition != null)
            {
                return UnityEditor.ObjectNames.NicifyVariableName(condition.GetType().Name);
            }
            else if (!string.IsNullOrEmpty(type))
            {
                return UnityEditor.ObjectNames.NicifyVariableName(type);
            }
            return "Empty Condition";
        }

        public override string GetNodeType()
        {
            return "Condition";
        }

        public override bool CanAddChild()
        {
            // Condition은 체인 구조에서 하나의 자식만 가능
            return childrenID.Count == 0;
        }

        public override object GetNodeObject()
        {
            return condition;
        }

        public override void CreateNodeObject()
        {
            if (string.IsNullOrEmpty(type)) return;

            var operationType = Action.GetOperationType(nameSpace, type);
            if (operationType != null && typeof(ICondition).IsAssignableFrom(operationType))
            {
                condition = Activator.CreateInstance(operationType) as ICondition;
            }
        }

        /// <summary>
        /// Condition이 유효한지 확인
        /// </summary>
        public bool IsValid => condition != null;
    }
}
