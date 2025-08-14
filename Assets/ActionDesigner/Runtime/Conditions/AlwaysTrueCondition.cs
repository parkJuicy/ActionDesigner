using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class AlwaysTrueCondition : Condition
    {
        public override bool Evaluate(ActionRunner runner)
        {
            return true; // 항상 조건 만족
        }

        public override string GetDescription()
        {
            return "Always True";
        }
    }
}
