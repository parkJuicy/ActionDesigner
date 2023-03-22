using JuicyFlowChart;
using UnityEngine;

public class DebugAction : Action
{
    public string debugValue;
    protected override void Start()
    {
        Debug.Log("START");
    }

    protected override void Update()
    {
        Debug.Log(string.Format($"UPDATE : {debugValue}"));
    }
}
