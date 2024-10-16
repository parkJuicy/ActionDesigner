using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ActionDesigner.Runtime
{
    [Serializable]
    public class Node
    {
        [HideInInspector]
        public string type;
        [HideInInspector]
        public string nameSpace;
        [HideInInspector]
        public int id;
        [HideInInspector]
        public Vector2 position;
        [HideInInspector]
        public List<int> childrenID = new List<int>();
        [SerializeReference]
        public Task task;
        [HideInInspector]
        public string baseType;
    }
}