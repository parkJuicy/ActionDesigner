using UnityEngine;

namespace ActionDesigner.Runtime
{
    [System.Serializable]
    public sealed class DebugLogTask : IMotion
    {
        [SerializeField] string message = "Hello World!";

        public bool Update(float deltaTime)
        {
            Debug.Log(message);
            return true;
        }
    }
}
