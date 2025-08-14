using UnityEngine;

namespace ActionDesigner.Runtime
{
    /// <summary>
    /// ActionRunner 상태 열거형
    /// </summary>
    public enum ActionRunnerState
    {
        Idle,
        ExecutingMotion,
        EvaluatingCondition,
        Completed,
        Stopped,
        Paused
    }

    /// <summary>
    /// Action을 실행하는 MonoBehaviour (Update 기반)
    /// Motion → Condition → Motion 체인 구조로 실행
    /// </summary>
    public class ActionRunner : MonoBehaviour
    {
        [SerializeReference, SubclassSelector]
        Action _action = new Action();

        [Header("Runtime Settings")]
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _loop = false;

        [Header("Debug Info")]
        [SerializeField] private int _currentNodeID = 0;
        [SerializeField] private bool _isRunning = false;
        [SerializeField] private ActionRunnerState _currentState = ActionRunnerState.Idle;

        private BaseNode _currentNode;
        private bool _motionCompleted = false;
        private float _motionStartTime;

        // Public Properties
        public Action action => _action;
        public bool isRunning => _isRunning;
        public int currentNodeID => _currentNodeID;
        public BaseNode currentNode => _currentNode;
        public ActionRunnerState currentState => _currentState;

        private void Start()
        {
            if (_autoStart && _action != null)
            {
                StartAction();
            }
        }

        private void Update()
        {
            if (!_isRunning || _currentNode == null) return;

            if (_currentNode is MotionNode motionNode && !_motionCompleted)
            {
                UpdateMotion();
            }
            else if (_currentNode is ConditionNode conditionNode)
            {
                UpdateCondition();
            }
        }

        /// <summary>
        /// Motion 업데이트 처리
        /// </summary>
        private void UpdateMotion()
        {
            var motionNode = _currentNode as MotionNode;
            if (motionNode?.motion == null) return;

            _currentState = ActionRunnerState.ExecutingMotion;

            // Motion 업데이트
            _motionCompleted = motionNode.motion.Update(this);

            if (_motionCompleted)
            {
                motionNode.motion.OnComplete(this);
                MoveToNextNode();
            }
        }

        /// <summary>
        /// Condition 업데이트 처리  
        /// </summary>
        private void UpdateCondition()
        {
            var conditionNode = _currentNode as ConditionNode;
            if (conditionNode?.condition == null) return;

            _currentState = ActionRunnerState.EvaluatingCondition;

            conditionNode.condition.Initialize(this);
            bool conditionMet = false;

            if (conditionNode.condition.IsWaitCondition)
            {
                // 대기형 조건: 조건이 만족될 때까지 매 프레임 체크
                conditionMet = conditionNode.condition.Evaluate(this);
                if (!conditionMet)
                {
                    return; // 다음 프레임에 다시 체크
                }
            }
            else
            {
                // 즉시 평가 조건
                conditionMet = conditionNode.condition.Evaluate(this);
            }

            conditionNode.condition.Cleanup(this);

            if (conditionMet)
            {
                // 조건 만족 시 다음 Motion으로 진행
                MoveToNextNode();
            }
            else
            {
                // 조건 불만족 시 처리
                HandleConditionFailed();
            }
        }

        /// <summary>
        /// Action 실행 시작
        /// </summary>
        public void StartAction()
        {
            var rootNode = _action?.GetRootNode();
            if (rootNode == null)
            {
                return;
            }

            if (!(rootNode is MotionNode motionNode) || !motionNode.IsValid)
            {
                return;
            }

            _isRunning = true;
            _currentNode = rootNode;
            _currentNodeID = _currentNode.id;
            _motionCompleted = false;
            _motionStartTime = Time.time;

            StartCurrentNode();
        }

        /// <summary>
        /// Action 실행 중단
        /// </summary>
        public void StopAction()
        {
            if (_isRunning && _currentNode is MotionNode motionNode && !_motionCompleted)
            {
                motionNode.motion?.Stop();
            }

            _isRunning = false;
            _currentNode = null;
            _currentNodeID = 0;
            _motionCompleted = false;
            _currentState = ActionRunnerState.Stopped;
        }

        /// <summary>
        /// Action 재시작 (루프용)
        /// </summary>
        private void RestartAction()
        {
            StartAction();
        }

        /// <summary>
        /// 현재 노드 시작
        /// </summary>
        private void StartCurrentNode()
        {
            if (_currentNode == null) return;

            if (_currentNode is MotionNode motionNode)
            {
                motionNode.motion?.Initialize(this);
                _motionStartTime = Time.time;
                _motionCompleted = false;
            }
        }

        /// <summary>
        /// 다음 노드로 이동
        /// </summary>
        private void MoveToNextNode()
        {
            var nextNode = GetNextNode(_currentNode);

            if (nextNode != null)
            {
                _currentNode = nextNode;
                _currentNodeID = _currentNode.id;
                StartCurrentNode();
            }
            else
            {
                // Action 완료
                HandleActionCompleted();
            }
        }

        /// <summary>
        /// Action 완료 처리
        /// </summary>
        private void HandleActionCompleted()
        {
            if (_loop)
            {
                RestartAction();
            }
            else
            {
                _isRunning = false;
                _currentNode = null;
                _currentNodeID = 0;
                _currentState = ActionRunnerState.Completed;
            }
        }

        /// <summary>
        /// 조건 실패 처리
        /// </summary>
        private void HandleConditionFailed()
        {
            if (_loop)
            {
                RestartAction();
            }
            else
            {
                StopAction();
            }
        }

        /// <summary>
        /// 다음 노드 반환
        /// </summary>
        private BaseNode GetNextNode(BaseNode node)
        {
            if (node.childrenID.Count > 0)
            {
                return _action.FindNode(node.childrenID[0]);
            }
            return null;
        }

        /// <summary>
        /// 현재 Motion의 진행 시간 반환
        /// </summary>
        public float GetMotionElapsedTime()
        {
            return Time.time - _motionStartTime;
        }

        /// <summary>
        /// Action 일시정지/재개
        /// </summary>
        public void PauseAction()
        {
            if (!_isRunning) return;

            _isRunning = !_isRunning;
            _currentState = _isRunning ? ActionRunnerState.ExecutingMotion : ActionRunnerState.Paused;
        }

        /// <summary>
        /// 에디터용 미리보기
        /// </summary>
        [ContextMenu("Preview Action")]
        public void PreviewAction()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action preview is only available in Play Mode");
                return;
            }
            StartAction();
        }

        /// <summary>
        /// 에디터용 액션 정지
        /// </summary>
        [ContextMenu("Stop Action")]
        public void StopActionFromMenu()
        {
            if (!Application.isPlaying) return;
            StopAction();
        }

        /// <summary>
        /// 에디터용 상태 리셋
        /// </summary>
        [ContextMenu("Reset State")]
        public void ResetState()
        {
            if (Application.isPlaying)
            {
                StopAction();
            }

            _currentNodeID = 0;
            _currentState = ActionRunnerState.Idle;
        }

        private void OnDisable()
        {
            if (_isRunning)
            {
                StopAction();
            }
        }

        private void OnDrawGizmos()
        {
            // 에디터에서 현재 상태 시각화
            if (_isRunning && _currentNode != null)
            {
                Gizmos.color = _currentNode is MotionNode ? Color.red : Color.blue;
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
            }
        }
    }
}
