using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionDesigner.Runtime.Decorators
{
    [Serializable]
    public sealed class Parallel : IBehavior
    {
        [SerializeReference, SubclassSelector] IBehavior[] behaviors;
        HashSet<int> completedIndex = new HashSet<int>();

        public void Start()
        {
            completedIndex.Clear();
            foreach (var behavior in behaviors)
            {
                behavior.Start();
            }
        }

        public bool Update(float deltaTime)
        {
            if (behaviors.Length == completedIndex.Count)
                return true;

            for (int i = 0; i < behaviors.Length; i++)
            {
                if (completedIndex.Contains(i))
                    continue;

                var behavior = behaviors[i];
                var success = behavior.Update(deltaTime);
                if (success)
                {
                    behavior.End();
                    completedIndex.Add(i);
                }
            }
            return false;
        }

        public void End()
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                if (completedIndex.Contains(i))
                    continue;
                
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
