using System.Collections.Generic;
using UnityEngine;

namespace JuicyFlowChart
{
    public abstract class Condition : Task
    {
        protected abstract bool Check();

        public sealed override void Tick()
        {
            if (Check())
            {
                _state = State.Enable;
                foreach (Task child in Children)
                {
                    child.Tick();
                }
            }
            else
            {
                if (_state == State.Enable)
                {
                    _state = State.Disable;
                    foreach (Task child in Children)
                    {
                        child.ChangeToDisableState();
                    }
                }
            }
        }
    }
}