using UnityEngine;

namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class KeyPress : ICondition
    {
        [SerializeField] KeyCode keyCode = KeyCode.Space;
        [SerializeField] bool requireKeyDown = true;

        public bool Evaluate(float deltaTime)
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
    }
}
