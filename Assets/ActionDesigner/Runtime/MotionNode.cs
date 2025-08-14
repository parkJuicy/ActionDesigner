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
        public IMotion motion;

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
            if (operationType != null && typeof(IMotion).IsAssignableFrom(operationType))
            {
                motion = Activator.CreateInstance(operationType) as IMotion;
            }
        }
        
        public bool IsValid => motion != null;
    }
}
