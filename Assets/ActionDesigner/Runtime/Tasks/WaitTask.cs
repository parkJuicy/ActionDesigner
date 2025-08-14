using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class WaitTask : Motion
    {
        [SerializeField]
        public float waitTime = 1.0f;
        
        [SerializeField]
        public bool useUnscaledTime = false;
        
        [SerializeField]
        public bool randomizeTime = false;
        
        [SerializeField]
        public Vector2 randomTimeRange = new Vector2(0.5f, 2.0f);

        private float _startTime;
        private float _actualWaitTime;

        public override void Initialize(ActionRunner runner)
        {
            _startTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            
            if (randomizeTime)
            {
                _actualWaitTime = Random.Range(randomTimeRange.x, randomTimeRange.y);
            }
            else
            {
                _actualWaitTime = waitTime;
            }
        }

        public override bool Update(ActionRunner runner)
        {
            float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            float elapsedTime = currentTime - _startTime;
            
            return elapsedTime >= _actualWaitTime;
        }

        public override float GetProgress()
        {
            float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            float elapsedTime = currentTime - _startTime;
            return Mathf.Clamp01(elapsedTime / _actualWaitTime);
        }

        public override string GetDescription()
        {
            if (randomizeTime)
            {
                return $"Wait {randomTimeRange.x}-{randomTimeRange.y}s";
            }
            return $"Wait {waitTime}s";
        }
    }
}
