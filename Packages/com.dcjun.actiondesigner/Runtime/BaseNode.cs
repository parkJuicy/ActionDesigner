using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// 모든 노드의 기본 클래스 - type/namespace 기반으로 런타임에 객체 생성
    /// </summary>
    [Serializable]
    public abstract class BaseNode
    {
        [HideInInspector]
        public string type;
        [HideInInspector]
        public string nameSpace;
        [HideInInspector]
        public int id;
        [HideInInspector]
        public Vector2 position;
        [HideInInspector]
        public List<int> childrenID = new List<int>();
        public string title;

        /// <summary>
        /// 노드의 표시 이름 반환
        /// </summary>
        public abstract string GetDisplayName();

        /// <summary>
        /// 노드 타입 반환 ("Behavior" 또는 "Condition")
        /// </summary>
        public abstract string GetNodeType();

        /// <summary>
        /// 노드가 자식을 추가할 수 있는지 확인
        /// </summary>
        public virtual bool CanAddChild()
        {
            return true;
        }

        /// <summary>
        /// 노드에 할당된 객체 반환 (Behavior 또는 Condition)
        /// </summary>
        public abstract object GetNodeObject();

        /// <summary>
        /// type과 namespace로부터 실제 객체 생성
        /// </summary>
        public abstract void CreateNodeObject();
    }
}