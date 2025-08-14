using UnityEngine;
using ActionDesigner.Runtime.Conditions;

namespace ActionDesigner.Runtime
{
    public enum ActionRunnerState
    {
        Idle,
        Running,
        Paused
    }

    public class ActionRunner : MonoBehaviour
    {
        [SerializeReference] Action action = new Action();
        [SerializeField] bool autoStart = true;

        BaseNode currentNode;
        bool isMotionCompleted;

        public Action Action => action;
        public ActionRunnerState currentState { get; private set; }
        public int currentNodeID { get; private set; }

        void Start()
        {
            if (autoStart && action != null)
            {
                StartAction();
            }
        }

        public void StartAction()
        {
            var rootNode = action?.GetRootNode();
            if (rootNode == null)
            {
                return;
            }

            if (!(rootNode is MotionNode motionNode) || !motionNode.IsValid)
            {
                return;
            }

            currentState = ActionRunnerState.Running;
            currentNode = rootNode;
            currentNodeID = currentNode.id;
            isMotionCompleted = false;

            StartCurrentNode();
        }

        public void StopAction()
        {
            if (currentNode is MotionNode motionNode)
            {
                motionNode.motion?.Stop();
                EndAllConditionsForMotion(motionNode);
            }

            currentNode = null;
            currentNodeID = 0;
            isMotionCompleted = false;
            currentState = ActionRunnerState.Idle;
        }

        public void PauseAction()
        {
            if (currentState == ActionRunnerState.Running)
                currentState = ActionRunnerState.Paused;
            else if (currentState == ActionRunnerState.Paused)
                currentState = ActionRunnerState.Running;
        }

        void Update()
        {
            if (currentState != ActionRunnerState.Running || currentNode == null)
                return;

            if (currentNode is MotionNode motionNode)
            {
                UpdateMotionWithTransitions(motionNode);
            }
        }

        void UpdateMotionWithTransitions(MotionNode motionNode)
        {
            if (motionNode?.motion == null) return;
            isMotionCompleted = motionNode.motion.Update();
            BaseNode nextNode = EvaluateTransitions(motionNode);

            if (nextNode == null || isMotionCompleted && motionNode.childrenID.Count == 0)
            {
                CompleteAction(motionNode);
                return;
            }

            if (nextNode != currentNode)
            {
                if (isMotionCompleted)
                    motionNode.motion.End();
                else
                    motionNode.motion.Stop();
                EndAllConditionsForMotion(motionNode);
                TransitionToNode(nextNode);
            }
        }

        BaseNode EvaluateTransitions(MotionNode motionNode)
        {
            if (motionNode.childrenID.Count == 0) return null;

            foreach (var childID in motionNode.childrenID)
            {
                var conditionNode = action.FindNode(childID) as ConditionNode;
                if (conditionNode?.condition == null) continue;

                bool conditionSuccess = isMotionCompleted;
                if (conditionNode.condition is not EndCondition)
                {
                    conditionSuccess = conditionNode.condition.Evaluate();
                }

                if (conditionSuccess)
                {
                    conditionNode.condition.OnSuccess();
                    if (conditionNode.childrenID.Count == 0)
                        return null;

                    return GetNextMotionFromCondition(conditionNode);
                }
            }
            return motionNode;
        }

        BaseNode GetNextMotionFromCondition(ConditionNode conditionNode)
        {
            if (conditionNode.childrenID.Count > 0)
            {
                var nextNodeID = conditionNode.childrenID[0];
                var nextNode = action.FindNode(nextNodeID);
                if (nextNode is MotionNode motionNode && motionNode.IsValid)
                {
                    return nextNode;
                }
            }
            return null;
        }

        void TransitionToNode(BaseNode nextNode)
        {
            currentNode = nextNode;
            currentNodeID = currentNode.id;
            isMotionCompleted = false;
            StartCurrentNode();
        }

        void EndAllConditionsForMotion(MotionNode motionNode)
        {
            foreach (var childID in motionNode.childrenID)
            {
                var conditionNode = action.FindNode(childID) as ConditionNode;
                if (conditionNode?.condition != null)
                {
                    conditionNode.condition.End();
                }
            }
        }

        void StartAllConditionsForMotion(MotionNode motionNode)
        {
            foreach (var childID in motionNode.childrenID)
            {
                var conditionNode = action.FindNode(childID) as ConditionNode;
                if (conditionNode?.condition != null)
                {
                    conditionNode.condition.Start();
                }
            }
        }

        void StartCurrentNode()
        {
            if (currentNode == null) return;

            if (currentNode is MotionNode motionNode)
            {
                motionNode.motion?.Start();
                isMotionCompleted = false;
                StartAllConditionsForMotion(motionNode);
            }
        }

        void OnDisable()
        {
            if (currentState != ActionRunnerState.Idle)
            {
                StopAction();
            }
        }

        void CompleteAction(MotionNode motionNode)
        {
            motionNode.motion.End();
            EndAllConditionsForMotion(motionNode);
            currentNode = null;
            currentNodeID = 0;
            isMotionCompleted = false;
            currentState = ActionRunnerState.Idle;
        }
    }
}
