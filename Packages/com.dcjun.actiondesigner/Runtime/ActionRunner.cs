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
        bool isBehaviorCompleted;

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

            if (!(rootNode is BehaviorNode behaviorNode) || !behaviorNode.IsValid)
            {
                return;
            }

            currentState = ActionRunnerState.Running;
            currentNode = rootNode;
            currentNodeID = currentNode.id;
            isBehaviorCompleted = false;

            StartCurrentNode();
        }

        public void StopAction()
        {
            if (currentNode is BehaviorNode behaviorNode)
            {
                behaviorNode.behavior?.Stop();
                EndAllConditionsForBehavior(behaviorNode);
            }

            currentNode = null;
            currentNodeID = 0;
            isBehaviorCompleted = false;
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

            float deltaTime = Time.deltaTime;
            if (currentNode is BehaviorNode behaviorNode)
            {
                UpdateBehaviorWithTransitions(behaviorNode, deltaTime);
            }
        }

        void UpdateBehaviorWithTransitions(BehaviorNode behaviorNode, float deltaTime)
        {
            if (behaviorNode?.behavior == null) return;
            isBehaviorCompleted = behaviorNode.behavior.Update(deltaTime);
            BaseNode nextNode = EvaluateTransitions(behaviorNode, deltaTime);

            if (nextNode == null || isBehaviorCompleted && behaviorNode.childrenID.Count == 0)
            {
                CompleteAction(behaviorNode);
                return;
            }

            if (nextNode != currentNode)
            {
                if (isBehaviorCompleted)
                    behaviorNode.behavior.End();
                else
                    behaviorNode.behavior.Stop();
                EndAllConditionsForBehavior(behaviorNode);
                TransitionToNode(nextNode);
            }
        }

        BaseNode EvaluateTransitions(BehaviorNode behaviorNode, float deltaTime)
        {
            if (behaviorNode.childrenID.Count == 0) return null;

            foreach (var childID in behaviorNode.childrenID)
            {
                var conditionNode = action.FindNode(childID) as ConditionNode;
                if (conditionNode?.condition == null) continue;

                bool conditionSuccess = isBehaviorCompleted;
                if (conditionNode.condition is not EndCondition)
                {
                    conditionSuccess = conditionNode.condition.Evaluate(deltaTime);
                }

                if (conditionSuccess)
                {
                    conditionNode.condition.OnSuccess();
                    if (conditionNode.childrenID.Count == 0)
                        return null;

                    return GetNextBehaviorFromCondition(conditionNode);
                }
            }
            return behaviorNode;
        }

        BaseNode GetNextBehaviorFromCondition(ConditionNode conditionNode)
        {
            if (conditionNode.childrenID.Count > 0)
            {
                var nextNodeID = conditionNode.childrenID[0];
                var nextNode = action.FindNode(nextNodeID);
                if (nextNode is BehaviorNode behaviorNode && behaviorNode.IsValid)
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
            isBehaviorCompleted = false;
            StartCurrentNode();
        }

        void EndAllConditionsForBehavior(BehaviorNode behaviorNode)
        {
            foreach (var childID in behaviorNode.childrenID)
            {
                var conditionNode = action.FindNode(childID) as ConditionNode;
                if (conditionNode?.condition != null)
                {
                    conditionNode.condition.End();
                }
            }
        }

        void StartAllConditionsForBehavior(BehaviorNode behaviorNode)
        {
            foreach (var childID in behaviorNode.childrenID)
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

            if (currentNode is BehaviorNode behaviorNode)
            {
                behaviorNode.behavior?.Start();
                isBehaviorCompleted = false;
                StartAllConditionsForBehavior(behaviorNode);
            }
        }

        void OnDisable()
        {
            if (currentState != ActionRunnerState.Idle)
            {
                StopAction();
            }
        }

        void CompleteAction(BehaviorNode behaviorNode)
        {
            behaviorNode.behavior.End();
            EndAllConditionsForBehavior(behaviorNode);
            currentNode = null;
            currentNodeID = 0;
            isBehaviorCompleted = false;
            currentState = ActionRunnerState.Idle;
        }
    }
}