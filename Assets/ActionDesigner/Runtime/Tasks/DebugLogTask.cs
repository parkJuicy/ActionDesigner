using UnityEngine;

namespace ActionDesigner.Runtime.Tasks
{
    [System.Serializable]
    public class DebugLogTask : Motion
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

        private bool _executed = false;
        private float _startTime;

        public override void Initialize(ActionRunner runner)
        {
            _executed = false;
            _startTime = Time.time;
        }

        public override bool Update(ActionRunner runner)
        {
            if (_executed) return true;

            // 딜레이가 있다면 대기
            if (delay > 0 && Time.time - _startTime < delay)
            {
                return false;
            }

            // 로그 출력
            string logMessage = message;
            
            if (includeTimestamp)
            {
                logMessage = $"[{Time.time:F2}s] {logMessage}";
            }

            if (useCustomColor)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(logColor);
                logMessage = $"<color=#{colorHex}>{logMessage}</color>";
            }

            Debug.Log(logMessage);
            _executed = true;
            return true;
        }

        public override float GetProgress()
        {
            if (delay <= 0) return _executed ? 1.0f : 0.0f;
            
            float elapsed = Time.time - _startTime;
            return Mathf.Clamp01(elapsed / delay);
        }

        public override string GetDescription()
        {
            return $"Log: {message}";
        }
    }
}
