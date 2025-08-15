using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class DebugLogTask : IMotion
    {
        [SerializeField]
        public string message = "Hello World!";
        
        [SerializeField]
        public float delay = 0f;

        private bool _executed = false;
        private float _startTime;

        public void Start()
        {
            _executed = false;
            _startTime = Time.time;
        }

        public bool Update(float deltaTime)
        {
            if (_executed) return true;

            // 딜레이가 있다면 대기
            if (delay > 0 && Time.time - _startTime < delay)
            {
                return false;
            }

            Debug.Log(message);
            _executed = true;
            return true;
        }
        
        public void End()
        {
            Debug.Log("디버그 엔드");
        }

        public void Stop()
        {
            Debug.Log("디버그 스탑");
        }
    }
}
