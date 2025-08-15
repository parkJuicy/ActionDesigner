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

        public void Start()
        {
            Debug.Log("END START");
        }

        public void End()
        {
            Debug.Log("END End");
        }

        public void OnSuccess()
        {
            Debug.Log("END OnSuccess - Motion 완료로 인한 전환!");
        }
    }
}
