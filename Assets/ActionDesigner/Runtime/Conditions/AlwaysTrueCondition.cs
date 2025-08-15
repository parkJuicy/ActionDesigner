using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class AlwaysTrueCondition : ICondition
    {
        public bool Evaluate(float deltaTime)
        {
            return true;
        }
        
        public void OnSuccess()
        {
            Debug.Log("ALWAYS OnSuccess - 항상 True 조건으로 인한 전환!");
        }
    }
}
