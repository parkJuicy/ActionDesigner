namespace JuicyFlowChart
{
    public class Root : Task
    {
        public override void Tick()
        {
            foreach (Task child in Children)
            {
                child.Tick();
            }
        }
    }
}