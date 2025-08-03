using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class MoveTransformTask : Task
    {
        [SerializeField]
        public Transform targetTransform;
        
        [SerializeField]
        public Vector3 targetPosition = Vector3.zero;
        
        [SerializeField]
        public bool useLocalPosition = false;
        
        [SerializeField]
        public float moveSpeed = 5.0f;
        
        [SerializeField]
        public AnimationCurve moveCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [SerializeField]
        public bool waitForCompletion = true;
    }
}
