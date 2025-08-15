using UnityEngine;

namespace ActionDesigner.Runtime
{
    public interface ICondition
    {
        public bool Evaluate(float deltaTime);
        public void Start() { }
        public void End() { }
        public void OnSuccess() { }
    }
}
