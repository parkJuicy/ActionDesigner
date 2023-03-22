using System;
using System.Collections.Generic;
using UnityEngine;

namespace JuicyFlowChart
{    public abstract class Task
    {
        public enum State
        {
            Enable,
            Disable
        }

        protected State _state = State.Disable;
        protected GameObject gameObject;
        protected Transform transform;

        private List<Task> _children = new List<Task>();
        private int _nodeID;

        public State CurrentState { get => _state; }
        public List<Task> Children { get => _children; set => _children = value; }
        public int NodeID { get => _nodeID; set => _nodeID = value; }

        public abstract void Tick();

        internal void ChangeToDisableState()
        {
            _state = State.Disable;
            foreach (Task child in _children)
            {
                child.ChangeToDisableState();
            }
        }

        protected T GetComponent<T>() where T : MonoBehaviour
        {
            return gameObject.GetComponent<T>();
        }

        public void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
        }
    }
}