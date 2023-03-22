using System.Collections.Generic;
using UnityEngine;

namespace JuicyFlowChart
{
    public abstract class Action : Task
    {
        protected abstract void Start();
        protected abstract void Update();

        public sealed override void Tick()
        {
            if (_state == State.Disable)
            {
                _state = State.Enable;
                Start();
            }

            Update();
            foreach (Task child in Children)
            {
                child.Tick();
            }
        }
    }
}