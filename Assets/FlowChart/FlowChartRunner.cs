using UnityEngine;

namespace JuicyFlowChart
{
    public class FlowChartRunner : MonoBehaviour
    {
        [SerializeField]
        private FlowChart _flowChart;
        private Task _root;

        public FlowChart FlowChart { get => _flowChart; }
        public Task Root { get => _root; }

        private void Start()
        {
            if(_flowChart == null)
            {
                Debug.LogWarning("Not Found FlowChart");
                return;
            }

            _root = _flowChart.Clone(gameObject);
        }

        private void Update()
        {
            if (_root == null)
                return;

            _root.Tick();
        }
    }
}