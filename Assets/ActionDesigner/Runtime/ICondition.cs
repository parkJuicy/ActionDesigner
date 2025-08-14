using UnityEngine;

namespace ActionDesigner.Runtime
{
    public interface ICondition
    {
        public bool Evaluate();
        public void Start() { }
        public void End() { }
        public void OnSuccess() { }
    }
}
