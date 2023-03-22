using System.Collections.Generic;
using UnityEngine;
using System;

namespace JuicyFlowChart
{
    [Serializable]
    public class Node
    {
        [HideInInspector]
        public string Name;
        [HideInInspector]
        public string Namespace;
        [HideInInspector]
        public string BaseType;
        [HideInInspector]
        public int ID;
        [HideInInspector]
        public string Data;
        [HideInInspector]
        public Vector2 Position;
        [HideInInspector]
        public List<int> ChildrenID = new List<int>();
        public Task Task;
    }
}