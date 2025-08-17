using UnityEngine;
using ActionDesigner.Runtime;

// 네임스페이스 없이 사용하는 예제
[System.Serializable]
public class SimpleDebugLogTask : IBehavior
{
    [SerializeField] private string message = "Hello from Action Designer!";
    [SerializeField] private bool completeImmediately = true;
    
    public void Start()
    {
        Debug.Log($"[SimpleDebugLogTask] Started: {message}");
    }
    
    public bool Update(float deltaTime)
    {
        return completeImmediately;
    }
    
    public void End()
    {
        Debug.Log($"[SimpleDebugLogTask] Completed: {message}");
    }
    
    public void Stop()
    {
        Debug.Log($"[SimpleDebugLogTask] Stopped: {message}");
    }
}

// 네임스페이스를 사용하는 예제
namespace ActionDesigner.Samples
{
    /// <summary>
    /// 키보드 입력을 감지하는 조건
    /// </summary>
    [System.Serializable]
    public class KeyPressCondition : ICondition
    {
        [SerializeField] private KeyCode keyCode = KeyCode.Space;
        
        public bool Evaluate(float deltaTime)
        {
            return Input.GetKeyDown(keyCode);
        }
        
        public void Start() { }
        public void End() { }
        public void OnSuccess() 
        {
            Debug.Log($"{keyCode} key pressed!");
        }
    }

    /// <summary>
    /// 항상 참을 반환하는 조건 (테스트용)
    /// </summary>
    [System.Serializable]
    public class AlwaysTrueCondition : ICondition
    {
        public bool Evaluate(float deltaTime)
        {
            return true;
        }
        
        public void Start() { }
        public void End() { }
        public void OnSuccess() { }
    }

    /// <summary>
    /// 디버그 로그를 출력하는 Behavior
    /// </summary>
    [System.Serializable]
    public class DebugLogTask : IBehavior
    {
        [SerializeField] private string message = "Hello from Action Designer!";
        [SerializeField] private bool completeImmediately = true;
        
        public void Start()
        {
            Debug.Log($"[DebugLogTask] Started: {message}");
        }
        
        public bool Update(float deltaTime)
        {
            return completeImmediately;
        }
        
        public void End()
        {
            Debug.Log($"[DebugLogTask] Completed: {message}");
        }
        
        public void Stop()
        {
            Debug.Log($"[DebugLogTask] Stopped: {message}");
        }
    }

    /// <summary>
    /// 오브젝트를 회전시키는 Behavior
    /// </summary>
    [System.Serializable]
    public class RotateBehavior : IBehavior
    {
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90, 0);
        [SerializeField] private float duration = 2f;
        
        private Transform targetTransform;
        private float elapsedTime;
        
        public void Start()
        {
            // ActionRunner의 Transform 사용
            var actionRunner = Object.FindObjectOfType<ActionRunner>();
            if (actionRunner != null)
            {
                targetTransform = actionRunner.transform;
            }
            elapsedTime = 0f;
        }
        
        public bool Update(float deltaTime)
        {
            if (targetTransform != null)
            {
                targetTransform.Rotate(rotationSpeed * deltaTime);
            }
            
            elapsedTime += deltaTime;
            return elapsedTime >= duration;
        }
        
        public void End() { }
        public void Stop() { }
    }

    /// <summary>
    /// 랜덤 확률로 성공하는 조건
    /// </summary>
    [System.Serializable]
    public class RandomChanceCondition : ICondition
    {
        [SerializeField] [Range(0f, 1f)] private float successChance = 0.5f;
        
        public bool Evaluate(float deltaTime)
        {
            return Random.value < successChance;
        }
        
        public void Start() { }
        public void End() { }
        public void OnSuccess() 
        {
            Debug.Log($"Random chance succeeded! (chance: {successChance * 100}%)");
        }
    }

    /// <summary>
    /// 일정 시간 후에 성공하는 조건
    /// </summary>
    [System.Serializable]
    public class TimerCondition : ICondition
    {
        [SerializeField] private float waitTime = 3f;
        private float elapsedTime;
        private bool hasStarted;
        
        public bool Evaluate(float deltaTime)
        {
            if (!hasStarted) return false;
            
            elapsedTime += deltaTime;
            return elapsedTime >= waitTime;
        }
        
        public void Start() 
        {
            elapsedTime = 0f;
            hasStarted = true;
            Debug.Log($"Timer started for {waitTime} seconds");
        }
        
        public void End() 
        {
            hasStarted = false;
        }
        
        public void OnSuccess() 
        {
            Debug.Log($"Timer completed after {elapsedTime:F1} seconds");
        }
    }
}