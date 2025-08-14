using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class KeyPressCondition : Condition
    {
        [SerializeField]
        public KeyCode keyCode = KeyCode.Space;
        
        [SerializeField]
        public bool requireKeyDown = true; // true: GetKeyDown, false: GetKey

        public override bool Evaluate(ActionRunner runner)
        {
            if (requireKeyDown)
            {
                return Input.GetKeyDown(keyCode);
            }
            else
            {
                return Input.GetKey(keyCode);
            }
        }

        public override bool IsWaitCondition => requireKeyDown; // KeyDown은 매 프레임 체크 필요

        public override string GetDescription()
        {
            string action = requireKeyDown ? "Press" : "Hold";
            return $"{action} {keyCode}";
        }
    }
}
