using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class KeyPressCondition : ICondition
    {
        [SerializeField]
        public KeyCode keyCode = KeyCode.Space;
        
        [SerializeField]
        public bool requireKeyDown = true;

        public bool Evaluate()
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
        
        public void OnSuccess()
        {
            Debug.Log($"KEY OnSuccess - {keyCode} 키로 인한 전환!");
        }
    }
}
