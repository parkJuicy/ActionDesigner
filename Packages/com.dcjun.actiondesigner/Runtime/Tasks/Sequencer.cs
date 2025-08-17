using System;
using UnityEngine;

namespace ActionDesigner.Runtime.Decorators
{
    [Serializable]
    public sealed class Sequencer : IBehavior
    {
        [SerializeReference, SubclassSelector] IBehavior[] behaviors;
        int currentIndex;

        public void Start()
        {
            currentIndex = 0;
            if (behaviors.Length > 0)
                behaviors[currentIndex].Start();
        }

        public bool Update(float deltaTime)
        {
            if (currentIndex >= behaviors.Length)
            {
                return true;
            }
            var currentBehavior = behaviors[currentIndex];
            var success = currentBehavior.Update(deltaTime);
            while (success)
            {
                behaviors[currentIndex].End();
                currentIndex++;
                if (currentIndex >= behaviors.Length)
                {
                    return true;
                }
                
                behaviors[currentIndex].Start();
                success = behaviors[currentIndex].Update(deltaTime);
            }
            return false;
        }

        public void End()
        {
            for(int i = currentIndex; i < behaviors.Length; i++)
            {
                behaviors[i].End();
            }
        }

        public void Stop()
        {
            foreach (var behavior in behaviors)
            {
                behavior.Stop();
            }
        }
    }
}