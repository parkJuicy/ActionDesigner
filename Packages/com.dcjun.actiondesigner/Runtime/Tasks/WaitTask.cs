using UnityEngine;

namespace ActionDesigner.Runtime
{
    [System.Serializable]
    public sealed class WaitTask : IBehavior
    {
        [SerializeField] float waitTime = 1.0f;
        float elapsedTime;

        public void Start()
        {
            elapsedTime = 0;
        }

        public bool Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= waitTime)
            {
                return true;
            }
            return false;
        }
    }
}