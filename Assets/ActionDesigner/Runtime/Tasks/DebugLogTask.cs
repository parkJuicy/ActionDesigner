using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class DebugLogTask : Task
    {
        [SerializeField]
        public string message = "Hello World!";
        
        [SerializeField]
        public bool useCustomColor = false;
        
        [SerializeField]
        public Color logColor = Color.white;
        
        [SerializeField]
        public float delay = 0f;
        
        [SerializeField]
        public bool includeTimestamp = true;
    }
}
