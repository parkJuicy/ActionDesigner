using UnityEngine;

namespace ActionDesigner.Runtime
{
    public class ActionRunner : MonoBehaviour
    {
        [SerializeReference]
        Action _action = new Action();

        public Action action { get => _action; }
    }
}