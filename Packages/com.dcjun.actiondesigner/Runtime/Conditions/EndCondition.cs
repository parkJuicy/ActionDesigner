using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class EndCondition : ICondition
    {
        public bool Evaluate(float deltaTime)
        {
            return true;
        }
    }
}