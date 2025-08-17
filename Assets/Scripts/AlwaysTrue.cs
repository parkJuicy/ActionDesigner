namespace ActionDesigner.Runtime.Conditions
{
    [System.Serializable]
    public class AlwaysTrue : ICondition
    {
        public bool Evaluate(float deltaTime)
        {
            return true;
        }
    }
}
