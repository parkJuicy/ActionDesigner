using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class WaitTask : Task
    {
        [SerializeField]
        public float waitTime = 1.0f;
        
        [SerializeField]
        public bool useUnscaledTime = false;
        
        [SerializeField]
        public bool randomizeTime = false;
        
        [SerializeField]
        public Vector2 randomTimeRange = new Vector2(0.5f, 2.0f);
    }
}
