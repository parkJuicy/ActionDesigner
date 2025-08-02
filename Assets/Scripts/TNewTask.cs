using ActionDesigner.Runtime;
using UnityEngine;

namespace deep
{
    public class TNewTask : Transition
    {
        [SerializeField] AnimationClip animationClip;
        [SerializeField, SerializeReference, SubclassSelector] Parent parent;
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public interface Parent
    {
        
    }
    
    [System.Serializable]
    public sealed class Child : Parent
    {
        public int value;
    }
    
    [System.Serializable]
    public sealed class AnotherChild : Parent
    {
        public float floatValue;
        public string name;
    }
    
    [System.Serializable]
    public sealed class ComplexChild : Parent
    {
        public Vector3 position;
        public Color color;
        public bool isActive;
    }
}