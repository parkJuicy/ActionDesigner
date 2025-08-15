using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionDesigner.Runtime.Decorators
{
    [Serializable]
    public sealed class Parallel : IMotion
    {
        [SerializeReference, SubclassSelector] IMotion[] motions;
        HashSet<int> completedIndex = new HashSet<int>();

        public void Start()
        {
            completedIndex.Clear();
            foreach (var motion in motions)
            {
                motion.Start();
            }
        }

        public bool Update(float deltaTime)
        {
            if (motions.Length == completedIndex.Count)
                return true;

            for (int i = 0; i < motions.Length; i++)
            {
                if (completedIndex.Contains(i))
                    continue;

                var motion = motions[i];
                var success = motion.Update(deltaTime);
                if (success)
                {
                    motion.End();
                    completedIndex.Add(i);
                }
            }
            return false;
        }

        public void End()
        {
            for (int i = 0; i < motions.Length; i++)
            {
                if (completedIndex.Contains(i))
                    continue;
                
                motions[i].End();
            }
        }

        public void Stop()
        {
            foreach (var motion in motions)
            {
                motion.Stop();
            }
        }
    }
}
