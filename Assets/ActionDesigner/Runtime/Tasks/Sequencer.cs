using System;
using UnityEngine;

namespace ActionDesigner.Runtime.Decorators
{
    [Serializable]
    public sealed class Sequencer : IMotion
    {
        [SerializeReference, SubclassSelector] IMotion[] motions;
        int currentIndex;

        public void Start()
        {
            currentIndex = 0;
            if (motions.Length > 0)
                motions[currentIndex].Start();
        }

        public bool Update(float deltaTime)
        {
            if (currentIndex >= motions.Length)
            {
                return true;
            }
            var currentMotion = motions[currentIndex];
            var success = currentMotion.Update(deltaTime);
            while (success)
            {
                motions[currentIndex].End();
                currentIndex++;
                if (currentIndex >= motions.Length)
                {
                    return true;
                }
                
                motions[currentIndex].Start();
                success = motions[currentIndex].Update(deltaTime);
            }
            return false;
        }

        public void End()
        {
            for(int i = currentIndex; i < motions.Length; i++)
            {
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
