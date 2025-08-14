using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class WaitTask : IMotion
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

        public void Start()
        {
            Debug.Log("웨이트 스타또");
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

        public bool Update()
        {
            float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            float elapsedTime = currentTime - _startTime;
            
            return elapsedTime >= _actualWaitTime;
        }
        
        public void End()
        {
            Debug.Log("웨이트 엔드");
        }

        public void Stop()
        {
            Debug.Log("웨이트 스탑");
        }
    }
}
