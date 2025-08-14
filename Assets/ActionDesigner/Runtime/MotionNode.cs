using System;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// Motion을 담는 노드 - type/namespace 기반으로 Motion 객체 생성
    /// </summary>
    [Serializable]
    public class MotionNode : BaseNode
    {
        [SerializeReference, SubclassSelector]
        public Motion motion;

        public override string GetDisplayName()
        {
            if (motion != null)
            {
                return UnityEditor.ObjectNames.NicifyVariableName(motion.GetType().Name);
            }
            else if (!string.IsNullOrEmpty(type))
            {
                return UnityEditor.ObjectNames.NicifyVariableName(type);
            }
            return "Empty Motion";
        }

        public override string GetNodeType()
        {
            return "Motion";
        }

        public override bool CanAddChild()
        {
            // Motion은 여러 자식 가능 (하지만 체인 구조에서는 보통 1개)
            return true;
        }

        public override object GetNodeObject()
        {
            return motion;
        }

        public override void CreateNodeObject()
        {
            if (string.IsNullOrEmpty(type)) return;

            var operationType = Action.GetOperationType(nameSpace, type);
            if (operationType != null && typeof(Motion).IsAssignableFrom(operationType))
            {
                motion = Activator.CreateInstance(operationType) as Motion;
            }
        }

        /// <summary>
        /// Motion이 유효한지 확인
        /// </summary>
        public bool IsValid => motion != null;
    }
}
